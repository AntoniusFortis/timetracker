import React, { Component } from 'react';

export class Auth extends Component {

    constructor(props) {
        super(props);
        this.state = { name: "" };

        this.onSubmit = this.onSubmit.bind(this);
        this.onNameChange = this.onNameChange.bind(this);
    }

    onNameChange(e) {
        this.setState({ name: e.target.value });
    }

    onSubmit(e) {
        e.preventDefault();
        var userName = this.state.name.trim();
        if (!userName) {
            return;
        }
        this.setState({ name: "" });

        const data = new FormData();
        data.append("Name", userName);

        var xhr = new XMLHttpRequest();
        xhr.onload = function () {
            if (xhr.status == 200) {
                window.location.href = "/"
                //this.props.history.push("/");
            }
        }.bind(this);
        xhr.open("post", "/WeatherForecast/Auth", false);

        xhr.send(data);
    }

    render() {
        return (
            <form onSubmit={this.onSubmit}>
                <p>
                    <input type="text" placeholder="Name" value={this.state.name} onChange={this.onNameChange} />
                </p>
                <input type="submit" value="Save" />
            </form>
        );
    }
}
