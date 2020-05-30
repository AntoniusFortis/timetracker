import React, { useState } from 'react';
import { Post } from '../../restManager';

export const TaskAdd = (props) => {
    const [title, setTitle] = useState('');
    const [description, setDescription] = useState('');
    const [duration, setDuration] = useState('');

    const tryAddTask = (event) => {
        event.preventDefault();

        const worktask = {
            Title: title,
            Description: description,
            Duration: parseInt(duration),
            StateId: 1,
            ProjectId: parseInt(props.match.params.projectId),
            CreatedDate: new Date()
        };

        const body = { worktask: worktask };

        Post('api/task/add', body, (response) => {
                if (response.status === 200) {
                    window.location.href = '/project/get/' + props.match.params.projectId;
                }
        });
    }

    return (
        <form onSubmit={tryAddTask}>
            <p>
                <input required type="text" placeholder="Название" value={title} onChange={(e) => setTitle(e.target.value)} />
            </p>
            <p>
                <input type="text" placeholder="Описание" value={description} onChange={(e) => setDescription(e.target.value)} />
            </p>
            <p>
                <input required type="text" placeholder="Длительность" value={duration} onChange={(e) => setDuration(e.target.value)} />
            </p>
            <input type="submit" value="Добавить" />
        </form>
    );
}