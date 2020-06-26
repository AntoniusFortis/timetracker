import React, { useState, useEffect, useCallback } from 'react';
import { Get, Post } from '../../restManager';
import { Redirect } from 'react-router';
import cogoToast from 'cogo-toast';

const InputField = (props) => {
    return (
        <div style={{ paddingTop: '15px' }}>
            <input required={props.required} minLength={props.minLength} maxLength={props.maxLength} style={{ width: '270px', textAlign: 'center' }} type={props.type} placeholder={props.placeholder} value={props.value} onChange={props.onChange} />
        </div>);
}

export const ProjectUpdate = (props) => {
    const [title, setTitle] = useState('');
    const [descr, setDescr] = useState('');
    const [projectId, setProjectId] = useState(undefined);
    const [referrer, setReferrer] = useState(null);

    const getProjectsData = useCallback(() => {
        Get("api/project/get?id=" + props.match.params.projectId, (response) => {
            response.json()
                .then(result => {
                    setTitle(result.project.Title);
                    setDescr(result.project.Description);
                    setProjectId(result.project.Id);
                });
        });
    }, []);

    useEffect(() => getProjectsData(), []);

    const tryUpdateProject = useCallback((event) => {
        event.preventDefault();

        const body = { Project: { Id: projectId, Title: title.trim(), Description: descr.trim() } };
        Post('api/project/update', body, (response) => {
            response.json().then(result => {
                if (result.status === 200) {
                    setReferrer('/project/all/');
                }
                else {
                    cogoToast.error(result.message);
                }
            });
        });
    }, [projectId, title, descr]);

    return (
        <div style={{ width: '300px', margin: '0 auto', paddingTop: '125px', height: '300px', display: 'block' }}>
            {referrer && <Redirect to={referrer} />}
            <form style={{ width: '400px' }} onSubmit={tryUpdateProject}>
                <InputField required={true} minLength="3" maxLength="50" type="text" placeholder="Название" value={title} onChange={(event) => setTitle(event.target.value)} />
                <InputField required={false} minLength="1" maxLength="250" type="text" placeholder="Описание" value={descr} onChange={(event) => setDescr(event.target.value)} />
                <input style={{ display: 'block', width: '270px', marginTop: '15px' }} type="submit" value="Изменить" />
            </form>
        </div>
    );
}