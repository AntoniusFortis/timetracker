import { hasAuthorized } from './components/Account'

function fetchServer(uri, callback, body, method, contentType) {
    const headers = new Headers();

    if (hasAuthorized()) {
        headers.append("Authorization", "Bearer " + localStorage.getItem('tokenKey'));
    }

    fetch(uri, {
        method: method,
        headers: headers,
        body: body
    })
        .then(result => callback(result));
}

export function Get(uri, callback) {
    fetchServer(uri, callback, null, 'GET', null);
}

export function Post(uri, body, callback, contentType = 'Json') {
    if (contentType === 'Json') {
        const json = JSON.stringify(body);
        fetchServer(uri, callback, json, 'POST', contentType);
        return;
    }

    fetchServer(uri, callback, body, 'POST', contentType);
}

export function Delete(uri, body, callback, contentType = 'Json') {
    if (contentType === 'Json') {
        const json = JSON.stringify(body);
        fetchServer(uri, callback, json, 'DELETE', contentType);
        return;
    }

    fetchServer(uri, callback, body, 'DELETE', contentType);
}