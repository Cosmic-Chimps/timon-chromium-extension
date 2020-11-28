import jwt_decode from 'jwt-decode';

const decode = function (text) {
    return jwt_decode(text);
};

export { decode };
