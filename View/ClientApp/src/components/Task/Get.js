import React, { Component } from 'react';
import { Get, Delete } from '../../restManager';

export class TaskGet extends Component {
    constructor(props) {
        super(props);

        this.state = {
            worktask: null,
            loading: true
        };

        this.onRemoveProject = this.onRemoveProject.bind(this);
        this.onClickEditProject = this.onClickEditProject.bind(this);
    }

    componentDidMount() {
        this.getProjectsData();
    }

    async getProjectsData() {
        Get("api/task/get?id=" + this.props.match.params.taskId, (response) => {
            response.json()
                .then(result => {
                    this.setState({
                        worktask: result.worktask,
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
                        <td>Состояние: {worktask.State.Title}</td>
                        <td>Проект: {worktask.Project.Title}</td>
                    </tr>
                </tbody>
            </table>
        );
    }

    onRemoveProject(e) {
        e.preventDefault();

        Delete("api/task/delete?Id=" + this.state.project.Id,
            {},
            (response) => {
                if (response.status === 200) {
                    window.location.href = "/project/get/" + this.state.project.Id;
                }
            });
    }

    onClickEditProject(e) {
        window.location.href = "/task/update/" + this.state.worktask.Project.Id;
    }

    render() {
        const worktask = this.state.loading
            ? <p><em>Загрузка...</em></p>
            : this.renderTaskTable(this.state.worktask);

        return (
            <div>
                <div style={{ display: "inline-block" }}>
                    <button onClick={this.onClickEditProject}>Редактировать задачу</button>
                </div>
                <div style={{ display: "inline-block"}}>
                    <form onSubmit={this.onRemoveProject}>
                        <button>Удалить задачу</button>
                    </form>
                </div>

                <h6>Задача</h6>
                {worktask}
            </div>
        );
    }
}
