import React from 'react';
import { hasAuthorized } from './Account';
import { Redirect } from 'react-router-dom';

export const Home = () => {
    return <div>{hasAuthorized() ? <Redirect to="/project/all" /> : <Redirect to="/account/signin/" />}</div>;
}