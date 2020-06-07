import React, { PureComponent, Component } from 'react';
import Select from 'react-select';
import { Get, Delete, Post } from '../../restManager';
import { TrackingService } from '../../services/TrackingService'
import { Tabs, Tab } from '../../Tabs';
import moment from 'moment'
import { Redirect } from 'react-router';
import { NavLink } from 'reactstrap';
import { Link } from 'react-router-dom';
const editIcon = require('../editProject.png');
const deleteIcon = require('../deleteProject.png');

export class TaskGet extends Component {
    states = {}

    constructor(props) {
        super(props);

        this.state = {
            worktask: {},
            loading: true,
            project: {},
            worktracks: [],
            isAdmin: true,
            referrer: null
        };
    }

    componentDidMount() {
        this.getStatesData().then(this.getTaskData());
        this.getWorktacksData();
    }

    async getTaskData() {
        Get("api/task/get?id=" + this.props.match.params.taskId, (response) => {
            response.json()
                .then(result => {
                    this.setState({ worktask: result.worktask, project: result.project, isAdmin: result.isAdmin });
                });
        });
    }

    getWorktacksData = () => {
        Get("api/worktrack/getall?id=" + this.props.match.params.taskId, (response) => {
            response.json()
                .then(result => {
                    this.setState({ worktracks: result, loading: false });
                });
        });
    }

    async getStatesData() {
        Get("api/state/getall", (response) => {
            response.json()
                .then(result => {
                    const states = result.states.map(state => {
                        return {
                            value: state.Id,
                            label: state.Title
                        }
                    });

                    this.states = states;
                });
        });
    }

    renderTaskTable = (worktask) => {
        const offset = moment().utcOffset();
        const createdDate = moment(worktask.CreatedDate).add(offset, 'm').format('L');
        const val = this.states[worktask.StateId - 1];

        return (
            <div>
            <table className='table' aria-labelledby="tabelLabel">
                <tbody>
                    <tr key={worktask.Id}>
                            <td>Проект: {this.state.project.Title}</td>
                            <td>Состояние: <div style={{ display: 'inline-block', width: '250px', paddingLeft: '5px' }}><Select options={this.states} defaultValue={val} onChange={this.onStateChange} /></div></td>
                    </tr>
                    <tr>
                        <td>Дата создания: {createdDate}</td>
                        <td>Выделено часов: {worktask.Duration}</td>
                    </tr>
                </tbody>
                </table>
                {worktask.Description && <div style={{ padding: '12px' }}>Описание: <br />{worktask.Description}</div>}
            </div>
        );
    }

    renderWorktracksTable(worktracks) {
        const offset = moment().utcOffset();

        return (
            <table className='table table-striped' aria-labelledby="tabelLabel">
                <thead>
                    <tr>
                        <th>Пользователь</th>
                        <th>Время начала</th>
                        <th>Время окончания</th>
                        <th>Затраченное время</th>
                    </tr>
                </thead>
                <tbody>
                    {
                        worktracks.map(worktrack => {
                            const start = moment(worktrack.StartedTime).add(offset, 'm').format('YYYY-MM-DD HH:mm:ss');
                            const stop = moment(worktrack.StoppedTime).add(offset, 'm').format('YYYY-MM-DD HH:mm:ss');
                            return (
                                <tr key={worktrack.Id}>
                                    <td>{worktrack.User}</td>
                                    <td>{start}</td>
                                    <td>{stop}</td>
                                    <td>{worktrack.TotalTime}</td>
                                </tr>
                            )
                         } )
                    }
                </tbody>
            </table>
        );
    }

    onStateChange = (event) => {
        const body = {
            taskId: this.state.worktask.Id,
            stateId: event.value
        }

        Post("api/task/UpdateState", body);
    }

    onRemoveWorktask = (event) => {
        event.preventDefault();

        Delete("api/task/delete?Id=" + this.state.worktask.Id,
            {},
            (response) => {
                if (response.status === 200) {
                    this.setState({ referrer: '/project/get/' + this.state.worktask.Id });
                }
            });
    }

    onClickEditProject = (event) => {
        this.setState({ referrer: '/task/update/' + this.state.worktask.Id });
    }

    render() {
        const { loading, worktask, worktracks } = this.state;

        if (loading) {
            return <div />;
        }

        const worktaskInfo = this.renderTaskTable(worktask);
        const worktracksInfo = this.renderWorktracksTable(worktracks);

        const removebutton = this.state.isAdmin ? <div style={{ display: "inline-block", margin: '5px' }}>
            <form onSubmit={this.onRemoveWorktask}>
                <button style={{ border: 'none', paddingLeft: '2px' }}><img src={deleteIcon} style={{ marginBottom: '3px' }} width="28"></img><span>Удалить</span></button>
            </form>
        </div> : (<div />);

        const changeButton = this.state.isAdmin ? <div style={{ display: "inline-block", margin: '5px' }}>
            <button onClick={this.onClickEditProject} style={{ border: 'none', paddingLeft: '2px' }}><img src={editIcon} style={{ marginBottom: '3px' }} width="28"></img><span>Редактировать</span></button>
        </div> : (<div />);

        return (
            <div>
                {this.state.referrer && <Redirect to={this.state.referrer} />}
                <div style={{ display: "block" }}>
                    <h4>Задача: {this.state.worktask.Title}</h4>
                </div>
                <div style={{ height: '40px' }}>
                    {removebutton}
                    {changeButton}
                    {!loading && <TrackingService worktaskId={this.state.worktask.Id} onActiveTrackingReceive={() => this.getWorktacksData()} />}
                </div>

                <Tabs selectedTab={this.state.selectedTab} onChangeTab={selectedTab => this.setState({ selectedTab })}>
                    <Tab name="first" title="Описание задачи">
                        {worktaskInfo}
                    </Tab>
                    <Tab name="second" title="Затраченное время">
                        {worktracksInfo}
                    </Tab>
                </Tabs>
            </div>
        );
    }
}
