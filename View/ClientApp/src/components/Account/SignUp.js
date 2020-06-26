import React, { useState, useCallback } from 'react';
import { Post } from '../../restManager';
import { Redirect } from 'react-router';
import cogoToast from 'cogo-toast';

const InputField = (props) => {
    return (
        <div style={{ paddingTop: '15px' }}>
            <input required={props.required} minLength={props.minLength} maxLength={props.maxLength} style={{ width: '270px', textAlign: 'center' }} type={props.type} placeholder={props.placeholder} value={props.value} onChange={props.onChange} />
        </div>);
}

export const SignUp = () => {
    const [name, setName] = useState('');
    const [password, setPassword] = useState('');
    const [password2, setPassword2] = useState('');
    const [firstName, setFirstName] = useState('');
    const [surname, setSurname] = useState('');
    const [middleName, setMiddleName] = useState('');
    const [city, setCity] = useState('');
    const [birthDate, setBirthDate] = useState(undefined);
    const [email, setEmail] = useState('');
    const [referrer, setReferrer] = useState(null);

    const onResponse = useCallback((response) => {
        if (response.status === 200) {
            setReferrer('/account/signin');
        }
        else {
            cogoToast.error(response.message);
        }
    }, []);

    const trySignUp = useCallback((event) => {
        event.preventDefault();
        const { hide } = cogoToast.loading('Идёт отправка данных');

        if (password !== password2) {
            hide();
            cogoToast.error('Пароли не совпадают');
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

        Post('api/account/signup', body, (response) => {
            hide();
            response.json().then(onResponse);
        });
    }, [name, password, password2, firstName, surname, middleName, city, birthDate, email]);

    return (
        <div style={{ width: '300px', margin: '0 auto', paddingTop: '60px', height: '300px', display: 'block' }}>
            {referrer && <Redirect to={referrer} />}
            <form onSubmit={trySignUp}>
                <InputField type="text" required={true} minLength="4" maxLength="20" placeholder="Логин" value={name} onChange={(e) => setName(e.target.value)} />
                <InputField type="password" required={true} minLength="5" maxLength="20" placeholder="Пароль" value={password} onChange={(e) => setPassword(e.target.value)} />
                <InputField type="password" required={true} minLength="5" maxLength="20" placeholder="Повторите пароль" value={password2} onChange={(e) => setPassword2(e.target.value)} />
                <InputField type="text" required={true} minLength="2" maxLength="30" placeholder="Имя" value={firstName} onChange={(e) => setFirstName(e.target.value)} />
                <InputField type="text" required={true} minLength="2" maxLength="30" placeholder="Фамилия" value={surname} onChange={(e) => setSurname(e.target.value)} />
                <InputField type="text" required={false} minLength="2" maxLength="30" placeholder="Отчество" value={middleName} onChange={(e) => setMiddleName(e.target.value)} />
                <InputField type="email" required={true} minLength="3" maxLength="80" placeholder="Email" value={email} onChange={(e) => setEmail(e.target.value)} />
                <InputField type="text" required={false} minLength="3" maxLength="50" placeholder="Город" value={city} onChange={(e) => setCity(e.target.value)} />
                <div style={{ paddingTop: '15px' }}>
                    <input style={{ width: '270px', textAlign: 'center' }} type="date" placeholder="Дата рождения" value={birthDate} onChange={(e) => setBirthDate(e.target.value)} />
                </div>
                <input style={{ display: 'block', width: '270px', marginTop: '15px' }} type="submit" value="Зарегистрироваться" />
            </form>
        </div>);
}