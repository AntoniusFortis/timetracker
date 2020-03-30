import React, { Component } from 'react';
import { Get, Delete } from '../../restManager';
import { Link } from 'react-router-dom';
import { NavLink } from 'reactstrap';

export class ProjectGet extends Component {
    constructor(props) {
        super(props);

        this.state = {
            project: null,
            loading: true,
            users: []
        };

        this.onRemoveProject = this.onRemoveProject.bind(this);
        this.onClickEditProject = this.onClickEditProject.bind(this);
        this.onClickAddTask = this.onClickAddTask.bind(this);
    }

    componentDidMount() {
        this.getProjectsData();
    }

    async getProjectsData() {
        Get("api/project/get?id=" + this.props.match.params.projectId, (response) => {
            response.json()
                .then(result => {
                    this.setState({
                        project: result.project,
                        loading: false,
                        users: result.users,
                        tasks: result.tasks
                    });
                });
        });
    }

    renderProjectsTable(project) {
        return (
            <table className='table table-striped' aria-labelledby="tabelLabel">
                <thead>
                    <tr>
                        <th>Название</th>
                        <th>Описание</th>
                    </tr>
                </thead>
                <tbody>
                    <tr key={project.Id}>
                        <td>{project.Title}</td>
                        <td>{project.Description}</td>
                    </tr>
                </tbody>
            </table>
        );
    }

    renderUsersTable(users) {
        return (
            <table className='table table-striped' aria-labelledby="tabelLabel">
                <thead>
                    <tr>
                        <th>Имя пользователя</th>
                    </tr>
                </thead>
                <tbody>
                    {
                        users.map(userName => (
                            <tr><td>{userName}</td></tr>
                        ))
                    }
                </tbody>
            </table>
        );
    }

    renderTasksTable(tasks) {
        return (
            <table className='table table-striped' aria-labelledby="tabelLabel">
                <thead>
                    <tr>
                        <th>Название</th>
                    </tr>
                </thead>
                <tbody>
                    {
                        tasks.map(task => (
                            <tr>
                                <td>
                                    <NavLink tag={Link} className="text-dark" to={"/task/get/" + task.Id}>{task.Title}</NavLink>
                                </td>
                            </tr>
                        ))
                    }
                </tbody>
            </table>
        );
    }

    onRemoveProject(e) {
        e.preventDefault();

        Delete("api/project/delete?Id=" + this.state.project.Id,
            { },
            (response) => {
                if (response.status === 200) {
                    window.location.href = "/project/all";
                }
            });
    }

    onClickEditProject(e) {
        window.location.href = "/project/update/" + this.state.project.Id;
    }

    onClickAddTask(e) {
        window.location.href = "/task/add/" + this.state.project.Id;
    }

    render() {
        const project = this.state.loading
            ? <p><em>Загрузка...</em></p>
            : this.renderProjectsTable(this.state.project);

        const users = this.state.loading
            ? <p><em>Загрузка...</em></p>
            : this.renderUsersTable(this.state.users);

        const tasks = this.state.loading
            ? <p><em>Загрузка...</em></p>
            : this.renderTasksTable(this.state.tasks);
        
        return (
            <div>
                <h6>Проект</h6>
                <button onClick={this.onClickEditProject}>Редактировать проект</button>
                <form onSubmit={this.onRemoveProject}>
                    <button>Удалить проект</button>
                </form>
                {project}
                <h6>Пользователи</h6>
                {users}
                <button onClick={this.onClickAddTask}>Добавить задачу</button>
                <h6>Задачи</h6>
                {tasks}
            </div>
        );
    }
}
