import React, { useState } from 'react';
import { Post } from '../../restManager';

export const SignIn = () => {
    const [name, setName] = useState('');
    const [password, setPassword] = useState('');

    const onSuccessSignedIn = (response) => {
        if (response.status === 200) {
            localStorage.setItem('tokenKey', response.access_token);
            window.location.href = "/";
        }
    }

    const trySignIn = (event) => {
        event.preventDefault();

        const body = { Login: name, Pass: password };

        Post("api/account/signin", body, (response) => {
            response.json().then(onSuccessSignedIn);
        });
    }

    return (
        <form onSubmit={trySignIn}>
            <p>
                <input required type="text" placeholder="Логин" value={name} onChange={(e) => setName(e.target.value)} />
            </p>
            <p>
                <input required type="password" placeholder="Пароль" value={password} onChange={(e) => setPassword(e.target.value)} />
            </p>
            <input type="submit" value="Авторизоваться" />
        </form>);
}