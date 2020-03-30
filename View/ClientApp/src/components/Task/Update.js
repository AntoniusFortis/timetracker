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

        this.onSubmit = this.onSubmit.bind(this);

        this.onTitleChange = this.onTitleChange.bind(this);
        this.onDescriptionChange = this.onDescriptionChange.bind(this);
        this.onDurationChange = this.onDurationChange.bind(this);
    }

    componentDidMount() {
        this.getTaskData();
    }

    onTitleChange(e) {
        this.setState({ title: e.target.value.trim() });
    }

    onDescriptionChange(e) {
        this.setState({ description: e.target.value.trim() });
    }

    onDurationChange(e) {
        this.setState({ duration: e.target.value.trim() });
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

    onSubmit(e) {
        e.preventDefault();
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
