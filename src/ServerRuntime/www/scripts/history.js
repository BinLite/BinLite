import { getRealms, getUsers, getUser, getHistory } from './api.js';

let user;
let users;
let realms;

window.onload = async () => {
  user = await getUser();
  users = await getUsers();
  realms = await getRealms();


}

document.getElementById("")

async function load() {

}