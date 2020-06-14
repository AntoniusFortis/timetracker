import React, { useState, useCallback } from 'react';
import { Post } from '../../restManager';
import { Redirect } from 'react-router';

const InputField = (props) => {
    return (
        <div style={{ paddingTop: '15px' }}>
            <input required={props.required} minLength={props.minLength} maxLength={props.maxLength} style={{ width: '270px', textAlign: 'center' }} type={props.type} placeholder={props.placeholder} value={props.value} onChange={props.onChange} />
        </div>);
}

export const ProjectAdd = () => {
    const [title, setTitle] = useState('');
    const [descr, setDescr] = useState('');
    const [referrer, setReferrer] = useState(null);

    const tryAddProject = useCallback((event) => {
        event.preventDefault();

        const body = { title: title, description: descr };

        Post('api/project/add', body, (response) => {
            response.json().then(result => {
                if (result.status === 200) {
                    setReferrer('/project/all');
                }
                else {
                    alert(result.message);
                }
            });
        });
    }, [title, descr])

    return (
        <div style={{ width: '300px', margin: '0 auto', paddingTop: '125px', height: '300px', display: 'block' }}>
            {referrer && <Redirect to={referrer} />}
            <form style={{ width: '400px' }} onSubmit={tryAddProject}>
                <InputField required={true} minLength="5" maxLength="50" type="text" placeholder="Название" value={title} onChange={(event) => setTitle(event.target.value)} />
                <div style={{ paddingTop: '15px' }}>
                    <textarea style={{ width: '270px', textAlign: 'center' }} maxLength="250" placeholder="Описание" value={descr} onChange={(e) => setDescr(e.target.value)} />
                </div>
                <input style={{ display: 'block', width: '270px', marginTop: '15px' }} type="submit" value="Создать" />
            </form>
        </div>);
}