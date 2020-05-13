import React, { Component } from 'react';
import { Post } from '../../restManager';

export class ProjectAdd extends Component {

    constructor(props) {
        super(props);

        this.state = {
            title: "",
            description: ""
        };
    }

    onTitleChange = (event) => {
        this.setState({ title: event.target.value });
    }

    onDescriptionChange = (event) => {
        this.setState({ description: event.target.value });
    }

    onSubmit = (event) => {
        event.preventDefault();

        const { title, description, users } = this.state;

        if (!title) {
            return;
        }

        const body = { Title: title, Description: description };

        Post("api/project/add",
            { Project: body, Users: users },
            (response) => {
                if (response.status === 200) {
                    window.location.href = "/project/all";
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
                <input type="submit" value="Создать проект" />
            </form>
        );
    }
}