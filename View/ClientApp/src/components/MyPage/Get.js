﻿import React, { useState, useEffect } from 'react';
import { Get, Post } from '../../restManager';
import { Redirect } from 'react-router';

export const MyPageGet = () => {
    const [currentPwd, setCurrentPwd] = useState('');
    const [password, setPassword] = useState('');
    const [password2, setPassword2] = useState('');
    const [firstName, setFirstName] = useState('');
    const [surname, setSurname] = useState('');
    const [middleName, setMiddleName] = useState('');
    const [city, setCity] = useState('');
    const [birthDate, setBirthDate] = useState('');
    const [email, setEmail] = useState('');
    const [referrer, setReferrer] = useState(null);

    const getInfo = () => {
        Get('api/mypage/get', (response) => {
            response.json().then(result => {
                setFirstName(result.user.FirstName);
                setSurname(result.user.Surname);
                setMiddleName(result.user.MiddleName);
                setCity(result.user.City);
                setBirthDate(result.user.BirthDate);
                setEmail(result.user.Email);
            });
        });
    }


    useEffect(() => getInfo(), []);

    const onChangedInformation = (response) => {
        if (response.passwordChanged) {
            localStorage.removeItem('tokenKey');
            setReferrer('/account/signin');
        }
    }

    const updateInfo = (event) => {
        event.preventDefault();

        if (password != password2) {
            alert('Ваши пароли не совпадают');
            return;
        }

        const body = {
            CurrentPass: currentPwd,
            Pass: password,
            FirstName: firstName,
            Surname: surname,
            MiddleName: middleName,
            City: city,
            BirthDate: birthDate,
            Email: email
        }

        Post('api/mypage/update', body, (response) => {
            response.json().then(onChangedInformation);
        });
    }

    return (
        <div>
            {referrer && <Redirect to={referrer} />}
            <form onSubmit={updateInfo}>
                <p>
                    <input type="password" minLength="4" maxLength="20" placeholder="Введите нынешний пароль" value={currentPwd} onChange={(e) => setCurrentPwd(e.target.value)} />
                </p>
            <p>
                <input type="password" minLength="4" maxLength="20" placeholder="Пароль" value={password} onChange={(e) => setPassword(e.target.value)} />
            </p>
            <p>
                <input type="password" minLength="4" maxLength="20" placeholder="Повторите пароль" value={password2} onChange={(e) => setPassword2(e.target.value)} />
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
            <input type="submit" value="Изменить" />
            </form>
        </div>);
}