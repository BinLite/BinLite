process.on("uncaughtException", err => {
    console.log("Error: ");
    console.log(err);
});

require("./server");