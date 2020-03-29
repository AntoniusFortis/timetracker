
export function hasAuthorized() {
    var result = localStorage.getItem('tokenKey') !== null;
    return result;
}