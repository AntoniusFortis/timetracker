import React, { Component, useState, useCallback, useEffect } from 'react';
import { Collapse, Container, Navbar, NavbarBrand, NavbarToggler, NavItem, NavLink } from 'reactstrap';
import { Link } from 'react-router-dom';
import './NavMenu.css';
import { hasAuthorized, getToken } from '../Account';
import moment from 'moment'
import { SignalR_Provider } from '../../signalr/SignalR_Provider';
import cogoToast from 'cogo-toast';

const TrackingTimer = (props) => {
    const [elapsed, setElapsed] = useState(0);
    const [timer, setTimer] = useState(null);

    const cleanupTimer = useCallback(() => {
        clearInterval(timer);
        setTimer(null);
    });

    const tick = () => {
        const now = moment.utc();
        setElapsed(now.diff(props.start));
    };

    useEffect(() => {
        const timerId = setInterval(tick, 1000);
        setTimer(timerId);
        return () => cleanupTimer();
    }, []);

    const displayTime = moment.utc(elapsed);

    return <div style={{ display: 'inline' }}>{displayTime.format('HH:mm:ss')}</div>;
}

class HeaderMenuTracking extends Component {
    timerId = 0;

    constructor(props) {
        super(props);

        this.state = {
            hubConnection: null,
            worktask: {},
            time: undefined,
            isTracked: false,
            offset: moment().utcOffset()
        };
    }

    async start() {
        const { hubConnection } = this.state;

        try {
            await hubConnection.start();
        } catch (err) {
            console.log(err);
            setTimeout(() => this.start(), 5000);
        }
    };

    showMessage = (text) => {
        if (text.trim() !== '') {
            cogoToast.info(text);
        }
    }

    onActiveTrackingReceive = (istracking, worktask, started, message) => {
        this.showMessage(message);

        if (!istracking) {
            this.setState({
                isTracked: false,
                worktask: {},
                time: {}
            });
            return;
        }
        
        const startTime = moment.utc(worktask.startedTime);

        this.setState({
            isTracked: istracking,
            worktask: worktask,
            time: startTime
        });
    }

    componentDidMount() {
        SignalR_Provider.callbacks.push(this.onActiveTrackingReceive);

        const hubConnection = SignalR_Provider.getConnection(getToken());

        this.setState({ hubConnection }, () => {
            this.start();
        });
    }

    render() {
        return (
            <div>
                {this.state.isTracked && <div>Отслеживается: <b>{this.state.worktask.worktask.title}</b> (<TrackingTimer start={this.state.time} />)</div>}
            </div>
        );
    }
}

export class NavMenu extends Component {
    static Auth = false;

    constructor(props) {
        super(props);
        NavMenu.Auth = hasAuthorized();
        this.state = {
            collapsed: true
        };
    }


    toggleNavbar = (event) => {
        this.setState({
            collapsed: !this.state.collapsed
        });
    }

    Signout = (event) => {
        SignalR_Provider.callbacks = [];
        SignalR_Provider.connection.stop();
        SignalR_Provider.connection = null;
        SignalR_Provider.trackingIsOn = false;

        localStorage.removeItem('tokenKey');
        NavMenu.Auth = false;
        this.setState({ collapsed: false });
    }

    render() {
        let tracking = NavMenu.Auth ? (<HeaderMenuTracking auth={NavMenu.Auth} />) : <div />;

        let menu = NavMenu.Auth ? (
            <ul className="navbar-nav flex-grow">
                <NavItem>
                    <NavLink className="text-dark" href="/swagger">Time Tracker API</NavLink>
                </NavItem>
                <NavItem>
                    <NavLink tag={Link} className="text-dark" to="/report/">Отчёты</NavLink>
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
                            <NavbarBrand tag={Link} to="/">Time Tracker</NavbarBrand>
                            <NavbarToggler onClick={this.toggleNavbar} className="mr-2" />
                            {tracking}
                            <Collapse className="d-sm-inline-flex flex-sm-row-reverse" isOpen={!this.state.collapsed} navbar>
                                {menu}
                            </Collapse>
                        </Container>
                    </Navbar>
                </header>);
    }
}
