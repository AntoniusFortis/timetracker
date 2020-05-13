import React, { Component } from 'react';
import { Get, Delete } from '../../restManager';
import { Link } from 'react-router-dom';
import { NavLink } from 'reactstrap';
import { Tabs, Tab } from '../../Tabs';

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
    }

    componentDidMount() {
        this.getProjectsData()
            .then(this.getUsersData());
    }

    async getProjectsData() {
        Get("api/project/get?id=" + this.props.match.params.projectId, (response) => {
            response.json()
                .then(result => {
                    this.setState({
                        project: result.project,
                        loading: false,
                        tasks: result.tasks
                    });
                });
        });
    }

    async getUsersData() {
        Get("api/project/getusers?id=" + this.props.match.params.projectId, (response) => {
            response.json()
                .then(result => this.setState({ users: result.users }));
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

    onRemoveProject = (event) => {
        event.preventDefault();

        Delete("api/project/delete?Id=" + this.state.project.Id,
            { },
            (response) => {
                if (response.status === 200) {
                    window.location.href = "/project/all";
                }
            });
    }

    onClickEditProject = (event) => {
        window.location.href = "/project/update/" + this.state.project.Id;
    }

    onClickInviteProject = (event) => {
        window.location.href = "/project/invite/" + this.state.project.Id;
    }

    onClickAddTask = (event) => {
        window.location.href = "/task/add/" + this.state.project.Id;
    }

    onClickSortTasks = (event) => {
        this.setState({ orderTasksFunc: (tasks) => tasks.sort((a, b) => a.StateId >= b.StateId ? 1 : -1) });
    }

    onClickSortDefTasks = (event) => {
        this.setState({ orderTasksFunc: (tasks) => tasks });
    }

    render() {
        const users = this.state.loading
            ? <p><em>Загрузка...</em></p>
            : this.renderUsersTable(this.state.users);
        
        return (
            <div>
                <div style={{ display: "inline-block", paddingRight: "10px" }}>
                    <h4>Проект: {this.state.project.Title}</h4>
                </div>
                <button onClick={this.onClickEditProject}>Редактировать проект</button>
                <button onClick={this.onClickInviteProject}>Изменить участников</button>
                <div style={{ display: "inline-block", paddingRight: "10px" }}>
                    <form onSubmit={this.onRemoveProject}>
                        <button>Удалить проект</button>
                    </form>
                </div>

                <Tabs selectedTab={this.state.selectedTab} onChangeTab={selectedTab => this.setState({ selectedTab })}>
                    <Tab name="first" title="Задачи">
                        <button onClick={this.onClickAddTask}>Добавить задачу</button>
                        <button onClick={this.onClickSortTasks}>Отсортировать по их состоянию</button>
                        <button onClick={this.onClickSortDefTasks}>Сортировка по умолчанию</button>
                        <TaskList tasks={this.state.tasks} orderFunc={this.state.orderTasksFunc} />
                    </Tab>
                    <Tab name="second" title="Участники">
                        {users}
                    </Tab>
                </Tabs>
            </div> 
        );
    }
}
