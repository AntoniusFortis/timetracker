import React, { PureComponent } from 'react';
import { Get, Delete, Post } from '../../restManager';
import { Link } from 'react-router-dom';
import { NavLink } from 'reactstrap';
import { Tabs, Tab } from '../../Tabs';
import Select from 'react-select';

const ProjectHeaderPanel = (props) => {
    return <div>
        <div style={{ display: "inline-block", paddingRight: "10px" }}>
            <h4>Проект: {props.Title}</h4>
        </div>
        {
            props.isAdmin && (<div> <button onClick={props.onClickEditProject}>Редактировать проект</button>
            <div style={{ display: "inline-block", paddingRight: "10px" }}>
                <form onSubmit={props.onRemoveProject}>
                    <button>Удалить проект</button>
                </form>
                </div>
            </div>)
        }
    </div>;
}

const WorktasksPanel = (props) => {
    return (<div>
        <button onClick={props.onClickAddTask}>Добавить задачу</button>
        <button onClick={props.onClickSortTasks}>Отсортировать по их состоянию</button>
        <button onClick={props.onClickSortDefTasks}>Сортировка по умолчанию</button>
        <TaskList tasks={props.orderFunc} />
    </div>);
}

const TaskList = ({ tasks }) => (
    <table className='table table-striped' aria-labelledby="tabelLabel">
        <thead>
            <tr>
                <th>Название</th>
                <th>Состояние</th>
            </tr>
        </thead>
        <tbody>
            {
                !!tasks && tasks.map(task => (
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
);

export class ProjectGet extends PureComponent {
    constructor(props) {
        super(props);

        this.state = {
            project: {},
            loading: true,
            users: [],
            tasks: [],
            isAdmin: true,
            orderTasksFunc: (tasks) => tasks
        };
    }

    options = [
        { value: '1', label: 'Администратор' },
        { value: '2', label: 'Пользователь' }
    ]

    componentDidMount() {
        this.getProjectsData()
            .then(this.getUsersData());
    }

    async getProjectsData() {
        Get("api/project/get?id=" + this.props.match.params.projectId, (response) => {
            response.json()
                .then(result => {
                    if (result.status == 401) {
                        window.location.href = "/error401";
                        return;
                    }

                    this.setState({
                        project: result.project,
                        loading: false,
                        tasks: result.tasks,
                        isAdmin: result.isAdmin
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

    onStateChange = (event, login) => {
        Post("api/project/UpdateUser",
            { userLogin: login, rightId: event.value, projectId: this.props.match.params.projectId },
            (response) => {
            });
    }

    onRemoveUser = (userName, userId) => {
        const users = this.state.users;
        const idx = users.findIndex((element) => { return element.login === userName });
        const newarr = users.splice(idx, 1);
        this.setState({ users: newarr }, () => {
            const body = {
                ProjectId: this.props.match.params.projectId,
                UserId: userId
            };
            Post("api/project/RemoveUserFromProject", body, (r) => { });
        });
    }

    renderUsersTable(users, isadmin) {
        return (
            <table className='table table-striped' aria-labelledby="tabelLabel">
                <thead>
                    <tr>
                        <th>Имя пользователя</th>
                        <th>Роль</th>
                        <th></th>
                    </tr>
                </thead>
                <tbody>
                    {
                        users.map(user => (
                            <tr>
                                <td>{user.login}</td>
                                <td>
                                    {isadmin && <Select options={this.options} defaultValue={this.options[user.right.Id - 1]} onChange={(event) => this.onStateChange(event, user.login)} /> }
                                    {!isadmin && user.right.Name}
                                </td>
                                <td>
                                    {isadmin && <button onClick={(e) => this.onRemoveUser(user.login, user.Id)}>Удалить</button>}
                                    {!isadmin && (<div />)}
                                </td>
                            </tr>
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
            : this.renderUsersTable(this.state.users, this.state.isAdmin);

        const adminRightes = this.state.isAdmin ? <button onClick={this.onClickInviteProject}>Добавить участников</button>: (<div />);

        return (
            <div>
                <ProjectHeaderPanel isAdmin={this.state.isAdmin} Id={this.state.project.Id} Title={this.state.project.Title} onClickEditProject={this.onClickEditProject} onRemoveProject={this.onRemoveProject} />
                <Tabs selectedTab={this.state.selectedTab} onChangeTab={selectedTab => this.setState({ selectedTab })}>
                    <Tab name="first" title="Задачи">
                        <WorktasksPanel onClickAddTask={this.onClickAddTask} onClickSortTasks={this.onClickSortTasks} onClickSortDefTasks={this.onClickSortDefTasks} orderFunc={this.state.orderTasksFunc(this.state.tasks.slice())} />
                    </Tab>
                    <Tab name="second" title="Участники">
                        {adminRightes}
                        {users}
                    </Tab>
                </Tabs>
            </div> 
        );
    }
}
