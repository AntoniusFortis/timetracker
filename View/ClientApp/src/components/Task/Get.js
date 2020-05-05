import React, { Component } from 'react';
import Select from 'react-select';
import { Get, Delete, Post } from '../../restManager';
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr';

export class TaskTracking extends Component {
    constructor(props) {
        super(props);

        this.state = {
            hubConnection: null,
            buttonToggle: true
        };
    }

    showMessage(text)
    {
        if (text.trim() !== '') {
            alert(text);
        }
    }

    componentDidMount() {
        const hubConnection = new HubConnectionBuilder()
            .withUrl("/tracking", { accessTokenFactory: () => localStorage.getItem('tokenKey') })
            .configureLogging(LogLevel.Information)
            .build();

        this.setState({ hubConnection }, () => {
            this.state.hubConnection.start()
                .catch(err => console.log(err));

            this.state.hubConnection.on('StartTracking', (receivedMessage, status) => {
                this.setState({ buttonToggle: false });
                this.showMessage(receivedMessage);
            });

            this.state.hubConnection.on('StopTracking', (receivedMessage, status) => {
                this.setState({ buttonToggle: true });
                this.showMessage(receivedMessage);
            });

            this.state.hubConnection.on('GetActiveTracking', (istracking) => {
                this.setState({ buttonToggle: !istracking });
            });
        });

    }

    getActiveTracking = () => {
        this.state.hubConnection
            .invoke('GetActiveTracking')
            .catch(err => {
                console.error(err);
                this.setState({ buttonToggle: true });
            });
    }

    startTracking = () => {
        this.state.hubConnection
            .invoke('StartTracking', this.props.worktaskId)
            .catch(err => {
                console.error(err);
                this.setState({ buttonToggle: true });
            });
    };

    stopTracking = () => {
        this.state.hubConnection
            .invoke('StopTracking')
            .catch(err => {
                console.error(err);
                this.setState({ buttonToggle: true });
            });
    };

    render() {
        return (
            <div>
                <div style={{ display: (this.state.buttonToggle ? 'block' : 'none') }}>
                    <button onClick={this.startTracking}>Начать отслеживание</button>
                </div>
                <button style={{ display: (this.state.buttonToggle ? 'none' : 'block') }} onClick={this.stopTracking}>Остановить отслеживание</button>
            </div>);
    }
}

export class TaskGet extends Component {
    constructor(props) {
        super(props);

        this.state = {
            worktask: {},
            loading: true,
            states: [],
            project: {},
            worktracks: []
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
                    this.setState({ worktask: result.worktask, project: result.project });
                });
        });
    }

    async getWorktacksData() {
        Get("api/worktrack/getall?worktaskId=" + this.props.match.params.taskId, (response) => {
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
                    <tr>
                        <TaskTracking worktaskId={worktask.Id} />
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
        const { worktask } = this.state;

        worktask.State = {
            Id: event.value,
            Title: event.label
        }
        worktask.StateId = event.Id;

        Post("api/task/update",
            { worktask: worktask },
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
        window.location.href = "/task/update/" + this.state.worktask.Id;
    }

    render() {
        const worktask = this.state.loading
            ? <p><em>Загрузка...</em></p>
            : this.renderTaskTable(this.state.worktask);
        const worktracks = this.state.loading
            ? <p><em>Загрузка...</em></p>
            : this.renderWorktracksTable(this.state.worktracks);

        return (
            <div>
                <div style={{ display: "inline-block", paddingRight: "10px" }}>
                    <h4>Задача {this.state.worktask.Id}: { this.state.worktask.Title }</h4>
                </div>
                <div style={{ display: "inline-block" }}>
                    <button onClick={this.onClickEditProject}>Редактировать задачу</button>
                </div>
                <div style={{ display: "inline-block"}}>
                    <form onSubmit={this.onRemoveProject}>
                        <button>Удалить задачу</button>
                    </form>
                </div>
                {worktask}
                {worktracks}
            </div>
        );
    }
}
