import React, { useState, useEffect } from 'react';
import { Get } from '../../restManager';

export const MyPageGet = () => {
    //const [password, setPassword] = useState('');
    //const [password2, setPassword2] = useState('');
    const [firstName, setFirstName] = useState('');
    const [surname, setSurname] = useState('');
    const [middleName, setMiddleName] = useState(undefined);
    const [city, setCity] = useState(undefined);
    const [birthDate, setBirthDate] = useState(undefined);
    const [email, setEmail] = useState('');

    const getInfo = () => {
        Get('api/mypage/get', (response) => {
            response.json().then(result => {
                setFirstName(result.user.FirstName);
                setSurname(result.user.Surname);
                setMiddleName(result.user.MiddleName);
            });
        });
    }

    useEffect(() => getInfo(), []);

    return (
        <form>
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
        </form>);
}