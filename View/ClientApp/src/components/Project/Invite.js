import React, { PureComponent } from 'react';
import { Get, Post } from '../../restManager';

const UserAdd = ({ userName, onRemove }) => (
    <div>
        <p><b>{userName}</b></p>
        <p><button onClick={(e) => onRemove(e, userName)}>Удалить</button></p>
    </div>
);

const UsersList = (props) => {
    return (
        <div>
            {
                props.users.map(userName => (
                    <UserAdd userName={userName} onRemove={props.onRemoveUser} />
                ))
            }
        </div>);
}

export class ProjectInvite extends PureComponent {
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
                    window.location.href = "/project/get/" + projectId;
                }
            });
        });
    }

    onRemoveUser = (event, userName) => {
        event.preventDefault();

        const users = this.state.users;
        const idx = users.findIndex((element) => { return element === userName });
        this.state.users.splice(idx, 1);
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
                <UsersList users={this.state.users} onRemoveUser={this.onRemoveUser} />
                <input type="submit" value="Принять" />
            </form>
        );
    }
}
