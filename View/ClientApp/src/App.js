import React, { Component } from 'react';
import { Route } from 'react-router';
import { Layout } from './components/Layout';
import { Home } from './components/Home';
import { Auth, Registration } from './components/Auth';
import { Projects, AddProject } from './components/Project';

import './custom.css'

export default class App extends Component {
  static displayName = App.name;

  render () {
    return (
      <Layout>
            <Route exact path='/' component={Home} />
            <Route path='/auth' component={Auth} />
            <Route path='/Reg' component={Registration} />
            <Route path='/projects' component={Projects} />
            <Route path='/addproject' component={AddProject} />
      </Layout>
    );
  }
}
