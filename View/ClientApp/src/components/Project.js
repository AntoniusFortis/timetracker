import React, { Component } from 'react';
import { Link } from 'react-router-dom';
import { NavLink } from 'reactstrap';

export class Projects extends Component {
    constructor(props) {
        super(props);
        this.state = { projectView: [], loading: true };
    }

    componentDidMount() {
        this.getProjectsData();
    }

    async getProjectsData() {
        fetch('Project')
            .then(x => x.json())
            .then(x => {
                this.setState({ projectView: x, loading: false });
            });
    }

    static renderProjectsTable(projects) {
        return (
            <table className='table table-striped' aria-labelledby="tabelLabel">
                <thead>
                    <tr>
                        <th>Title</th>
                        <th>Description</th>
                    </tr>
                </thead>
                <tbody>
                    {
                        projects.map(x => {
                            return <tr key={x.id}>
                                <td>
                                    <NavLink tag={Link} className="text-dark" to="/projects">{x.title}</NavLink>
                                </td>
                                <td>{x.description}</td>
                            </tr>;
                        })
                    }
                </tbody>
            </table>
        );
    }

    render() {
        let signedProjects = this.state.loading
            ? <p><em>Loading...</em></p>
            : Projects.renderProjectsTable(this.state.projectView.signedProjects);
        let notSignedProjects = this.state.loading
            ? <p><em>Loading...</em></p>
            : Projects.renderProjectsTable(this.state.projectView.notSignedProjects);

        return (
            <div>
                <h1 id="tabelLabel">My projects</h1>
                <p>Projects</p>
                {signedProjects}
                <p>You was invited to Projects</p>
                {notSignedProjects}
            </div>
        );
    }
}

export class NewProject extends Component {

    constructor(props) {
        super(props);
        this.state = { title: "", description: "", users: [] };

        this.onSubmit = this.onSubmit.bind(this);
        this.onNameChange = this.onTitleChange.bind(this);
    }

    onTitleChange(e) {
        this.setState({ title: e.target.value });
    }

    onDescriptionChange(e) {
        this.setState({ description: e.target.value });
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

        fetch('Project/AddProject', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json;charset=utf-8'
            },
            body: JSON.stringify({ Title: title, Description: description })
        })
            .then(x => {
                if (x.status === 200)
                    window.location.href = "/"
            });
    }

    render() {
        return (
            <form onSubmit={this.onSubmit}>
                <p>
                    <input type="text" placeholder="Title" value={this.state.title} onChange={ x => this.onTitleChange(x) } />
                </p>
                <p>
                    <input type="text" placeholder="Description" value={this.state.description} onChange={x => { this.onDescriptionChange(x) }} />
                </p>
                <input type="submit" value="Add" />
            </form>
        );
    }
}