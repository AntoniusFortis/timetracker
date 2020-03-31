import React, { Component } from 'react';
import Select from 'react-select';
import { Get, Delete, Post } from '../../restManager';

export class TaskGet extends Component {
    constructor(props) {
        super(props);

        this.state = {
            worktask: {},
            loading: true,
            states: [],
            project: {}
        };

        this.onRemoveProject = this.onRemoveProject.bind(this);
        this.onClickEditProject = this.onClickEditProject.bind(this);
        this.onStateChange = this.onStateChange.bind(this);
        this.renderTaskTable = this.renderTaskTable.bind(this);
    }

    componentDidMount() {
        this.getTaskData().then(this.getStatesData());
    }

    async getTaskData() {
        Get("api/task/get?id=" + this.props.match.params.taskId, (response) => {
            response.json()
                .then(result => {
                    this.setState({ worktask: result.worktask, project: result.project });
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

    renderTaskTable(worktask) {
        return (
            <table className='table table-striped' aria-labelledby="tabelLabel">
                <tbody>
                    <tr key={worktask.Id}>
                        <td>Название: {worktask.Title}</td>
                        <td>Описание: {worktask.Description}</td>
                    </tr>
                    <tr key={worktask.Id}>
                        <td>Дата создания: {worktask.CreatedDate}</td>
                        <td>Часов: {worktask.Duration}</td>
                    </tr>
                    <tr key={worktask.Id}>
                        <td>Состояние:
                            <Select options={this.state.states} defaultValue={this.state.states[worktask.StateId - 1]} onChange={this.onStateChange} />
                        </td>
                        <td>Проект: {this.state.project.Title}</td>
                    </tr>
                </tbody>
            </table>
        );
    }

    onStateChange(e) {
        const { worktask } = this.state;

        worktask.State = {
            Id: e.value,
            Title: e.label
        }
        worktask.StateId = e.Id;

        Post("api/task/update",
            { worktask: worktask },
            (response) => {
                if (response.status === 200) {
                    alert("Статус задания изменён");
                    //window.location.href = "/task/get/" + this.props.match.params.taskId;
                }
            });
    }

    onRemoveProject(e) {
        e.preventDefault();

        Delete("api/task/delete?Id=" + this.state.worktask.Id,
            {},
            (response) => {
                if (response.status === 200) {
                    window.location.href = "/project/get/" + this.state.worktask.Project.Id;
                }
            });
    }

    onClickEditProject(e) {
        window.location.href = "/task/update/" + this.state.worktask.Id;
    }

    render() {
        const worktask = this.state.loading
            ? <p><em>Загрузка...</em></p>
            : this.renderTaskTable(this.state.worktask);

        return (
            <div>
                <div style={{ display: "inline-block", paddingRight: "10px" }}>
                    <h4>Задача</h4>
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
            </div>
        );
    }
}
