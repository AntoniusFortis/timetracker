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
            .withUrl("/trackingHub", { accessTokenFactory: () => token })
            .withAutomaticReconnect()
            .configureLogging(LogLevel.Information)
            .build();

        this.setState({ hubConnection }, () => {
            this.state.hubConnection.start()
                .catch(err => console.log(err));

            this.state.hubConnection.on('startTracking', (message, status) => {
                this.setState({ buttonToggle: false });
                this.showMessage(message);
            });

            this.state.hubConnection.on('stopTracking', (receivedMessage, status) => {
                this.setState({ buttonToggle: true });
                this.showMessage(receivedMessage);
            });

            this.state.hubConnection.on('getActiveTracking', (istracking) => {
                this.setState({ buttonToggle: !istracking });
            });

            //this.getActiveTracking();
        });

    }

    invokeFunction = (name) => {
        this.state.hubConnection
            .invoke(name)
            .catch(err => {
                console.error(err);
                this.setState({ buttonToggle: true });
            });
    }

    invokeFunctionArg = (name, arg) => {
        this.state.hubConnection
            .invoke(name, arg)
            .catch(err => {
                console.error(err);
                this.setState({ buttonToggle: true });
            });
    }

    //getActiveTracking = () => {
    //    this.invokeFunction('GetActiveTracking');
    //}

    startTracking = (event) => {
        this.state.hubConnection
            .invoke('StartTracking', this.props.worktaskId)
            .catch(err => {
                console.error(err);
                this.setState({ buttonToggle: true });
            });
    };

    stopTracking = () => {
        this.invokeFunction('StopTracking');
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