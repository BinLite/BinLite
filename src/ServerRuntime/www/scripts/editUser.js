import { changePassword, getUsers, getUser, changeEmail } from './api.js';

let users = [];
let editingUser;

window.onload = async () => {
  const urlParams = new URLSearchParams(window.location.search);

  users = await getUsers();
  let currentUser = await getUser();
  if (urlParams.get('user') == undefined || users.find(u => u.id == urlParams.get('user')) == undefined) {
    editingUser = currentUser;
  } else {
    if (!currentUser.serverAdmin) { location.href = "/"; }
    editingUser = users.find(u => u.id == urlParams.get('user'));
  }

  document.getElementById("idP").innerHTML = cleanxss(editingUser.id);
  document.getElementById("usernameP").innerHTML = cleanxss(editingUser.username);
  document.getElementById("emailP").innerHTML = cleanxss(editingUser.email);
};

document.getElementById("changeEmail").onclick = async () => {
  let newEmail = prompt("Enter new user email: ", editingUser.email);
  if (newEmail == null) { return; }
  let r = await changeEmail(editingUser.id, newEmail);
  if (r) {
    alert("Email changed.");
    location.reload();
  } else {
    alert("Failed to change email.");
    location.reload();
  }
};
document.getElementById("changePassword").onclick = async () => {
  let oldPW = document.getElementById("oldpw").value.trim();
  let newPW = document.getElementById("newpw").value.trim();

  if (newPW.length < 8) {
    alert("Password must be at least 8 characters long.");
    return;
  }

  let r = await changePassword(oldPW, newPW);
  console.log(r);
  if (r) {
    alert("Password changed.");
    location.reload();
  } else {
    alert("Failed to change password.");
    location.reload();
  }
};