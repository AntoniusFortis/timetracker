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

        this.onSubmit = this.onSubmit.bind(this);

        this.onTitleChange = this.onTitleChange.bind(this);
        this.onDescriptionChange = this.onDescriptionChange.bind(this);
        this.onAddingUser = this.onAddingUser.bind(this);
        this.onRemoveUser = this.onRemoveUser.bind(this);
        this.onUserInputChange = this.onUserInputChange.bind(this);
    }

    componentDidMount() {
        this.getProjectsData();
    }

    onTitleChange(e) {
        this.setState({ title: e.target.value.trim() });
    }

    onDescriptionChange(e) {
        this.setState({ description: e.target.value.trim() });
    }

    onUserInputChange(e) {
        this.setState({ user_input: e.target.value });
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

    onSubmit(e) {
        e.preventDefault();
        const { description, title, project, users } = this.state;

        Post("api/project/update",
            { Project: { Id: project.Id, Title: title, Description: description }, Users: users },
            (response) => {
                if (response.status === 200) {
                    window.location.href = "/projects/get/" + this.props.match.params.projectId;
                }
            });
    }

    onRemoveUser(data) {
        const users = this.state.users;
        const idx = users.indexOf(data.Name);

        this.state.users.splice(idx);

        this.setState({ users: this.state.users });
    }

    onAddingUser(e) {
        e.preventDefault();

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
                    <input type="text" placeholder="Title" value={this.state.title} onChange={this.onTitleChange} />
                </p>
                <p>
                    <input type="text" placeholder="Description" value={this.state.description} onChange={this.onDescriptionChange} />
                </p>
                <input type="submit" value="Update" />

                <input type="text" placeholder="user name" onChange={x => this.onUserInputChange(x)} />
                <button onClick={x => this.onAddingUser(x)}>Add user</button>

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
