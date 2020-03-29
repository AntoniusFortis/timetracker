import React, { Component } from 'react';
import { Post } from '../../restManager';

export class SignIn extends Component {

    constructor(props) {
        super(props);

        this.state = {
            name: "",
            password: ""
        };

        this.onSubmit = this.onSubmit.bind(this);

        this.onNameChange = this.onNameChange.bind(this);
        this.onPasswordChange = this.onPasswordChange.bind(this);
    }

    onNameChange(e) {
        this.setState({ name: e.target.value });
    }

    onPasswordChange(e) {
        this.setState({ password: e.target.value });
    }

    onSubmit(e) {
        e.preventDefault();

        const { name, password } = this.state;

        if (!name) {
            return;
        }

        if (!password) {
            return;
        }

        Post("api/account/signin", { Login: name, Pass: password }, (response) => {
            response.json().then(result => {
                if (result.status === 200) {
                    localStorage.setItem('tokenKey', result.access_token);
                    window.location.href = "/";
                }
            });
        });
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
                <input type="submit" value="Войти" />
            </form>
        );
    }
}