import React, { Component } from 'react';
import { Get, Post } from '../../restManager';

export class TaskUpdate extends Component {

    constructor(props) {
        super(props);

        this.state = {
            worktask: null,
            title: "",
            description: "",
            duration: ""
        };
    }

    componentDidMount() {
        this.getTaskData();
    }

    onTitleChange = (event) => {
        this.setState({ title: event.target.value });
    }

    onDescriptionChange = (event) => {
        this.setState({ description: event.target.value });
    }

    onDurationChange = (event) => {
        this.setState({ duration: event.target.value });
    }

    async getTaskData() {
        Get("api/task/get?id=" + this.props.match.params.taskId, (response) => {
            response.json()
                .then(result => {
                    this.setState({
                        worktask: result.worktask,
                        title: result.worktask.Title,
                        description: result.worktask.Description,
                        duration: result.worktask.Duration
                    });
                });
        });
    }

    onSubmit = (event) => {
        event.preventDefault();
        const { description, title, worktask, duration } = this.state;

        worktask.Title = title;
        worktask.Description = description;
        worktask.Duration = parseInt(duration);

        Post("api/task/update",
            { worktask: worktask },
            (response) => {
                if (response.status === 200) {
                    window.location.href = "/task/get/" + this.props.match.params.taskId;
                }
            });
    }

    render() {
        return (
            <form onSubmit={this.onSubmit}>
                <p>
                    <input type="text" placeholder="Название" value={this.state.title} onChange={this.onTitleChange} />
                </p>
                <p>
                    <input type="text" placeholder="Описание" value={this.state.description} onChange={this.onDescriptionChange} />
                </p>
                <p>
                    <input type="text" placeholder="Длительность" value={this.state.duration} onChange={this.onDurationChange} />
                </p>
                <input type="submit" value="Изменить" />
            </form>
        );
    }
}
