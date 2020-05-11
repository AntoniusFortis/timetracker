import React, { Component } from 'react';
import { Link } from 'react-router-dom';
import { NavLink } from 'reactstrap';
import { Get } from '../../restManager';

export class ProjectGetAll extends Component {
    constructor(props) {
        super(props);

        this.state = {
            projectView: [],
            loading: true,
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
                    this.setState({ projectView: result, loading: false });
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

    render() {
        const { loading, projectView } = this.state;

        const signedProjects = loading
            ? <p><em>Загрузка...</em></p>
            : this.renderTable(projectView.SignedProjects);

        const notSignedProjects = loading
            ? <p><em>Загрузка...</em></p>
            : this.renderTable(projectView.NotSignedProjects);

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


function Tabs({ children, selectedTab, onChangeTab }) {
    let tabProps = []
    const content = React.Children.map(children, (child) => {
        if (child.type === Tab) {
            const { title, name } = child.props
            tabProps.push({ title, name })
            // By default show first tab if there is none selected
            if (selectedTab ? (selectedTab !== child.props.name) : (tabProps.length !== 1)) {
                return null
            }
        }
        return child
    })

    const finalSelectedTab = selectedTab ||
        (tabProps.length > 0 && tabProps[0].name)

    return (
        <div className="tabs">
            <Tablist
                selectedTab={finalSelectedTab}
                onChangeTab={onChangeTab}
                tabs={tabProps}
            />
            <div className="tabs__content">
                {content}
            </div>
        </div>
    )
}

function Tablist({ tabs, selectedTab, onChangeTab }) {
    const linkClass = selected => {
        const c = 'tabs__tablist__link'
        return selected ? `${c} ${c}--selected` : c
    }

    return (
        <menu className="tabs__tablist">
            <ul>
                {tabs.map(({ name, title }) =>
                    <li aria-selected={name === selectedTab} role="tab" key={name}>
                        <a
                            className={linkClass(name === selectedTab)}
                            onClick={() => onChangeTab(name)}
                        >
                            {title}
                        </a>
                    </li>
                )}
            </ul>
        </menu>
    )
}

function Tab({ name, children }) {
    return (
        <div id={`tab-${name}`} className="tabs__tab">
            {children}
        </div>
    )
}