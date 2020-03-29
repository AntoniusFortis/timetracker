import React, { Component } from 'react';
import { Link } from 'react-router-dom';
import { NavLink } from 'reactstrap';
import { Get } from '../../restManager';

export class ProjectGetAll extends Component {
    constructor(props) {
        super(props);

        this.state = {
            projectView: [],
            loading: true
        };
    }

    componentDidMount() {
        this.getProjectsData();
    }

    async getProjectsData() {
        Get('api/project/getall', (response) => {
            response.json()
                .then(result => {
                    this.setState({ projectView: result, loading: false });
                });
        });
    }

    renderTable(projects) {
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
                        projects.map(project => 
                            (
                                <tr key={project.Id}>
                                    <td>
                                        <NavLink tag={Link} className="text-dark" to={"/project/get/" + project.Id}>{project.Title}</NavLink>
                                    </td>
                                    <td>{project.Description}</td>
                                </tr>
                            )
                        )
                    }
                </tbody>
            </table>
        );
    }

    render() {
        const { loading, projectView } = this.state;

        const signedProjects = loading
            ? <p><em>Loading...</em></p>
            : this.renderTable(projectView.SignedProjects);

        const notSignedProjects = loading
            ? <p><em>Loading...</em></p>
            : this.renderTable(projectView.NotSignedProjects);

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