<<<<<<< Updated upstream
<<<<<<< Updated upstream
const express = require('express'),
    basicAuth = require('express-basic-auth');

let app = express();

app.use(basicAuth({
    authorizer: require('./authorize'),
    challenge: true,
    realm: "BinLite-1",
}));

var normalizedPath = require("path").join(__dirname, "routes");
require("fs").readdirSync(normalizedPath).forEach(function(file) {
    try {
        require("./routes/" + file)(app);
    } catch {
        console.log("Couldn't require `" + file + "`.");
    }
});

var server = app.listen(8080, function () {
    var host = server.address().address
    var port = server.address().port

    console.log("Example app listening at http://%s:%s", host, port)
});
=======
const express = require("express"),
  config = require("./config"),
  app = express(),
  router = express.Router();

router.use((req, res, next) => {
  if (req.)
});
=======
const express = require("express"),
  config = require("./config"),
  app = express(),
  router = express.Router();

router.use((req, res, next) => {
  if (req.)
});
>>>>>>> Stashed changes

app.use("/api", r);

app.listen(config.port, () => console.log("Listening on " + config.port));

<<<<<<< Updated upstream
console.log("Hello, world!");
>>>>>>> Stashed changes
=======
console.log("Hello, world!");
>>>>>>> Stashed changes
