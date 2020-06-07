import React, { Component } from 'react';
import { Get, Delete, Post } from '../../restManager';
import { Link } from 'react-router-dom';
import { NavLink } from 'reactstrap';
import { Tabs, Tab } from '../../Tabs';
import Select from 'react-select';
const editIcon = require('../editProject.png');
const deleteIcon = require('../deleteProject.png');

const ProjectHeaderPanel = (props) => {
    const removebutton = props.isAdmin ? <div style={{ display: "inline-block", margin: '5px' }}>
        <form onSubmit={props.onRemoveProject}>
            <button style={{ border: 'none', paddingLeft: '2px' }}><img src={deleteIcon} style={{ marginBottom: '3px' }} width="28"></img><span>Удалить</span></button>
        </form>
    </div> : (<div />);

    return <div>
        <div style={{ display: 'block' }}>
            <h4>Проект: {props.Title}</h4>
        </div>
        {removebutton}
        {
            props.isAdmin && <button onClick={props.onClickEditProject}>Редактировать проект</button>
        }
    </div>;
}

const WorktasksPanel = (props) => {
    return (<div>
        <NavLink style={{ width: '250px', display: 'inline' }} tag={Link} to={"/task/add/" + props.projectId}>Добавить задачу</NavLink>
        <TaskList tasks={props.tasks} />
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

export class ProjectGet extends Component {
    projectId = null;
    options = [
        { value: '1', label: 'Администратор' },
        { value: '2', label: 'Пользователь' }
    ]

    constructor(props) {
        super(props);

        this.state = {
            project: {},
            loading: true,
            users: [],
            caller: {},
            tasks: [],
            isAdmin: true,
            orderTasksFunc: (tasks) => tasks
        };

        this.projectId = this.props.match.params.projectId;
    }

    componentDidMount() {
        this.getProjectsData();
    }

    async getProjectsData() {
        Get('api/project/get?id=' + this.projectId, (response) => {
            response.json()
                .then(result => {
                    if (result.status == 401) {
                        window.location.href = '/error401';
                        return;
                    }

                    this.setState({
                        project: result.project,
                        loading: false,
                        tasks: result.tasks,
                        caller: result.caller,
                        users: result.users
                    });
                });
        });
    }


    onStateChange = (event, login) => {
        Post("api/project/UpdateUser", { userLogin: login, rightId: event.value, projectId: this.props.match.params.projectId });
    }

    onRemoveUser = (userId) => {
        let users = this.state.users;
        const idx = users.findIndex((element) => { return element.Id === userId });
        users.splice(idx, 1);
        this.setState({ users: users }, () => {
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
                                    {isadmin && user.right.Id !== 3 && <Select options={this.options} defaultValue={this.options[user.right.Id - 1]} onChange={(event) => this.onStateChange(event, user.login)} />}
                                    {(!isadmin || user.right.Id === 3) && user.right.Title}
                                </td>
                                <td>
                                    {isadmin && user.right.Id !== 3 && <button onClick={(e) => this.onRemoveUser(user.Id)}>Удалить</button>}
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

        Delete("api/project/delete?Id=" + this.props.match.params.projectId,
            { },
            (response) => {
                if (response.status === 200) {
                    window.location.href = "/project/all";
                }
            });
    }

    onClickEditProject = (event) => {
        window.location.href = "/project/update/" + this.props.match.params.projectId;
    }

    onClickInviteProject = (event) => {
        window.location.href = "/project/invite/" + this.props.match.params.projectId;
    }


    render() {
        if (this.state.loading) {
            return <div />;
        }

        const users = this.renderUsersTable(this.state.users, this.state.caller.right.Id !== 1);

        const adminRightes = this.state.caller.right.Id !== 1 ? <button onClick={this.onClickInviteProject}>Добавить участников</button> : (<div />);

        return (
            <div>
                <ProjectHeaderPanel isAdmin={this.state.isAdmin} Id={this.state.project.Id} Title={this.state.project.Title} onClickEditProject={this.onClickEditProject} onRemoveProject={this.onRemoveProject} />
                <Tabs selectedTab={this.state.selectedTab} onChangeTab={selectedTab => this.setState({ selectedTab })}>
                    <Tab name="first" title="Задачи">
                        <WorktasksPanel projectId={this.props.match.params.projectId} tasks={this.state.tasks} />
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
