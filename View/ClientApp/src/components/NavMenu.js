import React, { Component } from 'react';
import { Collapse, Container, Navbar, NavbarBrand, NavbarToggler, NavItem, NavLink } from 'reactstrap';
import { Link } from 'react-router-dom';
import './NavMenu.css';
import { hasAuthorized } from './Account';

import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr';

export class TaskTracking2 extends Component {
    constructor(props) {
        super(props);

        this.state = {
            hubConnection: null,
            worktask: {},
            time: '',
            isTracked: false
        };
    }

    showMessage(text) {
        if (text.trim() !== '') {
            alert(text);
        }
    }

    componentDidMount() {
        const token = localStorage.getItem('tokenKey');

        const hubConnection = new HubConnectionBuilder()
            .withUrl("/trackingHub", { accessTokenFactory: () => token })
            .withAutomaticReconnect()
            .configureLogging(LogLevel.Information)
            .build();

        this.setState({ hubConnection }, () => {
            this.state.hubConnection.on('startTracking', (message, status, obj) => {
                this.setState({ isTracked: true, worktask: obj });
            });

            this.state.hubConnection.on('stopTracking', (receivedMessage, status) => {
                this.setState({ isTracked: false, worktask: {} });
            });

            this.state.hubConnection.on('getActiveTracking', (istracking, obj, time) => {
                this.setState({
                    isTracked: istracking,
                    worktask: obj,
                    time: time
                });
            });

            this.state.hubConnection.start()
                .catch(err => console.log(err));
        });

    }

    render() {
        return (
            <div>
                {this.state.isTracked && <div>Идёт отслеживание: <b>{this.state.worktask.task.title}</b> ({this.state.time})</div>}
            </div>
        );
    }
}

export class NavMenu extends Component {
    constructor(props) {
        super(props);

        this.state = {
            auth: false,
            collapsed: true,
        };
    }

    componentDidMount() {
        this.setState({ auth: hasAuthorized() });
    }


    toggleNavbar = (event) => {
        this.setState({
            collapsed: !this.state.collapsed
        });
    }

    Signout = (event) => {
        localStorage.removeItem('tokenKey');
        this.setState({ collapsed: false, auth: false });
    }

    render() {
        const auth = this.state.auth;

        let menu = auth ? (
            <ul className="navbar-nav flex-grow">
                <NavItem>
                    <NavLink tag={Link} className="text-dark" to="/stat/">Статистика</NavLink>
                </NavItem>
                <NavItem>
                    <NavLink tag={Link} className="text-dark" to="/project/all">Проекты</NavLink>
                </NavItem>
                <NavItem>
                    <NavLink tag={Link} className="text-dark" to="/project/add">Новый проект</NavLink>
                </NavItem>
                <NavItem>
                    <NavLink tag={Link} className="text-dark" to="/mypage/">Моя страница</NavLink>
                </NavItem>
                <NavItem>
                    <NavLink tag={Link} onClick={this.Signout} className="text-dark" to="/">Выход</NavLink>
                </NavItem>
            </ul> )
            :  ( <ul className="navbar-nav flex-grow">
                    <NavItem>
                        <NavLink tag={Link} className="text-dark" to="/account/signin">Авторизация</NavLink>
                    </NavItem>
                    <NavItem>
                        <NavLink tag={Link} className="text-dark" to="/account/signup">Регистрация</NavLink>
                    </NavItem>
            </ul>);

            return (
                <header>
                    <Navbar className="navbar-expand-sm navbar-toggleable-sm ng-white border-bottom box-shadow mb-3" light>
                        <Container>
                            <NavbarBrand tag={Link} to="/">Timetracker</NavbarBrand>
                            <TaskTracking2 />
                            <NavbarToggler onClick={this.toggleNavbar} className="mr-2" />
                            <Collapse className="d-sm-inline-flex flex-sm-row-reverse" isOpen={!this.state.collapsed} navbar>
                                {menu}
                            </Collapse>
                        </Container>
                    </Navbar>
                </header>);
    }
}
