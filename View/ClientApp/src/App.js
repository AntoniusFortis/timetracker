import React, { Component } from 'react';
import { Route } from 'react-router';
import { Layout } from './components/Layout';
import { Home } from './components/Home';
import { SignIn, SignUp } from './components/Account';
import { AddProject, CurrentProject } from './components/Project';
import { Projects } from './components/Projects';

import './custom.css'
import { editProject } from './components/editProject';

export default class App extends Component {
  static displayName = App.name;

  render () {
    return (
      <Layout>
            <Route exact path='/' component={Home} />
            <Route path='/signin' component={SignIn} />
            <Route exact path='/project/:projectId' component={CurrentProject} />
            <Route path='/editproject/:projectId' component={editProject} />

            <Route path='/Reg' component={SignUp} />
            <Route path='/projects' component={Projects} />
            <Route path='/addproject' component={AddProject} />
      </Layout>
    );
  }
}
