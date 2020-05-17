import React, { useState } from 'react';
import { Post } from '../../restManager';

export const ProjectAdd = () => {
    const [title, setTitle] = useState('');
    const [descr, setDescr] = useState('');

    const tryAddProject = (event) => {
        event.preventDefault();

        const body = { Title: title, Description: descr };

        Post("api/project/add", body, (response) => {
                if (response.status === 200) {
                    window.location.href = "/project/all";
                }
            });
    }

    return (
        <form onSubmit={tryAddProject}>
            <p>
                <input required type="text" placeholder="Название" value={title} onChange={(e) => setTitle(e.target.value)} />
            </p>
            <p>
                <input type="text" placeholder="Описание" value={descr} onChange={(e) => setDescr(e.target.value)} />
            </p>
            <input type="submit" value="Создать проект" />
        </form>);
}