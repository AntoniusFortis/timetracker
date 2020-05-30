import React, { PureComponent } from 'react';
import Select from 'react-select';
import { Get, Delete, Post } from '../../restManager';
import { TaskTracking } from '../TaskTracking'
import { Tabs, Tab } from '../../Tabs';

export class TaskGet extends PureComponent {
    constructor(props) {
        super(props);

        this.state = {
            worktask: {},
            loading: true,
            states: [],
            project: {},
            worktracks: [],
            isAdmin: true
        };
    }

    componentDidMount() {
        this.getTaskData()
            .then(this.getStatesData())
            .then(this.getWorktacksData());
    }

    async getTaskData() {
        Get("api/task/get?id=" + this.props.match.params.taskId, (response) => {
            response.json()
                .then(result => {
                    this.setState({ worktask: result.worktask, project: result.project, isAdmin: result.isAdmin });
                });
        });
    }

    async getWorktacksData() {
        Get("api/worktrack/getall?id=" + this.props.match.params.taskId, (response) => {
            response.json()
                .then(result => {
                    this.setState({ worktracks: result });
                });
        });
    }

    async getStatesData() {
        Get("api/state/getall", (response) => {
            response.json()
                .then(result => {
                    const states = result.states.map(state => {
                        return {
                            value: state.Id,
                            label: state.Title
                        }
                    });

                    this.setState({
                        states: states,
                        loading: false
                    });
                });
        });
    }

    renderTaskTable = (worktask) => {
        return (
            <table className='table table-striped' aria-labelledby="tabelLabel">
                <tbody>
                    <tr key={worktask.Id}>
                        <td>Описание: {worktask.Description}</td>
                        <td>Проект: {this.state.project.Title}</td>
                    </tr>
                    <tr key={worktask.Id}>
                        <td>Дата создания: {worktask.CreatedDate}</td>
                        <td>Часов: {worktask.Duration}</td>
                    </tr>
                    <tr key={worktask.Id}>
                        <td>Состояние:</td>
                        <td>
                            <Select options={this.state.states} defaultValue={this.state.states[worktask.StateId - 1]} onChange={this.onStateChange} />
                        </td>
                    </tr>
                </tbody>
            </table>
        );
    }

    renderWorktracksTable(worktracks) {
        return (
            <table className='table table-striped' aria-labelledby="tabelLabel">
                <thead>
                    <tr>
                        <th>Пользователь</th>
                        <th>Время начала</th>
                        <th>Время окончания</th>
                        <th>Затраченное время</th>
                    </tr>
                </thead>
                <tbody>
                    {
                        worktracks.map(worktrack =>
                            (
                                <tr key={worktrack.Id}>
                                    <td>{worktrack.User}</td>
                                    <td>{worktrack.StartedTime}</td>
                                    <td>{worktrack.StoppedTime}</td>
                                    <td>{worktrack.TotalTime}</td>
                                </tr>
                            )
                        )
                    }
                </tbody>
            </table>
        );
    }

    onStateChange = (event) => {

        const body = {
            taskId: this.state.worktask.Id.toString(),
            stateId: event.value.toString()
        }

        Post("api/task/UpdateState", body,
            (response) => {
                //if (response.status === 200) {
                //    window.location.href = "/task/get/" + this.props.match.params.taskId;
                //}
            });
    }

    onRemoveProject = (event) => {
        event.preventDefault();

        Delete("api/task/delete?Id=" + this.state.worktask.Id,
            {},
            (response) => {
                if (response.status === 200) {
                    window.location.href = "/project/get/" + this.state.worktask.Project.Id;
                }
            });
    }

    onClickEditProject = (event) => {
        window.location.href = '/task/update/' + this.state.worktask.Id;
    }

    render() {
        const worktask = this.state.loading
            ? <p><em>Загрузка...</em></p>
            : this.renderTaskTable(this.state.worktask);
        const worktracks = this.state.loading
            ? <p><em>Загрузка...</em></p>
            : this.renderWorktracksTable(this.state.worktracks);

        const removebutton = this.state.isAdmin ? <div style={{ display: "inline-block" }}>
            <form onSubmit={this.onRemoveProject}>
                <button>Удалить задачу</button>
            </form>
        </div> : (<div />);

        return (
            <div>
                <div style={{ display: "inline-block" }}>
                    <TaskTracking worktaskId={this.state.worktask.Id} />
                </div>
                <div style={{ display: "inline-block", paddingRight: "10px" }}>
                    <h4>Задача: { this.state.worktask.Title }</h4>
                </div>
                <div style={{ display: "inline-block" }}>
                    <button onClick={this.onClickEditProject}>Редактировать задачу</button>
                </div>
                {removebutton}

                <Tabs selectedTab={this.state.selectedTab} onChangeTab={selectedTab => this.setState({ selectedTab })}>
                    <Tab name="first" title="Описание задачи">
                        {worktask}
                    </Tab>
                    <Tab name="second" title="Затраченное время">
                        {worktracks}
                    </Tab>
                </Tabs>
            </div>
        );
    }
}
