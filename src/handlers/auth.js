const accounts = [
    {
        username: "nheber",
        password: "somePass"
    }
];

let sessions = [];

module.exports = {
    createSession: (ws, msg, send) => {
        if (!msg.data.username || !msg.data.password) {
            send("error", "No username or password provided.");
            return;
        }
    
        let acc = accounts.filter(a => a.username === msg.data.username && a.password === msg.data.password);
        if (acc.length <= 0) {
            send("error", "Invalid username or passwrd.");
            return;
        }
    
        acc = acc[0];
        ws.clientData.auth = {
            username: acc.username,
            token: require("crypto").randomBytes(64).toString('hex'),
        };
    
        sessions.push({
            username: acc.username,
            token: ws.clientData.auth.token,
            createdTime: null, // Session creation time
        });
        
        send("response", { username: msg.data.username, token: ws.clientData.auth.token });
    },
    getSession = (token) => {
        // Add time check to prevent old tokens being used
        //sessions = sessions.filter(s => s.timestamp ... ?);

        let matches = sessions.filter(s => s.token === token);
        if (matches.length > 0) { return matches[0]; }
        return null;
    }
};