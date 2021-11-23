class Server {


  constructor(port) {

    var express = require('express'),
      app = express();
    this.app = app;
    
    console.log("Registering routes...");
    var routes = require("./api/routes");
    routes(app);

    console.log("Starting listening...");
    this.server = app.listen(port);
    console.log('Running server on port: ' + port);
  }

  exit() {
    this.server.close(err => console.log("Error: " + err));
    console.log("Closed express server.");
  }
}

module.exports = Server;