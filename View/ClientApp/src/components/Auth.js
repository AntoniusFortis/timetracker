import React, { Component } from 'react';

export function IsAuth() {
    var result = false;

    var xhr = new XMLHttpRequest();

    xhr.open("get", "Auth/IsAuth", false);

    xhr.onload = function () {
        if (xhr.status == 200)
            result = true;
    };

    xhr.send();

    return result;
}

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

        fetch('Auth/Auth', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json;charset=utf-8'
            },
            body: JSON.stringify({ Name: userName })
        })
            .then(x => {
                if (x.status === 200)
                    window.location.href = "/"
            });
    }

    render() {
        return (
            <form onSubmit={this.onSubmit}>
                <p>
                    <input type="text" placeholder="Name" value={this.state.name} onChange={this.onNameChange} />
                </p>
                <input type="submit" value="Sign In" />
            </form>
        );
    }
}

export class Registration extends Component {

    constructor(props) {
        super(props);
        this.state = { name: "", password: "" };

        this.onSubmit = this.onSubmit.bind(this);
        this.onNameChange = this.onNameChange.bind(this);
    }

    onNameChange(e) {
        this.setState({ name: e.target.value });
    }

    onPasswordChange(e) {
        this.setState({ password: e.target.value });
    }

    onSubmit(e) {
        e.preventDefault();
        var userName = this.state.name.trim();
        if (!userName) {
            return;
        }

        var userPassword = this.state.password.trim();
        if (!userPassword) {
            return;
        }

        fetch('Auth/Registration', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json;charset=utf-8'
            },
            body: JSON.stringify({ Name: userName })
        })
            .then(x => {
                if (x.status === 200)
                    window.location.href = "/"
            });
    }

    render() {
        return (
            <form onSubmit={this.onSubmit}>
                <p>
                    <input type="text" placeholder="Name" value={this.state.name} onChange={this.onNameChange} />
                </p>
                <p>
                    <input type="password" placeholder="Password" value={this.state.password} onChange={x => { this.onPasswordChange(x) }} />
                </p>
                <input type="submit" value="Sign Up" />
            </form>
        );
    }
}