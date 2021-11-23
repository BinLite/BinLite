console.log("Hello world!");

const Server = require("./server");
const readline = require("readline");

const serv = new Server(process.env.PORT || 3000);

console.log("Running");

const rl = readline.createInterface({input: process.stdin, output: process.stdout});

let running = true;
console.log("");
rl.setPrompt("Input command (exit): ");
rl.prompt();

rl.on('line', line => {
  console.log("");

  line = line.trim().toLowerCase();

  switch (line) {
    case "exit":
      console.log("Exiting...");
      serv.exit();
      console.log("Goodbye!");
      process.exit();
    default:
      console.log("Unknown command.");
      break;
  }
  rl.prompt();
});