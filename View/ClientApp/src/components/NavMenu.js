import React, { Component } from 'react';
import { Collapse, Container, Navbar, NavbarBrand, NavbarToggler, NavItem, NavLink } from 'reactstrap';
import { Link } from 'react-router-dom';
import './NavMenu.css';

export class NavMenu extends Component {
  static displayName = NavMenu.name;

  constructor (props) {
    super(props);

    this.toggleNavbar = this.toggleNavbar.bind(this);
      this.state = {
        auth: false,
      collapsed: true
    };
  }
    componentDidMount() {
        this.setState({ auth: this.isAuth() });
    }

  toggleNavbar () {
      this.setState({
      collapsed: !this.state.collapsed
    });
  }

    isAuth() {
        var tt = false;
        var xhr = new XMLHttpRequest();
        xhr.open("get", "WeatherForecast/IsAuth", false);
        xhr.onload = function () {
            if (xhr.status == 200 || xhr.status == 304) {
                tt = true;
            }
        }.bind(this);
        xhr.send();
        return tt;
    }

    Signout() {
        var xhr = new XMLHttpRequest();
        xhr.open("get", "WeatherForecast/SignOut", false);
        xhr.send();
        this.setState({ collapsed: false, auth: false });
    }

    render() {
        var auth = this.state.auth;
        if (auth) {
            return (
                <header>
                    <Navbar className="navbar-expand-sm navbar-toggleable-sm ng-white border-bottom box-shadow mb-3" light>
                        <Container>
                            <NavbarBrand tag={Link} to="/">View</NavbarBrand>
                            <NavbarToggler onClick={this.toggleNavbar} className="mr-2" />
                            <Collapse className="d-sm-inline-flex flex-sm-row-reverse" isOpen={!this.state.collapsed} navbar>
                                <ul className="navbar-nav flex-grow">
                                    <NavItem>
                                        <NavLink tag={Link} className="text-dark" to="/">Home</NavLink>
                                    </NavItem>
                                    <NavItem>
                                        <NavLink tag={Link} onClick={x => { this.Signout(); }} className="text-dark" to="/">SignOut</NavLink>
                                    </NavItem>
                                </ul>
                            </Collapse>
                        </Container>
                    </Navbar>
                </header>
            );
        }
        else {
            return (
                <header>

                    <Navbar className="navbar-expand-sm navbar-toggleable-sm ng-white border-bottom box-shadow mb-3" light>
                        <Container>
                            <NavbarBrand tag={Link} to="/">View</NavbarBrand>
                            <NavbarToggler onClick={this.toggleNavbar} className="mr-2" />
                            <Collapse className="d-sm-inline-flex flex-sm-row-reverse" isOpen={!this.state.collapsed} navbar>
                                <ul className="navbar-nav flex-grow">
                                    <NavItem>
                                        <NavLink tag={Link} className="text-dark" to="/Auth">Auth</NavLink>
                                    </NavItem>
                                </ul>
                            </Collapse>
                        </Container>
                    </Navbar>
                </header>);
        }
  }
}
