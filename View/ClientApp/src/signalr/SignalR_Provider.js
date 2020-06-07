import { HubConnectionBuilder, LogLevel, HttpTransportType } from '@microsoft/signalr';

export class SignalR_Provider {
    static url = '/trackingHub';
    static logLevel = LogLevel.Error;
    static transports = HttpTransportType.WebSockets | HttpTransportType.LongPolling;
    static serverTimeout = 120000;

    static connection = null;
    static callbacks = [];
    static trackingIsOn = false;

    static onSignalREvents = (istracking, worktask, started, message) => {
        this.trackingIsOn = istracking;
        
        for (var i = 0; i < this.callbacks.length; i++) {
            this.callbacks[i](istracking, worktask, started, message);
        }
    }

    static getConnection(token) {
        if (this.connection) {
            return this.connection;
        }

        const tokenGetter = () => token;

        const connection = new HubConnectionBuilder()
            .withUrl(this.url, { accessTokenFactory: tokenGetter, transport: this.transports })
            .configureLogging(this.logLevel)
            .build();

        connection.serverTimeoutInMilliseconds = this.serverTimeout;

        connection.onclose(this.onClose);

        connection.on('getActiveTracking', this.onSignalREvents);

        this.connection = connection;
        return connection;
    }

    static start = async () => {
        try {
            await this.connection.start();
        } catch (err) {
            console.log(err);
            setTimeout(() => this.start(), 5000);
        }
    }

    static onClose = async (error) => {
        await this.start();
    }
}