import React, { PureComponent } from 'react';
import { Get, Delete } from '../../restManager';
import { Link } from 'react-router-dom';
import { NavLink } from 'reactstrap';
import { Tabs, Tab } from '../../Tabs';

const ProjectHeaderPanel = (props) => {
    return <div>
        <div style={{ display: "inline-block", paddingRight: "10px" }}>
            <h4>Проект: {props.Title}</h4>
        </div>
        <button onClick={props.onClickEditProject}>Редактировать проект</button>
        <div style={{ display: "inline-block", paddingRight: "10px" }}>
            <form onSubmit={props.onRemoveProject}>
                <button>Удалить проект</button>
            </form>
        </div>
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
                <ProjectHeaderPanel Title={this.state.project.Title} onClickEditProject={this.onClickEditProject} onRemoveProject={this.onRemoveProject} />
                <Tabs selectedTab={this.state.selectedTab} onChangeTab={selectedTab => this.setState({ selectedTab })}>
                    <Tab name="first" title="Задачи">
                        <WorktasksPanel onClickAddTask={this.onClickAddTask} onClickSortTasks={this.onClickSortTasks} onClickSortDefTasks={this.onClickSortDefTasks} orderFunc={this.state.orderTasksFunc(this.state.tasks.slice())} />
                    </Tab>
                    <Tab name="second" title="Участники">
                        <button onClick={this.onClickInviteProject}>Изменить участников</button>
                        {users}
                    </Tab>
                </Tabs>
            </div> 
        );
    }
}
