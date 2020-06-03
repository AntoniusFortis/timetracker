import React, { useState } from 'react';
import { Post } from '../../restManager';
import { Redirect } from 'react-router';

export const SignIn = () => {
    const [name, setName] = useState('');
    const [password, setPassword] = useState('');
    const [referrer, setReferrer] = useState(null);

    const onSuccessSignedIn = (response) => {
        if (response.status === 200) {
            localStorage.setItem('tokenKey', response.access_token);
            localStorage.setItem('refresh_token', response.refresh_token);
            setReferrer('/project/all');
        }
    }

    const trySignIn = (event) => {
        event.preventDefault();

        const body = { Login: name, Pass: password };

        Post('api/account/signin', body, (response) => {
            response.json().then(onSuccessSignedIn);
        });
    }

    return (
        <div>
            {referrer && <Redirect to={referrer} />}
            <form onSubmit={trySignIn}>
                <p>
                    <input required type="text" placeholder="Логин" value={name} onChange={(e) => setName(e.target.value)} />
                </p>
                <p>
                    <input required type="password" placeholder="Пароль" value={password} onChange={(e) => setPassword(e.target.value)} />
                </p>
                <input type="submit" value="Авторизоваться" />
                </form>
            </div>  
          );
}