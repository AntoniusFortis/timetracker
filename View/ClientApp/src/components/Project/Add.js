import React, { Component } from 'react';
import { Post } from '../../restManager';

const UserAdd = ({ userName, onRemove }) => (
    <div>
        <p><b>{userName}</b></p>
        <p><button onClick={onRemove}>Удалить</button></p>
    </div>
);

export class ProjectAdd extends Component {

    constructor(props) {
        super(props);

        this.state = {
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
    }

    onTitleChange(e) {
        this.setState({ title: e.target.value.trim() });
    }

    onDescriptionChange(e) {
        this.setState({ description: e.target.value.trim()  });
    }

    onUserInputChange(e) {
        this.setState({ user_input: e.target.value.trim()  });
    }

    onSubmit(e) {
        e.preventDefault();

        const { title, description, users } = this.state;

        if (!title) {
            return;
        }

        Post("api/project/add",
            { Project: { Title: title, Description: description }, Users: users },
            (response) => {
                if (response.status === 200) {
                    window.location.href = "/project/all";
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
                    <input type="text" placeholder="Description" value={this.state.description} onChange={x => this.onDescriptionChange(x)} />
                </p>
                <input type="submit" value="Add" />

                <input type="text" placeholder="user name" onChange={x => this.onUserInputChange(x)} />
                <button onClick={this.onAddingUser}>Add user</button>

                <div>
                    {
                        this.state.users.map(name => (
                            <UserAdd userName={name} onRemove={this.onRemoveUser} />
                        ))
                    }
                </div>
            </form>
        );
    }
}