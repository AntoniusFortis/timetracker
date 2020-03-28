import React, { Component } from 'react';
import { Link } from 'react-router-dom';
import { NavLink } from 'reactstrap';

export function Get(uri, callback) {
    let headers = new Headers();
    headers.append("Authorization", "Bearer " + localStorage.getItem('tokenKey'));

    fetch(uri, {
        method: "GET",
        headers: headers
    })
        .then(x => x.json())
        .then(x => callback(x));
}

export class Projects extends Component {
    constructor(props) {
        super(props);
        this.state = { projectView: [], loading: true };
    }

    componentDidMount() {
        this.getProjectsData();
    }

    async getProjectsData() {
        Get('api/project/getall', (result) => {
            this.setState({ projectView: result, loading: false });
        });
    }

    renderProjectsTable(projects) {
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
                            const url = "/project/" + x.id;
                            return <tr key={x.id}>
                                <td>
                                    <NavLink tag={Link} className="text-dark" to={url}>{x.title}</NavLink>
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
            : this.renderProjectsTable(this.state.projectView.signedProjects);
        let notSignedProjects = this.state.loading
            ? <p><em>Loading...</em></p>
            : this.renderProjectsTable(this.state.projectView.notSignedProjects);

        return (
            <div>
                <h1 id="tabelLabel">My projects</h1>
                <p>Projects</p>
                {signedProjects}
                <p>You were invited to Projects</p>
                {notSignedProjects}
            </div>
        );
    }
}