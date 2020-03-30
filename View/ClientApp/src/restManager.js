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

    const json = JSON.stringify(body);

    fetch(uri, {
        method: "POST",
        headers: headers,
        body: json
    })
        .then(result => callback(result));
}

export function Delete(uri, body, callback) {
    const headers = new Headers();

    if (hasAuthorized()) {
        headers.append("Authorization", "Bearer " + localStorage.getItem('tokenKey'));
    }

    headers.append("Content-Type", "application/json;charset=utf-8");

    const json = JSON.stringify(body);

    fetch(uri, {
        method: "DELETE",
        headers: headers,
        body: json
    })
        .then(result => callback(result));
}