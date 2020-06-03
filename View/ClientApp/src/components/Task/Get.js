﻿import React, { PureComponent } from 'react';
import Select from 'react-select';
import { Get, Delete, Post } from '../../restManager';
import { TaskTracking } from '../TaskTracking'
import { Tabs, Tab } from '../../Tabs';
import moment from 'moment'
import { Redirect } from 'react-router';

export class TaskGet extends PureComponent {
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
        this.getStatesData().then(this.getTaskData()).then(this.getWorktacksData());
    }

    async getTaskData() {
        Get("api/task/get?id=" + this.props.match.params.taskId, (response) => {
            response.json()
                .then(result => {
                    this.setState({ worktask: result.worktask, project: result.project, isAdmin: result.isAdmin });
                });
        });
    }

    async getWorktacksData() {
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
            <table className='table' aria-labelledby="tabelLabel">
                <tbody>
                    <tr key={worktask.Id}>
                        <td>Проект: {this.state.project.Title}</td>
                        <td>Описание: {worktask.Description}</td>
                    </tr>
                    <tr>
                        <td>Дата создания: {createdDate}</td>
                        <td>Часов: {worktask.Duration}</td>
                    </tr>
                    <tr>
                        <td>Состояние:</td>
                        <td>
                            <Select options={this.states} defaultValue={val} onChange={this.onStateChange} />
                        </td>
                    </tr>
                </tbody>
            </table>
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
                            const start = moment(worktrack.StartedTime).add(offset, 'm').format('HH:mm:ss');
                            const stop = moment(worktrack.StoppedTime).add(offset, 'm').format('HH:mm:ss');
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

    onRemoveProject = (event) => {
        event.preventDefault();

        Delete("api/task/delete?Id=" + this.state.worktask.Id,
            {},
            (response) => {
                if (response.status === 200) {
                    window.location.href = "/project/get/" + this.state.worktask.Project.Id;
                }
            });
    }

    onClickEditProject = (event) => {
        this.setState({ referrer: '/task/update/' + this.state.worktask.Id });
    }

    render() {
        const { loading, worktask, worktracks } = this.state;

        const worktaskInfo = loading
            ? <p><em>Загрузка...</em></p>
            : this.renderTaskTable(worktask);
        const worktracksInfo = loading
            ? <p><em>Загрузка...</em></p>
            : this.renderWorktracksTable(worktracks);

        const removebutton = this.state.isAdmin ? <div style={{ display: "inline-block" }}>
            <form onSubmit={this.onRemoveProject}>
                <button>Удалить задачу</button>
            </form>
        </div> : (<div />);

        return (
            <div>
                {this.state.referrer && <Redirect to={this.state.referrer} />}
                <div style={{ display: "inline-block" }}>
                    <TaskTracking worktaskId={this.state.worktask.Id} />
                </div>
                <div style={{ display: "inline-block", paddingRight: "10px" }}>
                    <h4>Задача: { this.state.worktask.Title }</h4>
                </div>
                <div style={{ display: "inline-block" }}>
                    <button onClick={this.onClickEditProject}>Редактировать задачу</button>
                </div>
                {removebutton}

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
