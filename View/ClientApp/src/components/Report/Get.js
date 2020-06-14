import React, { PureComponent } from 'react';
import { Get, Post } from '../../restManager';
import Select from 'react-select';
import moment from 'moment'
import { NavLink } from 'reactstrap';
import { Link } from 'react-router-dom';
export class Report extends PureComponent {
    constructor(props) {
        super(props);

        this.state = {
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
                    const projects = result.acceptedProjects.map(project => {
                        return {
                            value: project.Id,
                            label: project.Title
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

        Post("api/worktrack/GetReport", body, (response) => {
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
                        isAdmin: result.caller.right.Id !== 1
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
        return ( worktracks.map(worktrack => {
                            const startedTime = moment(worktrack.startedTime).add(offset, 'm').format('YYYY-MM-DD HH:mm:ss');
                            const stop = moment(worktrack.stoppedTime).add(offset, 'm').format('YYYY-MM-DD hh:mm:ss');

                            return (
                                <tr key={worktrack.id}>
                                    <td>{worktrack.login}</td>
                                    <td><NavLink style={{ width: '250px', display: 'inline' }} tag={Link} to={"/task/get/" + worktrack.taskId}>{worktrack.task}</NavLink></td>
                                    <td>{startedTime}</td>
                                    <td>{stop}</td>
                                    <td>{worktrack.totalTime}</td>
                                </tr>
                            )
                } )
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
                <div>
                    <div>
                        <div style={{ width: '270px', display: 'inline-block', padding: '5px' }}>Проект: <Select options={this.state.projects} onChange={this.onProjectChange} /> </div>
                        <div style={{ width: '260px', display: 'inline-block', padding: '5px' }}>Пользователь: <Select isDisabled={this.state.projectId == 0} isClearable={true} options={this.state.users} onChange={this.onUserChange} /> </div>
                        <div style={{ width: '270px', display: 'inline-block', padding: '5px' }}>Задача: <Select isDisabled={this.state.projectId == 0} isClearable={true} options={this.state.tasks} onChange={this.onTaskChange} /> </div>
                    </div>
                    <div style={{ padding: '5px' }}>
                        <div> Даты: </div>
                        <div style={{ width: '170px', display: 'inline-block' }}>
                            <span>С </span>
                            <input type="date" onChange={this.onFromDate} />
                        </div>
                        <div style={{ width: '190px', display: 'inline-block' }}>
                            <label>по </label>
                            <input type="date" onChange={this.onEndDate} />
                        </div>
                    </div>
                    <hr />
                    <div style={{ marginBottom: '15px' }}>
                        <button disabled={!isReady} onClick={(e) => { this.getStat() }}>Запросить</button>
                    </div>
                </div>
                <div>
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
                            {htmlWorktracks}
                        </tbody>
                    </table>
                </div>
            </div>
        );
    }
}