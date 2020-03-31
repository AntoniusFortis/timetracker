import React, { Component } from 'react';
import { Get, Delete } from '../../restManager';
import { Link } from 'react-router-dom';
import { NavLink } from 'reactstrap';

export class TaskList extends Component {
    constructor(props) {
        super(props);
    }

    render() {
        const orderedtasks = this.props.orderFunc(this.props.tasks.slice());

        return (
            <table className='table table-striped' aria-labelledby="tabelLabel">
                <thead>
                    <tr>
                        <th>Название</th>
                        <th>Состояние</th>
                    </tr>
                </thead>
                <tbody>
                    {
                        !!orderedtasks && orderedtasks.map(task => (
                            <tr>
                                <td>
                                    <NavLink tag={Link} className="text-dark" to={"/task/get/" + task.Id}>{task.Title}</NavLink>
                                </td>
                                <td>{task.State.Title}</td>
                            </tr>
                        ))
                    }
                </tbody>
            </table>
        )
    }
}

export class ProjectGet extends Component {
    constructor(props) {
        super(props);

        this.state = {
            project: {},
            loading: true,
            users: [],
            tasks: [],
            orderTasksFunc: (tasks) => tasks
        };

        this.onRemoveProject = this.onRemoveProject.bind(this);
        this.onClickEditProject = this.onClickEditProject.bind(this);
        this.onClickAddTask = this.onClickAddTask.bind(this);
        this.onClickSortTasks = this.onClickSortTasks.bind(this);
        this.onClickSortDefTasks = this.onClickSortDefTasks.bind(this);
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

    onClickSortTasks(e) {
        this.setState({ orderTasksFunc: (tasks) => tasks.sort((a, b) => a.StateId >= b.StateId ? 1 : -1) });
    }

    onClickSortDefTasks(e) {
        this.setState({ orderTasksFunc: (tasks) => tasks });
    }

    render() {
        const users = this.state.loading
            ? <p><em>Загрузка...</em></p>
            : this.renderUsersTable(this.state.users);
        
        return (
            <div>
                <h4>Проект: {this.state.project.Title}</h4>
                <button onClick={this.onClickEditProject}>Редактировать проект</button>
                <form onSubmit={this.onRemoveProject}>
                    <button>Удалить проект</button>
                </form>
                <h6>Пользователи</h6>
                {users}
                <button onClick={this.onClickAddTask}>Добавить задачу</button>
                <h6>Задачи</h6>
                <button onClick={this.onClickSortTasks}>Отсортировать по их состоянию</button>
                <button onClick={this.onClickSortDefTasks}>Сортировка по умолчанию</button>

                <TaskList tasks={this.state.tasks} orderFunc={this.state.orderTasksFunc} />
            </div>
        );
    }
}
