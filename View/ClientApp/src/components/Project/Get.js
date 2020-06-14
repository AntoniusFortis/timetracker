import React, { Component } from 'react';
import { Get, Delete, Post } from '../../restManager';
import { Link } from 'react-router-dom';
import { NavLink } from 'reactstrap';
import { Tabs, Tab } from '../../Tabs';
import Select from 'react-select';
import { Redirect } from 'react-router';
const addUser = require('../addUser.png');
const addTaskIcon = require('../addTask.png');
const editIcon = require('../editProject.png');
const deleteIcon = require('../deleteProject.png');

const ProjectHeaderPanel = (props) => {
    const removebutton = props.isAdmin ? <div style={{ display: "inline-block", margin: '5px' }}>
        <form onSubmit={props.onRemoveProject}>
            <button style={{ border: 'none', paddingLeft: '2px' }}><img src={deleteIcon} style={{ marginBottom: '3px' }} width="28"></img><span>Удалить</span></button>
        </form>
    </div> : (<div />);

    const changeButton = props.isAdmin ? <div style={{ display: "inline-block", margin: '5px' }}>
        <button onClick={props.onClickEditProject} style={{ border: 'none', paddingLeft: '2px' }}><img src={editIcon} style={{ marginBottom: '3px' }} width="28"></img><span>Редактировать</span></button>
    </div> : (<div />);

    const adminRightes = props.isAdmin ? <div style={{ display: "inline-block", margin: '5px' }}>
            <button onClick={props.onClickInviteProject} style={{ border: 'none', paddingLeft: '2px' }}><img src={addUser} style={{ marginBottom: '3px' }} width="28"></img><span>Добавить участников</span></button>
        </div> : (<div />);

    return <div>
        <div style={{ display: 'block' }}>
            <h4>Проект: {props.Title}</h4>
        </div>
        {removebutton}
        {changeButton}
        <div style={{ display: "inline-block", margin: '5px' }}>
            <button onClick={props.onClickAddTask} style={{ border: 'none', paddingLeft: '2px' }}><img src={addTaskIcon} style={{ marginBottom: '3px' }} width="28"></img><span>Добавить задачу</span></button>
        </div>
        {adminRightes}
    </div>;
}

const WorktasksPanel = (props) => {
    return (<div>
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
        { value: '1', label: 'Пользователь' },
        { value: '2', label: 'Администратор' }
    ]

    constructor(props) {
        super(props);

        this.state = {
            project: {},
            loading: true,
            users: [],
            caller: {},
            tasks: [],
            referrer: null
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
                    this.setState({ referrer: '/project/all' });
                }
            });
    }

    onClickEditProject = (event) => {
        this.setState({ referrer: "/project/update/" + this.props.match.params.projectId  });
    }

    onClickAddTask = (event) => {
        this.setState({ referrer: "/task/add/" + this.props.match.params.projectId });
    }

    onClickInviteProject = (event) => {
        this.setState({ referrer: "/project/invite/" + this.props.match.params.projectId });
    }

    render() {
        if (this.state.loading) {
            return <div />;
        }

        const users = this.renderUsersTable(this.state.users, this.state.caller.right.Id !== 1);

        return (
            <div>
                {this.state.referrer && <Redirect to={this.state.referrer} />}
                <ProjectHeaderPanel isAdmin={this.state.caller.right.Id !== 1} Id={this.state.project.Id} onClickInviteProject={this.onClickInviteProject} onClickAddTask={this.onClickAddTask} Title={this.state.project.Title} onClickEditProject={this.onClickEditProject} onRemoveProject={this.onRemoveProject} />
                <Tabs selectedTab={this.state.selectedTab} onChangeTab={selectedTab => this.setState({ selectedTab })}>
                    <Tab name="first" title="Задачи">
                        <WorktasksPanel projectId={this.props.match.params.projectId} tasks={this.state.tasks} />
                    </Tab>
                    <Tab name="second" title="Участники">
                        {users}
                    </Tab>
                </Tabs>
            </div> 
        );
    }
}
