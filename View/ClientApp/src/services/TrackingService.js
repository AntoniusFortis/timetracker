import React, { useEffect, useState, useCallback } from 'react';
import { getToken } from '../components/Account'
import { SignalR_Provider } from '../signalr/SignalR_Provider';
const startTrackingIcon = require('../other/startTracking.png');
const stopTrackingIcon = require('../other/stopTracking.png');

export const TrackingService = (props) => {
    const [tracking, setTracking] = useState(true);

    const onActiveTrackingReceive = (istracking, worktask, started, message) => {
        setTracking(istracking);
        if (props.onActiveTrackingReceive && !istracking) {
            props.onActiveTrackingReceive();
        }
    }

    const unsubscribe = () => {
        SignalR_Provider.callbacks.pop();
    }

    useEffect(() => {
        SignalR_Provider.callbacks.push(onActiveTrackingReceive);
        setTimeout(() => {
            setTracking(SignalR_Provider.trackingIsOn);
        }, 1000);
        return () => unsubscribe();
    }, []);

    const startTracking = useCallback(() => {
        SignalR_Provider.getConnection(getToken())
            .invoke('StartTracking', props.worktaskId)
            .catch(err => {
                console.error(err);
                setTracking(false);
            });
    }, []);

    const stopTracking = (event) => {
        SignalR_Provider.getConnection(getToken())
            .invoke('StopTracking')
            .catch(err => {
                console.error(err);
                setTracking(true);
            });
    };

    return (
        <div style={{ display: 'inline-block', margin: '5px' }}>
            <button style={{ border: 'none', paddingLeft: '2px', display: (!tracking ? 'inline-block' : 'none') }} onClick={(e) => startTracking()}><img src={startTrackingIcon} style={{ marginBottom: '3px' }} width="28"></img><span>Начать отслеживание</span></button>
            <button style={{ border: 'none', paddingLeft: '2px', display: (tracking ? 'inline-block' : 'none') }} onClick={stopTracking}><img src={stopTrackingIcon} style={{ marginBottom: '3px' }} width="28"></img><span>Остановить отслеживание</span></button>
        </div>
    );
}