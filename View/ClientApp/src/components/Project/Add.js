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

    onSubmit = (event) => {
        event.preventDefault();

        const { title, description, users } = this.state;

        if (!title) {
            return;
        }

        Post("api/project/add",
            { Project: { Title: title.trim(), Description: description }, Users: users },
            (response) => {
                if (response.status === 200) {
                    window.location.href = "/project/all";
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
                <input type="submit" value="Создать проект" />

                <input type="text" placeholder="user name" onChange={this.onUserInputChange} />
                <button onClick={this.onAddingUser}>Добавить пользователя</button>

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