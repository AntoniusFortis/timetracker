import React, { Component } from 'react';
import { Container } from 'reactstrap';
import { NavMenu } from '../Menu/NavMenu';

export class Layout extends Component {
  render () {
    return (
      <div>
        <NavMenu />
        <Container>
          {this.props.children}
        </Container>
      </div>
    );
  }
}
