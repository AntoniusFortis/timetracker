import React, { useState, useCallback } from 'react';
import { Post } from '../../restManager';
import { Redirect } from 'react-router';
import { setToken } from '../Account'
import { NavMenu } from '../NavMenu';

const InputField = (props) => {
    return (
        <div style={{ paddingTop: '15px' }}>
            <input required style={{ width: '270px', textAlign: 'center' }} type={props.type} placeholder={props.placeholder} value={props.value} onChange={props.onChange} />
        </div>);
}

export const SignIn = () => {
    const [name, setName] = useState('');
    const [password, setPassword] = useState('');
    const [referrer, setReferrer] = useState(null);

    const onResponse = useCallback((response) => {
        if (response.status === 200) {
            NavMenu.Auth = true;
            setToken(response.access_token);
            localStorage.setItem('refresh_token', response.refresh_token);
            setReferrer('/project/all');
        }
        else {
            alert(response.message);
        }
    }, []);

    const trySignIn = useCallback((event) => {
        event.preventDefault();

        const body = { login: name, pass: password };

        Post('api/account/signin', body, (response) => {
            response.json().then(onResponse);
        });
    }, [name, password]);

    return (
        <div style={{ width: '300px', margin: '0 auto', paddingTop: '125px', height: '300px', display: 'block' }}>
            {referrer && <Redirect to={referrer} />}
            <form style={{ width: '400px' }} onSubmit={trySignIn}>
                <InputField type="text" placeholder="Логин" value={name} onChange={(event) => setName(event.target.value)} />
                <InputField type="password" placeholder="Пароль" value={password} onChange={(event) => setPassword(event.target.value)} />
                <input style={{ display: 'block', width: '270px', marginTop: '15px' }} type="submit" value="Авторизоваться" />
            </form>
        </div>  
       );
}