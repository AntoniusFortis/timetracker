import React, { Component } from 'react';

export function hasAuthorized() {
    var result = localStorage.getItem('tokenKey') != undefined;
    return result;
}

export class SignIn extends Component {

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

        fetch('api/account/signin', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json;charset=utf-8'
            },
            body: JSON.stringify({ Login: userName, Pass: userPassword })
        })
            .then(x => x.json())
            .then(x => {
                if (x.status === 200) {
                    console.log(x.access_token);

                    localStorage.setItem('tokenKey', x.access_token);
                    window.location.href = "/";
                }
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
                <input type="submit" value="Sign In" />
            </form>
        );
    }
}

export class SignUp extends Component {

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

        fetch('api/account/signup', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json;charset=utf-8'
            },
            body: JSON.stringify({ Login: userName, Pass: userPassword })
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