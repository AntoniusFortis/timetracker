import React, { Component } from 'react';
import { Get, Post } from '../../restManager';

const UserAdd = ({ userName, onRemove }) => (
    <div>
        <p><b>{userName}</b></p>
        <p><button onClick={onRemove}>Удалить</button></p>
    </div>
);

export class ProjectUpdate extends Component {

    constructor(props) {
        super(props);

        this.state = {
            project: null,
            title: "",
            description: "",
            users: [],
            user_input: ""
        };
    }

    componentDidMount() {
        this.getProjectsData();
    }

    onTitleChange = (event) => {
        this.setState({ title: event.target.value });
    }

    onDescriptionChange = (event) => {
        this.setState({ description: event.target.value });
    }

    onUserInputChange = (event) => {
        this.setState({ user_input: event.target.value });
    }

    async getProjectsData() {
        Get("api/project/get?id=" + this.props.match.params.projectId, (response) => {
            response.json()
                .then(result => {
                    this.setState({
                        project: result.project,
                        users: result.users,
                        title: result.project.Title,
                        description: result.project.Description
                    });
                });
        });
    }

    onSubmit = (event) => {
        event.preventDefault();
        const { description, title, project, users } = this.state;

        Post("api/project/update",
            { Project: { Id: project.Id, Title: title.trim(), Description: description.trim() }, Users: users },
            (response) => {
                if (response.status === 200) {
                    window.location.href = "/projects/get/" + this.props.match.params.projectId;
                }
            });
    }

    onRemoveUser = (event) => {
        const users = this.state.users;
        const idx = users.indexOf(event.Name);

        this.state.users.splice(idx);

        this.setState({ users: this.state.users });
    }

    onAddingUser = (event) => {
        event.preventDefault();

        const name = this.state.user_input;
        this.setState(prevState => {
            return {
                users: [...prevState.users, name]
            };
        });
    }

    render() {
        return (
            <form onSubmit={this.onSubmit}>
                <p>
                    <input type="text" placeholder="Название" value={this.state.title} onChange={this.onTitleChange} />
                </p>
                <p>
                    <input type="text" placeholder="Описание" value={this.state.description} onChange={this.onDescriptionChange} />
                </p>
                <input type="submit" value="Изменить проект" />

                <input type="text" placeholder="Имя пользователя" onChange={this.onUserInputChange} />
                <button onClick={this.onAddingUser}>Add user</button>

                <div>
                    {
                        this.state.users.map(userName => (
                            <UserAdd userName={userName} onRemove={this.onRemoveUser} />
                        ))
                    }
                </div>
            </form>
        );
    }
}
