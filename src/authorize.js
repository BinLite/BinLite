let auth = require('express-basic-auth');

module.exports = (username, password) => {
    return auth.safeCompare(username, "") && auth.safeCompare(password);
    
};