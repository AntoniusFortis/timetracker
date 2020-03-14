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

    static renderForecastsTable(projects) {
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
                        projects.map(forecast => {
                            return <tr key={forecast.id}>
                                <td>
                                    <NavLink tag={Link} className="text-dark" to="/projects">{forecast.title}</NavLink>
                                </td>
                                <td>{forecast.description}</td>
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
            : Projects.renderForecastsTable(this.state.projectView.signedProjects);
        let notSignedProjects = this.state.loading
            ? <p><em>Loading...</em></p>
            : Projects.renderForecastsTable(this.state.projectView.notSignedProjects);

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

    async getProjectsData() {
        fetch('Project')
            .then(x => x.json())
            .then(x => {
                this.setState({ projectView: x, loading: false });
            });
    }
}
