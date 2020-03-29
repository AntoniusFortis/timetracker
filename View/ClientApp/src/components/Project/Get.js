﻿import React, { Component } from 'react';
import { Get, Post } from '../../restManager';

export class ProjectGet extends Component {
    constructor(props) {
        super(props);

        this.state = {
            project: null,
            loading: true,
            users: []
        };

        this.onRemoveProject = this.onRemoveProject.bind(this);
        this.onClickEditProject = this.onClickEditProject.bind(this);
    }

    componentDidMount() {
        this.getProjectsData();
    }

    async getProjectsData() {
        Get("api/project/get?id=" + this.props.match.params.projectId, (response) => {
            response.json()
                .then(result => {
                    this.setState({ project: result.project, loading: false, users: result.users  });
                });
        });
    }

    renderProjectsTable(project) {
        return (
            <table className='table table-striped' aria-labelledby="tabelLabel">
                <thead>
                    <tr>
                        <th>Title</th>
                        <th>Description</th>
                    </tr>
                </thead>
                <tbody>
                    <tr key={project.Id}>
                        <td>{project.Title}</td>
                        <td>{project.Description}</td>
                    </tr>
                </tbody>
            </table>
        );
    }

    renderUsersTable(users) {
        return (
            <table className='table table-striped' aria-labelledby="tabelLabel">
                <thead>
                    <tr>
                        <th>Name</th>
                    </tr>
                </thead>
                <tbody>
                    {
                        users.map(x => (
                                <tr>
                                    <td>{x}</td>
                                </tr>
                            )
                        )
                    }
                </tbody>
            </table>
        );
    }

    onRemoveProject(e) {
        e.preventDefault();

        Post("api/project/remove?id=" + this.state.project.Id,
            { },
            (response) => {
                if (response.status === 200) {
                    window.location.href = "/project/all";
                }
            });
    }

    onClickEditProject(e) {
        window.location.href = "/project/update/" + this.state.project.Id;
    }

    render() {
        const project = this.state.loading
            ? <p><em>Loading...</em></p>
            : this.renderProjectsTable(this.state.project);

        const users = this.state.loading
            ? <p><em>Loading...</em></p>
            : this.renderUsersTable(this.state.users);

        return (
            <div>
                <p>Project</p>
                <button onClick={this.onClickEditProject}>Edit Project</button>
                <form onSubmit={this.onRemoveProject}>
                    <button>Удалить проект</button>
                </form>
                {project}
                <p>Пользователи</p>
                {users}
            </div>
        );
    }
}
