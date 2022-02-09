const { WebSocketServer } = require(`ws`),
    { readFileSync } = require(`fs`),
    { createServer } = require(`http`);

const http = createServer({ 
// certs here 
});
const wss = new WebSocketServer({ server: http });

wss.on(`connection`, (ws) => {
    ws.on(`message`, (data) => {
        let msg;

        try {
            msg = JSON.parse(data);
        } catch {
            ws.send(JSON.stringify({ type: "error", data: "Message format must be `{ type: \"...\", data: \"...\" }`" }));
            return;
        }

        let send = (type, data) => {
            let obj = { type, data };
            if (msg.responseId) {
                obj.responseId = msg.responseId;
            }
            ws.send(JSON.stringify(obj));
        };

        if (!msg.type || !msg.data) {
            send("error", "Message format must be `{ type: \"...\", data: \"...\" }`" );
            return;
        }

        if (!msg.token) {
            if (msg.type === "auth") {
                require("./handlers/auth").createSession(ws, msg, send);
            } else {
                send("error", "Invalid auth token.");
                return;
            }
        }

        let session = require("./handlers/auth").getSession(msg.token);
        if (!session) {
            send("error", "Invalid auth token.");
            return;
        }

        try {
            require("./handlers/" + msg.type)(ws, msg, send);
        } catch {
            send("error", "Invalid command type.")
        }
    });
});

http.listen(8080);