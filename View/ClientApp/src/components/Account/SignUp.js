import React, { Component } from 'react';
import { Post } from '../../restManager';

export class SignUp extends Component {

    constructor(props) {
        super(props);

        this.state = {
            name: "",
            password: "",
            password2: "",
            firstName: "",
            surname: "",
            middleName: undefined,
            city: undefined,
            birthDate: undefined,
            email: ""
        };
    }

    onNameChange = (event) => {
        this.setState({ name: event.target.value });
    }

    onPasswordChange = (event) => {
        this.setState({ password: event.target.value });
    }

    onPassword2Change = (event) => {
        this.setState({ password2: event.target.value });
    }

    onFirstNameChange = (event) => {
        this.setState({ firstName: event.target.value });
    }

    onSurnameChange = (event) => {
        this.setState({ surname: event.target.value });
    }

    onMiddleNameChange = (event) => {
        this.setState({ middleName: event.target.value });
    }

    onCityChange = (event) => {
        this.setState({ city: event.target.value });
    }

    onBirthDateChange = (event) => {
        this.setState({ birthDate: event.target.value });
    }

    onEmailChange = (event) => {
        this.setState({ email: event.target.value });
    }

    onSubmit = (event) => {
        event.preventDefault();

        const {
            name,
            password,
            password2,
            firstName,
            surname,
            middleName,
            city,
            birthDate,
            email
        } = this.state;

        if (!name) {
            return;
        }

        if (password !== password2) {
            alert("Пароли не совпадают");
            return;
        }

        const body = new FormData()

        body.append('Login', name);
        body.append('Pass', password); 
        body.append('FirstName', firstName);
        body.append('Surname', surname);
        body.append('MiddleName', middleName);
        body.append('City', city);
        body.append('BirthDate', birthDate);
        body.append('Email', email);

        Post("api/account/signup", body, this.onSuccessSignedUp, 'FormData');
    }

    onSuccessSignedUp(response) {
        if (response.status === 200) {
            window.location.href = "/account/signin";
        }
    }

    render() {
        return (
            <form onSubmit={this.onSubmit}>
                <p>
                    <input type="text" required minLength="4" maxLength="20" placeholder="Логин" value={this.state.name} onChange={this.onNameChange} />
                </p>
                <p>
                    <input type="password" required minLength="4" maxLength="20" placeholder="Пароль" value={this.state.password} onChange={this.onPasswordChange} />
                </p>
                <p>
                    <input type="password" required minLength="4" maxLength="20" placeholder="Повторите пароль" value={this.state.password2} onChange={this.onPassword2Change} />
                </p>
                <p>
                    <input type="text" required minLength="2" maxLength="30" placeholder="Имя" value={this.state.firstName} onChange={this.onFirstNameChange} />
                </p>
                <p>
                    <input type="text" required minLength="2" maxLength="30"  placeholder="Фамилия" value={this.state.surname} onChange={this.onSurnameChange} />
                </p>
                <p>
                    <input type="text" minLength="2" maxLength="30" placeholder="Отчество" value={this.state.middleName} onChange={this.onMiddleNameChange} />
                </p>
                <p>
                    <input type="email" required placeholder="E-mail" value={this.state.email} onChange={this.onEmailChange} />
                </p>
                <p>
                    <input type="text" minLength="3" maxLength="50" placeholder="Город" value={this.state.city} onChange={this.onCityChange} />
                </p>
                <p>
                    <input type="date" placeholder="Дата рождения" value={this.state.birthDate} onChange={this.onBirthDateChange} />
                </p>
                <br />
                <input type="submit" value="Зарегистрироваться" />
            </form>
        );
    }
}

