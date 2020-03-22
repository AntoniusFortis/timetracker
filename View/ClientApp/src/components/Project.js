import React, { Component } from 'react';

export class CurrentProject extends Component {
    constructor(props) {
        super(props);

        this.state = { project: null, loading: true, users: [] };
    }

    componentDidMount() {
        this.getProjectsData();
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
                this.setState({ project: x.project, loading: false, users: x.users });
            });
    }

    static renderProjectsTable(project) {
        return (
            <table className='table table-striped' aria-labelledby="tabelLabel">
                <thead>
                    <tr>
                        <th>Title</th>
                        <th>Description</th>
                    </tr>
                </thead>
                <tbody>
                    <tr key={project.id}>
                        <td>{project.title}</td>
                        <td>{project.description}</td>
                    </tr>
                </tbody>
            </table>
        );
    }

    static renderUsersTable(users) {
        return (
            <table className='table table-striped' aria-labelledby="tabelLabel">
                <thead>
                    <tr>
                        <th>Name</th>
                    </tr>
                </thead>
                <tbody>
                    {
                        users.map(x => {
                            return <tr>
                                <td>{x.login}</td>
                            </tr>;
                        })
                    }
                </tbody>
            </table>
        );
    }

    render() {
        let project = this.state.loading
            ? <p><em>Loading...</em></p>
            : CurrentProject.renderProjectsTable(this.state.project);

        let users = this.state.loading
            ? <p><em>Loading...</em></p>
            : CurrentProject.renderUsersTable(this.state.users);

        return (
            <div>
                <p>Project</p>
                {project}
                <p>Users</p>
                {users}
            </div>
        );
    }
}

export class AddProject_User extends React.Component {
    constructor(props) {
        super(props);
        this.state = { data: props.user };
        this.onClick = this.onClick.bind(this);
    }

    onClick(e) {
        this.props.onRemove(this.state.data);
    }

    render() {
        return <div>
            <p><b>{this.state.data}</b></p>
            <p><button onClick={this.onClick}>Удалить</button></p>
        </div>;
    }
}

export class AddProject extends Component {

    constructor(props) {
        super(props);
        this.state = { title: "", description: "", users: [], user_input: "" };

        this.onSubmit = this.onSubmit.bind(this);

        this.onNameChange = this.onTitleChange.bind(this);
        this.onDescriptionChange = this.onDescriptionChange.bind(this);
        this.onAddingUser = this.onAddingUser.bind(this);
        this.onRemoveUser = this.onRemoveUser.bind(this);
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

        fetch('api/project/addproject', {
            method: 'POST',
            headers: myHeaders,
            body: JSON.stringify({ Project: { Title: title, Description: description }, Users: users })
        })
            .then(x => {
                if (x.status === 200)
                    window.location.href = "/";
            });
    }

    onRemoveUser(data) {
        var arr = this.state.users;
        let tttt = arr.indexOf(data.Name);
        arr.splice(tttt, 1); 
        this.setState({ users: arr });
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
                    <input type="text" placeholder="Title" value={this.state.title} onChange={ x => this.onTitleChange(x) } />
                </p>
                <p>
                    <input type="text" placeholder="Description" value={this.state.description} onChange={x => this.onDescriptionChange(x) } />
                </p>
                <input type="submit" value="Add" />

                <input type="text" placeholder="user name" onChange={x => this.onUserInputChange(x)} />
                <button onClick={x => this.onAddingUser(x)}>Add user</button>

                <div>
                    {
                        this.state.users.map(function (x) {
                            return <AddProject_User key={x.id} user={x} onRemove={rem} />
                        })
                    }
                </div>
            </form>
        );
    }
}