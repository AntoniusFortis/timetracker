import React, { PureComponent } from 'react';
import { Link } from 'react-router-dom';
import { NavLink } from 'reactstrap';
import { Get } from '../../restManager';
import { Tabs, Tab } from '../../Tabs';

export class ProjectGetAll extends PureComponent {
    constructor(props) {
        super(props);

        this.state = {
            projectView: { SignedProjects: [], NotSignedProjects: [] },
            selectedTab: 0
        };
    }

    componentDidMount() {
        this.getProjectsData();
    }

    async getProjectsData() {
        Get('api/project/getall', (response) => {
            response.json()
                .then(result => {
                    this.setState({ projectView: result });
                });
        });
    }

    renderTable(projects) {
        return (
            <table className='table table-striped' aria-labelledby="tabelLabel">
                <thead>
                    <tr>
                        <th>Название</th>
                        <th>Описание</th>
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

    renderTable2(projects) {
        return (
            <table className='table table-striped' aria-labelledby="tabelLabel">
                <thead>
                    <tr>
                        <th>Название</th>
                        <th>Описание</th>
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
                                    <td>
                                        <form>
                                        </form>
                                    </td>
                                </tr>
                            )
                        )
                    }
                </tbody>
            </table>
        );
    }
    render() {
        const { projectView } = this.state;

        const signedProjects = this.renderTable(projectView.SignedProjects);
        const notSignedProjects = this.renderTable2(projectView.NotSignedProjects);

        return (
            <Tabs selectedTab={this.state.selectedTab} onChangeTab={selectedTab => this.setState({ selectedTab })}>
                <Tab name="first" title="Проекты">
                    {signedProjects}
                </Tab>
                <Tab name="second" title="Проекты в которые вы были приглашены">
                    {notSignedProjects}
                </Tab>
            </Tabs>
        );
    }
}