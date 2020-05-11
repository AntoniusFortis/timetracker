import React, { Component } from 'react';
import { Post } from '../../restManager';

export class SignIn extends Component {

    constructor(props) {
        super(props);

        this.state = {
            name: "",
            password: ""
        };
    }

    onNameChange = (event) => {
        this.setState({ name: event.target.value });
    }

    onPasswordChange = (event) => {
        this.setState({ password: event.target.value });
    }

    onSubmit = (event) => {
        event.preventDefault();

        const { name, password } = this.state;

        if (!name) {
            return;
        }

        if (!password) {
            return;
        }

        const body = { Login: name, Pass: password };

        Post("api/account/signin", body, (response) => {
            response.json().then(result => {
                if (result.status === 200) {
                    localStorage.setItem('tokenKey', result.access_token);
                    window.location.href = "/";
                }
            });
        }, 'Json');
    }

    render() {
        return (
            <form onSubmit={this.onSubmit}>
                <p>
                    <input type="text" placeholder="Логин" value={this.state.name} onChange={this.onNameChange} />
                </p>
                <p>
                    <input type="password" placeholder="Пароль" value={this.state.password} onChange={this.onPasswordChange} />
                </p>
                <input type="submit" value="Авторизоваться" />
            </form>
        );
    }
}