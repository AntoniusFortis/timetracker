﻿import React, { useState } from 'react';
import { Post } from '../../restManager';

export const SignUp = () => {
    const [name, setName] = useState('');
    const [password, setPassword] = useState('');
    const [password2, setPassword2] = useState('');
    const [firstName, setFirstName] = useState('');
    const [surname, setSurname] = useState('');
    const [middleName, setMiddleName] = useState(undefined);
    const [city, setCity] = useState(undefined);
    const [birthDate, setBirthDate] = useState(undefined);
    const [email, setEmail] = useState('');

    const onSuccessSignedUp = (response) => {
        if (response.status === 200) {
            window.location.href = "/account/signin";
        }
    }

    const trySignUp = (event) => {
        event.preventDefault();

        if (password !== password2) {
            alert("Пароли не совпадают");
            return;
        }

        const body = {
            Login: name,
            Pass: password,
            FirstName: firstName,
            Surname: surname,
            MiddleName: middleName,
            City: city,
            BirthDate: birthDate,
            Email: email
        }

        //const body = new FormData()
        //body.append('Login', name);
        //body.append('Pass', password);
        //body.append('FirstName', firstName);
        //body.append('Surname', surname);
        //body.append('MiddleName', middleName);
        //body.append('City', city);
        //body.append('BirthDate', birthDate);
        //body.append('Email', email);

        Post("api/account/signup", body, onSuccessSignedUp);
    }

    return (
        <form onSubmit={trySignUp}>
            <p>
                <input type="text" required minLength="4" maxLength="20" placeholder="Логин" value={name} onChange={(e) => setName(e.target.value)} />
            </p>
            <p>
                <input type="password" required minLength="4" maxLength="20" placeholder="Пароль" value={password} onChange={(e) => setPassword(e.target.value)} />
            </p>
            <p>
                <input type="password" required minLength="4" maxLength="20" placeholder="Повторите пароль" value={password2} onChange={(e) => setPassword2(e.target.value)} />
            </p>
            <p>
                <input type="text" required minLength="2" maxLength="30" placeholder="Имя" value={firstName} onChange={(e) => setFirstName(e.target.value)} />
            </p>
            <p>
                <input type="text" required minLength="2" maxLength="30" placeholder="Фамилия" value={surname} onChange={(e) => setSurname(e.target.value)} />
            </p>
            <p>
                <input type="text" minLength="2" maxLength="30" placeholder="Отчество" value={middleName} onChange={(e) => setMiddleName(e.target.value)} />
            </p>
            <p>
                <input type="email" required placeholder="E-mail" value={email} onChange={(e) => setEmail(e.target.value)} />
            </p>
            <p>
                <input type="text" minLength="3" maxLength="50" placeholder="Город" value={city} onChange={(e) => setCity(e.target.value)} />
            </p>
            <p>
                <input type="date" placeholder="Дата рождения" value={birthDate} onChange={(e) => setBirthDate(e.target.value)} />
            </p>
            <br />
            <input type="submit" value="Зарегистрироваться" />
        </form>);
}