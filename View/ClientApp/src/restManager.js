import { hasAuthorized, getToken, setToken } from './components/Account'

export class HttpRequestHelper {
    static Fetch(uri, callback, body, method) {
        const headers = new Headers();
        const token = getToken();
        if (hasAuthorized()) {
            headers.append('Authorization', 'Bearer ' + token);
        }

        headers.append('Content-Type', 'application/json')

        const request = async () => {
            const response = await fetch(uri, {
                method: method,
                headers: headers,
                body: body
            });

            if (response.status === 401 && hasAuthorized()) {
                await requestNewToken();
                await request();
                return;
            }

            if (callback) {
                callback(response);
            }
        }

        const requestNewToken = async () => {
            const tokenBody = { AccessToken: token, RefreshToken: localStorage.getItem('refresh_token') };
            const json = JSON.stringify(tokenBody);

            const tokenRequestHeaders = new Headers();
            tokenRequestHeaders.append('Content-Type', 'application/json')

            const tokenRequest = await fetch('/api/account/token', {
                method: 'POST',
                headers: tokenRequestHeaders,
                body: json
            });

            const result = await tokenRequest.json();
            setToken(result.access_token);
            localStorage.setItem('refresh_token', result.refresh_token);

            headers.set('Authorization', 'Bearer ' + result.access_token)
        }

        request();
    }
}

export function Get(uri, callback) {
    HttpRequestHelper.Fetch(uri, callback, null, 'GET');
}

export function Post(uri, body, callback, contentType = 'Json') {
    if (contentType === 'Json') {
        const json = JSON.stringify(body);
        HttpRequestHelper.Fetch(uri, callback, json, 'POST');
        return;
    }

    HttpRequestHelper.Fetch(uri, callback, body, 'POST');
}

export function Delete(uri, body, callback, contentType = 'Json') {
    if (contentType === 'Json') {
        const json = JSON.stringify(body);
        HttpRequestHelper.Fetch(uri, callback, json, 'DELETE');
        return;
    }

    HttpRequestHelper.Fetch(uri, callback, body, 'DELETE');
}