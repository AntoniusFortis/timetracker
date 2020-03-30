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

        this.onSubmit = this.onSubmit.bind(this);

        this.onTitleChange = this.onTitleChange.bind(this);
        this.onDescriptionChange = this.onDescriptionChange.bind(this);
        this.onDurationChange = this.onDurationChange.bind(this);
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

    onSubmit(e) {
        e.preventDefault();

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
                    <input type="text" placeholder="Описание" value={this.state.description} onChange={x => this.onDescriptionChange(x)} />
                </p>
                <p>
                    <input type="text" placeholder="Длительность" value={this.state.duration} onChange={this.onDurationChange} />
                </p>
                <input type="submit" value="Добавить" />
            </form>
        );
    }
}