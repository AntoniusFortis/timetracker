import React, { Component } from 'react';
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr';

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

    componentDidMount() {
        const token = localStorage.getItem('tokenKey');
        const hubConnection = new HubConnectionBuilder()
            .withUrl("/trackingHub?projectId=" + this.props.projectId, { accessTokenFactory: () => token })
            .withAutomaticReconnect()
            .configureLogging(LogLevel.Information)
            .build();

        this.setState({ hubConnection }, () => {
            this.state.hubConnection.start()
                .catch(err => console.log(err));

            this.state.hubConnection.on('startTracking', (message, status, obj) => {
                this.setState({ buttonToggle: false });
                this.showMessage(message);
            });

            this.state.hubConnection.on('stopTracking', (receivedMessage, status) => {
                this.setState({ buttonToggle: true });
                this.showMessage(receivedMessage);
            });

            this.state.hubConnection.on('getActiveTracking', (istracking, worktask) => {
                this.setState({ buttonToggle: !istracking });
            });
        });

    }

    startTracking = (event) => {
        this.state.hubConnection
            .invoke('StartTracking', this.props.worktaskId)
            .catch(err => {
                console.error(err);
                this.setState({ buttonToggle: true });
            });
    };

    stopTracking = (event) => {
        this.state.hubConnection.invoke('StopTracking')
            .catch(err => {
                console.error(err);
                this.setState({ buttonToggle: false });
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