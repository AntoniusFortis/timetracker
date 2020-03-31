import React, { Component } from 'react';
import { Collapse, Container, Navbar, NavbarBrand, NavbarToggler, NavItem, NavLink } from 'reactstrap';
import { Link } from 'react-router-dom';
import './NavMenu.css';
import { hasAuthorized } from './Account';
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr';

export class NavMenu extends Component {
  static displayName = NavMenu.name;

    constructor(props) {
        super(props);

        this.state = {
            auth: false,
            collapsed: true,
            message: '',
            hubConnection: null,
            buttonToggle: true
        };

        this.toggleNavbar = this.toggleNavbar.bind(this);
    }

    componentDidMount() {

        const hubConnection = new HubConnectionBuilder()
            .withUrl("/chat", { accessTokenFactory: () => localStorage.getItem('tokenKey') })
            .configureLogging(LogLevel.Information)
            .build();
            
        this.setState({ hubConnection }, () => {
            this.state.hubConnection
                .start()
                .then(() => console.log('Connection started!'))
                .catch(err => console.log('Error while establishing connection :('));

            this.state.hubConnection.on('sendToAll', (receivedMessage, status) => {
                if (status === 200) {
                    this.setState({ buttonToggle: false });
                }
                else {
                    this.setState({ message: '', buttonToggle: true });
                }
                alert(receivedMessage);
            });

            this.state.hubConnection.on('StopTracking', (receivedMessage, status) => {
                this.setState({ buttonToggle: true });
                alert(receivedMessage);
            });
        });

        this.setState({ auth: hasAuthorized() });
    }

    sendMessage = () => {
        this.state.hubConnection
            .invoke('sendToAll', this.state.message)
            .catch(err => {
                console.error(err);
                this.setState({ message: '', buttonToggle: true });
            });
    };

    stopTracking = () => {
        this.state.hubConnection
            .invoke('StopTracking', this.state.message)
            .catch(err => {
                console.error(err);
                this.setState({ message: '', buttonToggle: true });
            });

    };

    toggleNavbar() {
        this.setState({
            collapsed: !this.state.collapsed
        });
    }

    Signout() {
        localStorage.removeItem('tokenKey');
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
                                <div style={{ display: (this.state.buttonToggle ? 'block' : 'none') }}>
                                <input 
                                    type="text"
                                    value={this.state.message}
                                    onChange={e => this.setState({ message: e.target.value })}
                                />

                                    <button onClick={this.sendMessage}>Send</button>
                                </div>
                                <button style={{ display: (this.state.buttonToggle ? 'none' : 'block') }} onClick={this.stopTracking}>Stop tracking</button>

                                <ul className="navbar-nav flex-grow">
                                    <NavItem>
                                        <NavLink tag={Link} className="text-dark" to="/">Home</NavLink>
                                    </NavItem>
                                    <NavItem>
                                        <NavLink tag={Link} onClick={x => { this.Signout(); }} className="text-dark" to="/">SignOut</NavLink>
                                    </NavItem>
                                    <NavItem>
                                        <NavLink tag={Link} className="text-dark" to="/project/all">Проекты</NavLink>
                                    </NavItem>
                                    <NavItem>
                                        <NavLink tag={Link} className="text-dark" to="/project/add">Новый проект</NavLink>
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
                                        <NavLink tag={Link} className="text-dark" to="/account/signin">Авторизация</NavLink>
                                    </NavItem>
                                    <NavItem>
                                        <NavLink tag={Link} className="text-dark" to="/account/signup">Регистрация</NavLink>
                                    </NavItem>
                                </ul>
                            </Collapse>
                        </Container>
                    </Navbar>
                </header>);
        }
  }
}
