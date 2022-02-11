let listItems = (req, res) => {
    res.send("Item list!");
}

let createItem = (req, res) => {
    res.send("A");
}

let deleteItem = (req, res) => {

}

let updateItem = (req, res) => {

}

module.exports = (app) => {
    app.get('/api/item', listItems);
    app.post('/api/item', createItem);
    app.delete('/api/item', deleteItem);
    app.patch('/api/item', updateItem);
}