import { hasAuthorized } from './components/Account'

export function Get(uri, callback) {
    const headers = new Headers();

    if (hasAuthorized()) {
        headers.append("Authorization", "Bearer " + localStorage.getItem('tokenKey'));
    }

    fetch(uri, {
        method: "GET",
        headers: headers
    })
        .then(result => callback(result));
}

export function Post(uri, body, callback) {
    const headers = new Headers();

    if (hasAuthorized()) {
        headers.append("Authorization", "Bearer " + localStorage.getItem('tokenKey'));
    }

    headers.append("Content-Type", "application/json;charset=utf-8");

    fetch(uri, {
        method: "POST",
        headers: headers,
        body: JSON.stringify(body)
    })
        .then(result => callback(result));
}