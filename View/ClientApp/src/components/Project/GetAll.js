import React, { PureComponent } from 'react';
import { Link } from 'react-router-dom';
import { NavLink } from 'reactstrap';
import { Get, Post } from '../../restManager';
import { Tabs, Tab } from '../../Tabs';

const ProjectGetAllTable = (props) => {
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
                    props.projects.map(project =>
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
        </table>);
}


const ProjectGetAllInvitesTable = (props) => {
    return (
        <table className='table table-striped' aria-labelledby="tabelLabel">
            <thead>
                <tr>
                    <th></th>
                    <th>Название</th>
                    <th>Описание</th>

                </tr>
            </thead>
            <tbody>
                {
                    props.projects.map(project =>
                        (
                            <tr key={project.Id}>
                                <td>
                                    <button onClick={(e) => props.acceptInvite(project.Id)}>Принять</button>
                                    <button onClick={(e) => props.rejectInvite(project.Id)}>Отказать</button>
                                </td>
                                <td>{project.Title}</td>
                                <td>{project.Description}</td>
                            </tr>
                        )
                    )
                }
            </tbody>
        </table>);
}

export class ProjectGetAll extends PureComponent {
    constructor(props) {
        super(props);

        this.state = {
            projectView: { acceptedProjects: [], notAcceptedProjects: [] },
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

    acceptInvite = (projectId) => {
        Post('api/project/accept', { ProjectId: projectId }, (response) => {
            this.getProjectsData();
        });
    }

    rejectInvite = (projectId) => {
        Post('api/project/reject?id=' + projectId, {}, (response) => {
            this.getProjectsData();
        });
    }

    render() {
        const { projectView } = this.state;
        return (
            <Tabs selectedTab={this.state.selectedTab} onChangeTab={selectedTab => this.setState({ selectedTab })}>
                <Tab name="first" title="Проекты">
                    <ProjectGetAllTable projects={projectView.acceptedProjects} />
                </Tab>
                <Tab name="second" title="Приглашения">
                    <ProjectGetAllInvitesTable projects={projectView.notAcceptedProjects} acceptInvite={this.acceptInvite} rejectInvite={this.rejectInvite} />
                </Tab>
            </Tabs>
        );
    }
}