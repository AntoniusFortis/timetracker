import React, { useState } from 'react';
import { Post } from '../../restManager';
import { Redirect } from 'react-router';

export const ProjectAdd = () => {
    const [title, setTitle] = useState('');
    const [descr, setDescr] = useState('');
    const [referrer, setReferrer] = useState(null);

    const tryAddProject = (event) => {
        event.preventDefault();

        const body = { title: title, description: descr };

        Post("api/project/add", body, (response) => {
            if (response.status === 200) {
                setReferrer('/project/all');
            }
        });
    }

    return (
        <div>
            {referrer && <Redirect to={referrer} />}

        <form onSubmit={tryAddProject}>
                <p>
                    <input required minLength="5" maxLength="50" type="text" placeholder="Название" value={title} onChange={(e) => setTitle(e.target.value)} />
            </p>
                <p>
                    <input type="text" maxLength="250" placeholder="Описание" value={descr} onChange={(e) => setDescr(e.target.value)} />
            </p>
            <input type="submit" value="Создать проект" />
            </form>
        </div>);
}