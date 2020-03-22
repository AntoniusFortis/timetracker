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
        var myHeaders = new Headers();
        myHeaders.append("Authorization", "Bearer " + localStorage.getItem('tokenKey'));

        fetch('api/project/getprojects', {
            method: "GET",
            headers: myHeaders
        })
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