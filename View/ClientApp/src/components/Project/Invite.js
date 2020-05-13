import React, { Component } from 'react';
import { Get, Post } from '../../restManager';

const UserAdd = ({ userName, onRemove }) => (
    <div>
        <p><b>{userName}</b></p>
        <p><button onClick={onRemove}>Удалить</button></p>
    </div>
);

export class ProjectInvite extends Component {
    constructor(props) {
        super(props);

        this.state = {
            users: [],
            user_input: ""
        };
    }

    componentDidMount() {
        this.getUsersData();
    }

    onUserInputChange = (event) => {
        this.setState({ user_input: event.target.value });
    }

    async getUsersData() {
        Get("api/project/getusers?id=" + this.props.match.params.projectId, (response) => {
            response.json()
                .then(result => this.setState({ users: result.users }));
        });
    }

    onSubmit = (event) => {
        event.preventDefault();

        const { users } = this.state;

        const projectId = this.props.match.params.projectId;
        const body = { ProjectId: projectId, Users: users };

        Post("api/project/invite", body, (response) => {
            response.json().then(result => {
                if (result.status === 200) {
                    window.location.href = "/projects/get/" + projectId;
                }
            });
        }, 'Json');
    }

    onRemoveUser = (event) => {
        const users = this.state.users;
        debugger;
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
            }
        });
    }

    render() {
        return (
            <form onSubmit={this.onSubmit}>
                <input type="text" placeholder="Имя пользователя" onChange={this.onUserInputChange} />
                <button onClick={this.onAddingUser}>Добавить</button>
                <div>
                    {
                        this.state.users.map(userName => (
                            <UserAdd userName={userName} onRemove={this.onRemoveUser} />
                        ))
                    }
                </div>
                <input type="submit" value="Принять" />
            </form>
        );
    }
}
