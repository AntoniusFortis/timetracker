import React, { Component } from 'react';
import { Post } from '../../restManager';

export class SignUp extends Component {

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

        if (!password || password.length < 3) {
            alert("Пароль должен иметь длину больше трёх символов!");
            return;
        }

        Post("api/account/signup",
            { Login: name, Pass: password },
            (response) => {
                if (response.status === 200) {
                    window.location.href = "/account/signin";
                }
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
                <input type="submit" value="Зарегистрироваться" />
            </form>
        );
    }
}