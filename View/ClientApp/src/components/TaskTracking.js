import React, { Component } from 'react';
import { getToken } from './Account'
import { SignalR_Provider } from '../signalr/SignalR_Provider';

export class TaskTracking extends Component {
    constructor(props) {
        super(props);

        this.state = {
            hubConnection: null,
            buttonToggle: true
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

    onActiveTrackingReceive = (istracking, worktask, started, message) => {
        this.setState({ buttonToggle: !istracking });
        this.showMessage(message);
    }

    onClose = async (error) => {
        this.setState({ buttonToggle: false });
        await this.start();
    }

    componentWillUnmount() {
        this.state.hubConnection.off('getActiveTracking');
    }

    componentDidMount() {
        const connectionData = {
            token: getToken(),
            onClose: this.onClose,
            onActiveTrackingReceive: this.onActiveTrackingReceive
        };

        const hubConnection = SignalR_Provider.getConnection(connectionData);

        this.setState({ hubConnection }, () => {
            this.start();
        });
    }

    startTracking = (event) => {
        this.state.hubConnection
            .invoke('StartTracking', this.props.worktaskId)
            .catch(err => {
                console.error(err);
                this.setState({ buttonToggle: false });
            });
    };

    stopTracking = (event) => {
        this.state.hubConnection
            .invoke('StopTracking')
            .catch(err => {
                console.error(err);
                this.setState({ buttonToggle: true });
            });
    };

    render() {
        return (
            <div>
                <div style={{ display: (this.state.buttonToggle ? 'block' : 'none') }}>
                    <button onClick={this.startTracking}>Начать отслеживание</button>
                </div>
                <button style={{ display: (this.state.buttonToggle ? 'none' : 'block') }} onClick={this.stopTracking}>Остановить отслеживание</button>
            </div>
        );
    }
}