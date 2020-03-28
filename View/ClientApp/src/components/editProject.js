import React, { Component } from 'react';
import { AddProject_User } from './Project';

export class EditProject_User extends React.Component {
    constructor(props) {
        super(props);
        this.state = { data: props.user };
        this.onClick = this.onClick.bind(this);
    }

    onClick(e) {
        e.preventDefault();
        this.props.onRemove(this.state.data);
    }

    render() {
        return <div>
            <p><b>{this.state.data}</b></p>
            <p><button onClick={this.onClick}>Удалить</button></p>
        </div>;
    }
}

export class editProject extends Component {

    constructor(props) {
        super(props);

        this.state = { project: null, title: "", description: "", users: [], user_input: "" };

        this.onSubmit = this.onSubmit.bind(this);

        this.onNameChange = this.onTitleChange.bind(this);
        this.onDescriptionChange = this.onDescriptionChange.bind(this);
        this.onAddingUser = this.onAddingUser.bind(this);
        this.onRemoveUser = this.onRemoveUser.bind(this);
    }

    componentDidMount() {
        this.getProjectsData();
    }

    onTitleChange(e) {
        this.setState({ title: e.target.value });
    }

    onDescriptionChange(e) {
        this.setState({ description: e.target.value });
    }

    onUserInputChange(e) {
        this.setState({ user_input: e.target.value });
    }

    async getProjectsData() {
        var myHeaders = new Headers();
        myHeaders.append("Authorization", "Bearer " + localStorage.getItem('tokenKey'));
        fetch('api/project/getproject?id=' + this.props.match.params.projectId, {
            method: "GET",
            headers: myHeaders
        })
            .then(x => x.json())
            .then(x => {
                this.setState({ project: x.project, users: x.users, title: x.project.Title, description: x.project.Description });
            });
    }

    onSubmit(e) {
        e.preventDefault();
        var title = this.state.title.trim();
        if (!title) {
            return;
        }

        var description = this.state.description.trim();
        if (!description) {
            return;
        }

        var users = this.state.users;
        var myHeaders = new Headers();
        myHeaders.append("Authorization", "Bearer " + localStorage.getItem('tokenKey'));
        myHeaders.append("Content-Type", "application/json;charset=utf-8");

        fetch('api/project/update', {
            method: 'POST',
            headers: myHeaders,
            body: JSON.stringify({ Project: { Id: this.state.project.Id, Title: title, Description: description }, Users: users })
        })
            .then(x => {
                if (x.status === 200)
                    window.location.href = "/projects/getproject?id=" + this.props.match.params.projectId;
            });
    }

    onRemoveUser(data) {
        var arr = this.state.users;
        let tttt = arr.indexOf(data.Name);

        this.state.users.splice(tttt, 1);

        this.setState({ users: this.state.users });
    }

    onAddingUser(e) {
        e.preventDefault();
        var arr = this.state.users;
        arr.push(this.state.user_input);
        this.setState({ users: arr });
    }

    render() {
        var rem = this.onRemoveUser;
        return (
            <form onSubmit={this.onSubmit}>
                <p>
                    <input type="text" placeholder="Title" value={this.state.title} onChange={x => this.onTitleChange(x)} />
                </p>
                <p>
                    <input type="text" placeholder="Description" value={this.state.description} onChange={x => this.onDescriptionChange(x)} />
                </p>
                <input type="submit" value="Update" />

                <input type="text" placeholder="user name" onChange={x => this.onUserInputChange(x)} />
                <button onClick={x => this.onAddingUser(x)}>Add user</button>

                <div>
                    {
                        this.state.users.map(function (x) {
                            debugger;
                            return <EditProject_User key={x} user={x} onRemove={rem} />
                        })
                    }
                </div>
            </form>
        );
    }
}
