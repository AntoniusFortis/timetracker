import { hasAuthorized } from './components/Account'

function fetchServer(uri, callback, body, method) {
    const headers = new Headers();

    if (hasAuthorized()) {
        headers.append("Authorization", "Bearer " + localStorage.getItem('tokenKey'));
    }

    if (body) {
        headers.append("Content-Type", "application/json;charset=utf-8");
    }

    fetch(uri, {
        method: method,
        headers: headers,
        body: body
    })
        .then(result => callback(result));
}

export function Get(uri, callback) {
    fetchServer(uri, callback, null, 'GET');
}

export function Post(uri, body, callback) {
    const json = JSON.stringify(body);

    fetchServer(uri, callback, json, 'POST');
}

export function Delete(uri, body, callback) {
    const json = JSON.stringify(body);

    fetchServer(uri, callback, json, 'DELETE');
}