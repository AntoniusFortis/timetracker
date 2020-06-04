
export function setToken(token) {
    localStorage.setItem('tokenKey', token)
}

export function hasAuthorized() {
    const result = localStorage.getItem('tokenKey') !== null;
    return result;
}

export function getToken() {
    var result = localStorage.getItem('tokenKey');
    return result;
}