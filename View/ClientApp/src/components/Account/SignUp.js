import React, { Component } from 'react';
import { Post } from '../../restManager';

export class SignUp extends Component {

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
        this.setState({ name: e.target.value.trim() });
    }

    onPasswordChange(e) {
        this.setState({ password: e.target.value.trim() });
    }

    onSubmit(e) {
        e.preventDefault();

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
                    <input type="text" placeholder="Name" value={this.state.name} onChange={this.onNameChange} />
                </p>
                <p>
                    <input type="password" placeholder="Password" value={this.state.password} onChange={this.onPasswordChange} />
                </p>
                <input type="submit" value="Sign Up" />
            </form>
        );
    }
}