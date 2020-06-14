import React, { Component } from 'react';
import { Get, Post } from '../../restManager';

const InputField = (props) => {
    return (
        <div style={{ paddingTop: '15px' }}>
            <input required={props.required} minLength={props.minLength} maxLength={props.maxLength} style={{ width: '270px', textAlign: 'center' }} type={props.type} placeholder={props.placeholder} value={props.value} onChange={props.onChange} />
        </div>);
}

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
            <div style={{ width: '300px', margin: '0 auto', paddingTop: '125px', height: '300px', display: 'block' }}>
                <form style={{ width: '400px' }} onSubmit={this.onSubmit}>
                    <InputField required={true} minLength="5" maxLength="50" type="text" placeholder="Название" value={this.state.title} onChange={this.onTitleChange} />
                    <div style={{ paddingTop: '15px' }}>
                        <input style={{ width: '270px', textAlign: 'center' }} type="number" min="1" max="100" placeholder="Длительность" value={this.state.duration} onChange={this.onDurationChange} />
                    </div>
                    <div style={{ paddingTop: '15px' }}>
                        <textarea style={{ width: '270px', textAlign: 'center' }} maxLength="250" placeholder="Описание" value={this.state.description} onChange={this.onDescriptionChange} />
                    </div>
                    <input style={{ display: 'block', width: '270px', marginTop: '15px' }} type="submit" value="Изменить" />
                </form>
            </div>
        );
    }
}
