import React, { useState } from 'react';
import { Post } from '../../restManager';

const InputField = (props) => {
    return (
        <div style={{ paddingTop: '15px' }}>
            <input required={props.required} minLength={props.minLength} maxLength={props.maxLength} style={{ width: '270px', textAlign: 'center' }} type={props.type} placeholder={props.placeholder} value={props.value} onChange={props.onChange} />
        </div>);
}

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
        <div style={{ width: '300px', margin: '0 auto', paddingTop: '125px', height: '300px', display: 'block' }}>
            <form style={{ width: '400px' }} onSubmit={tryAddTask}>
                <InputField required={true} minLength="5" maxLength="50" type="text" placeholder="Название" value={title} onChange={(e) => setTitle(e.target.value)} />
                <div style={{ paddingTop: '15px' }}>
                    <input style={{ width: '270px', textAlign: 'center' }} type="number" min="1" max="100" placeholder="Длительность" value={duration} onChange={(e) => setDuration(e.target.value)} />
                </div>
                <div style={{ paddingTop: '15px' }}>
                    <textarea style={{ width: '270px', textAlign: 'center' }} maxLength="250" placeholder="Описание" value={description} onChange={(e) => setDescription(e.target.value)} />
                </div>
                <input style={{ display: 'block', width: '270px', marginTop: '15px' }} type="submit" value="Добавить" />
            </form>
        </div>
    );
}