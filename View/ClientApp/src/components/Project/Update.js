import React, { useState, useEffect } from 'react';
import { Get, Post } from '../../restManager';
import { Redirect } from 'react-router';

export const ProjectUpdate = (props) => {
    const [title, setTitle] = useState('');
    const [descr, setDescr] = useState('');
    const [projectId, setProjectId] = useState(undefined);
    const [referrer, setReferrer] = useState(null);

    const getProjectsData = () => {
        Get("api/project/get?id=" + props.match.params.projectId, (response) => {
            response.json()
                .then(result => {
                    setTitle(result.project.Title);
                    setDescr(result.project.Description);
                    setProjectId(result.project.Id);
                });
        });
    }
    useEffect(() => getProjectsData(), []);

    const tryUpdateProject = (event) => {
        event.preventDefault();

        const body = { Project: { Id: projectId, Title: title.trim(), Description: descr.trim() } };
        Post("api/project/update", body, (response) => {
                if (response.status === 200) {
                    setReferrer('/project/get/' + props.match.params.projectId);
                }
            });
    }

    return (
        <div>
            {referrer && <Redirect to={referrer} />}
        <form onSubmit={tryUpdateProject}>
            <p>
                    <input required minLength="3"  type="text" placeholder="Название" value={title} onChange={(e) => setTitle(e.target.value)} />
            </p>
            <p>
                <input type="text" placeholder="Описание" value={descr} onChange={(e) => setDescr(e.target.value)} />
            </p>
            <input type="submit" value="Изменить проект" />
            </form>
        </div>
    );
}