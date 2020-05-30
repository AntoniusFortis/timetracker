import React, { PureComponent } from 'react';
import { Get, Post } from '../../restManager';
import Select from 'react-select';

export class Today extends PureComponent {
    constructor(props) {
        super(props);

        this.state = {
            loading: true,
            projects: [],
            projectId: 0,
            users: [],
            userId: 0,
            startDate: '',
            endDate: '',
            taskId: 0,
            tasks: [],
            isAdmin: true,
            worktracks: []
        };
    }

    componentDidMount() {
        this.getProjects();
    }

    async getProjects() {
        Get('api/project/getall', (response) => {
            response.json()
                .then(result => {
                    const projects = result.SignedProjects.map(proj => {
                        return {
                            value: proj.Id,
                            label: proj.Title
                        }
                    });

                    this.setState({ projects: projects });
                });
        });
    }

    async getUsers() {
        if (this.state.isAdmin) {
            Get('api/project/GetUsers?id=' + this.state.projectId, (response) => {
                response.json()
                    .then(result => {
                        const users = result.users.map(user => {
                            return {
                                value: user.Id,
                                label: user.login
                            }
                        });

                        this.setState({ users: users });
                    });
            });
        }
        else {
            Get('api/account/GetCurrentUser', (response) => {
                response.json()
                    .then(result => {
                        const users = [{
                            value: result.user.Id,
                            label: result.user.Login
                        }];

                        this.setState({ users: users });
                    });
            });
        }
    }

    async getStat() {
        const { projectId, taskId, userId, startDate, endDate } = this.state;

        const body = {
            projectId: projectId,
            userId: userId,
            startDate: startDate,
            endDate: endDate,
            taskId: taskId
        }

        Post("api/worktrack/GetStat", body, (response) => {
            response.json()
                .then(result => {
                    this.setState({ worktracks: result });
                });
        });
    }

    onProjectChange = (event) => {
        Get("api/project/get?id=" + event.value, (response) => {
            response.json()
                .then(result => {
                    if (result.status == 401) {
                        window.location.href = "/error401";
                        return;
                    }
                    const tasks = result.tasks.map(proj => {
                        return {
                            value: proj.Id,
                            label: proj.Title
                        }
                    });
                    this.setState({
                        projectId: event.value,
                        tasks: tasks,
                        isAdmin: result.isAdmin
                    }, () => { this.getUsers() });
                });
        });
    }

    onUserChange = (event) => {
        if (event) {
            this.setState({ userId: event.value });
        }
        else {
            this.setState({ userId: 0 });
        }
    }

    onTaskChange = (event) => {
        if (event) {
            this.setState({ taskId: event.value });
        }
        else {
            this.setState({ taskId: 0 });
        }
    }

    renderWorktracksTable(worktracks) {
        const offset = moment().utcOffset();

        return (
            <table className='table table-striped' aria-labelledby="tabelLabel">
                <thead>
                    <tr>
                        <th>Пользователь</th>
                        <th>Задача</th>
                        <th>Время начала</th>
                        <th>Время окончания</th>
                        <th>Затраченное время</th>
                    </tr>
                </thead>
                <tbody>
                    {
                        worktracks.map(worktrack => {
                            const start = moment(worktrack.StartedTime).add(offset, 'm').format('HH:mm:ss');
                            const stop = moment(worktrack.StoppedTime).add(offset, 'm').format('HH:mm:ss');

                            return  (
                                <tr key={worktrack.Id}>
                                    <td>{worktrack.User}</td>
                                    <td>{worktrack.Task}</td>
                                    <td>{start}</td>
                                    <td>{stop}</td>
                                    <td>{worktrack.TotalTime}</td>
                                </tr>
                            )
                        }
                        )
                    }
                </tbody>
            </table>
        );
    }

    onFromDate = (event) => {
        this.setState({ startDate: event.target.value });
    }

    onEndDate = (event) => {
        this.setState({ endDate: event.target.value });
    }

    render() {
        const { projectId, startDate, endDate, worktracks } = this.state;
        const isReady = projectId != 0 && startDate != '' && endDate != '' && endDate >= startDate;
        const htmlWorktracks = worktracks.length > 0 ? this.renderWorktracksTable(worktracks) : (<div />);
        return (
            <div>
                <div style={{ width: '490px' }}>
                    <div>
                        <Select options={this.state.projects} onChange={this.onProjectChange} />
                        <Select isClearable={true} options={this.state.users} onChange={this.onUserChange} />
                        <Select isClearable={true} options={this.state.tasks} onChange={this.onTaskChange} />
                    </div>
                    <p>
                        <label>С </label>
                        <input type="date" onChange={this.onFromDate} />
                    </p>
                    <p>
                        <label>по </label>
                        <input type="date" onChange={this.onEndDate} />
                    </p>
                    <button disabled={!isReady} onClick={(e) => { this.getStat() }}>Запросить</button>
                </div>
                <div>
                    {htmlWorktracks}
                </div>
            </div>
        );
    }
}