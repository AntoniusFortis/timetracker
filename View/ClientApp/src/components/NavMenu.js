import React, { Component, PureComponent } from 'react';
import { Collapse, Container, Navbar, NavbarBrand, NavbarToggler, NavItem, NavLink } from 'reactstrap';
import { Link } from 'react-router-dom';
import './NavMenu.css';
import { hasAuthorized } from './Account';
import moment from 'moment'
import { SignalR_Provider } from '../signalr/SignalR_Provider';

class TrackingTimer extends PureComponent {
    constructor(props) {
        super(props);

        this.state = {
            elapsed: 0
        };
    }

    componentDidMount() {
        this.timer = setInterval(this.tick, 1000);
    }

    componentWillUnmount() {
        clearInterval(this.timer);
    }

    tick = () => {
        this.setState({ elapsed: moment().utc() - this.props.start });
    }

    render() {
        const displayTime = moment(this.state.elapsed).utc();

        return <div style={{ display: 'inline' }}>{displayTime.format('HH:mm:ss')}</div>;
    }
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

    showMessage(text) {
        if (text.trim() !== '') {
            alert(text);
        }
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

    onClose = async (error) => {
        await this.start();
    }

    componentWillUnmount() {
        this.state.hubConnection.off('getActiveTracking');
    }

    onActiveTrackingReceive = (istracking, worktask, started, message) => {
        if (!istracking) {
            this.setState({
                isTracked: false,
                worktask: {},
                time: {}
            });
            return;
        }

        let startTime;
        if (started) {
            startTime = moment(worktask.startedTime).utcOffset(this.state.offset);
        }
        else {
            startTime = moment(worktask.startedTime).add(this.state.offset, 'm');
        }

        this.setState({
            isTracked: istracking,
            worktask: worktask,
            time: startTime
        });
    }

    componentDidMount() {
        const token = localStorage.getItem('tokenKey');

        const connectionData = {
            token: token,
            onClose: this.onClose,
            onActiveTrackingReceive: this.onActiveTrackingReceive
        };

        const hubConnection = SignalR_Provider.getConnection(connectionData);

        this.setState({ hubConnection }, () => {
            this.start();
        });
    }

    render() {
        return (
            <div>
                {this.state.isTracked && <div>Идёт отслеживание: <b>{this.state.worktask.task.title}</b> (<TrackingTimer start={this.state.time} />)</div>}
            </div>
        );
    }
}

export class NavMenu extends Component {
    constructor(props) {
        super(props);

        this.state = {
            collapsed: true,
        };
    }


    toggleNavbar = (event) => {
        this.setState({
            collapsed: !this.state.collapsed
        });
    }

    Signout = (event) => {
        localStorage.removeItem('tokenKey');
        this.setState({ collapsed: false });
    }

    render() {
        const auth = hasAuthorized();

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
                            <NavbarBrand tag={Link} to="/">Time Tracker</NavbarBrand>
                            <HeaderMenuTracking />
                            <NavbarToggler onClick={this.toggleNavbar} className="mr-2" />
                            <Collapse className="d-sm-inline-flex flex-sm-row-reverse" isOpen={!this.state.collapsed} navbar>
                                {menu}
                            </Collapse>
                        </Container>
                    </Navbar>
                </header>);
    }
}
