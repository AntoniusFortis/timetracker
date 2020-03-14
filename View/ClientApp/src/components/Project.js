import React, { Component } from 'react';

export class Projects extends Component {
    constructor(props) {
        super(props);
        this.state = { projectView: [], loading: true };
    }

    componentDidMount() {
        this.getProjectsData();
    }

    static renderForecastsTable(forecasts) {
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
                        forecasts.value.signedProjects.map(forecast => {
                            console.log(forecast);
                            return (<tr key={forecast.id} >
                                <td>{forecast.title}</td>
                                <td>{forecast.description}</td>
                            </tr>);
                        })
                    }
                </tbody>
            </table>
        );
    }

    static renderForecastsTable2(forecasts) {
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
                        forecasts.value.notSignedProjects.map(forecast => {
                            console.log(forecast);
                            return (<tr key={forecast.id} >
                                <td>{forecast.title}</td>
                                <td>{forecast.description}</td>
                            </tr>);
                        })
                    }
                </tbody>
            </table>
        );
    }

    render() {
        let contents = this.state.loading
            ? <p><em>Loading...</em></p>
            : Projects.renderForecastsTable(this.state.projectView);
        let contents2 = this.state.loading
            ? <p><em>Loading...</em></p>
            : Projects.renderForecastsTable2(this.state.projectView);

        return (
            <div>
                <h1 id="tabelLabel" >My projects</h1>
                <p>Projects</p>
                {contents}
                <p>You was invited to Projects</p>
                {contents2}
            </div>
        );
    }

    async getProjectsData() {
        const response = await fetch('Project');
        const data = await response.json();
        this.setState({ projectView: data, loading: false });
    }
}
