import React, { Component } from 'react';
import { Post } from '../../restManager';

export class TaskAdd extends Component {

    constructor(props) {
        super(props);

        this.state = {
            title: "",
            description: "",
            duration: ""
        };
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

    onSubmit = (event) => {
        event.preventDefault();

        const { title, description, duration } = this.state;

        if (!title) {
            return;
        }

        const worktask = {
            Title: title,
            Description: description,
            Duration: parseInt(duration),
            StateId: 1,
            ProjectId: parseInt(this.props.match.params.projectId),
            CreatedDate: new Date()
        };

        Post("api/task/add",
            { worktask: worktask },
            (response) => {
                if (response.status === 200) {
                    window.location.href = "/project/get/" + this.props.match.params.projectId;
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
                <input type="submit" value="Добавить" />
            </form>
        );
    }
}