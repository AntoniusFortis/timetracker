import React, { PureComponent, Component } from 'react';
import { Route } from 'react-router';
import { Layout } from './components/Layout';
import { Home } from './components/Home';

import { SignIn } from './components/Account/SignIn';
import { SignUp } from './components/Account/SignUp';

import { ProjectGetAll } from './components/Project/GetAll';
import { ProjectGet } from './components/Project/Get';
import { ProjectAdd } from './components/Project/Add';
import { ProjectUpdate } from './components/Project/Update';
import { ProjectInvite } from './components/Project/Invite';

import { TaskGet } from './components/Task/Get';
import { TaskUpdate } from './components/Task/Update';
import { TaskAdd } from './components/Task/Add';

import { MyPageGet } from './components/MyPage/Get';

import './custom.css'

export default class App extends Component {
  render () {
    return (
      <Layout>
            <Route exact path='/' component={Home} />

            <Route exact path='/account/signin' component={SignIn} />
            <Route exact path='/account/signup' component={SignUp} />

            <Route exact path='/project/all' component={ProjectGetAll} />
            <Route exact path='/project/get/:projectId' component={ProjectGet} />
            <Route exact path='/project/add' component={ProjectAdd} />
            <Route exact path='/project/update/:projectId' component={ProjectUpdate} />
            <Route exact path='/project/invite/:projectId' component={ProjectInvite} />

            <Route exact path='/task/get/:taskId' component={TaskGet} />
            <Route exact path='/task/update/:taskId' component={TaskUpdate} />
            <Route exact path='/task/add/:projectId' component={TaskAdd} />

            <Route exact path='/mypage/' component={MyPageGet} />
      </Layout>
    );
  }
}
