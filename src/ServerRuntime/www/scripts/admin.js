import { createUser, getUsers, getIds, createRealm, getRealms, getUser, resetPassword } from './api.js';

let users = [];

window.onload = async () => {
  let user = await getUser();
  if (!user.serverAdmin) {
    location.href = "/";
  }

  document.getElementById("realmIdBx").value = (await getIds(1))[0];

  let selector = document.getElementById("ownerSelector");
  users = await getUsers();
  for (var u of users) {
    var opt = document.createElement('option');
    opt.value = u.id;
    opt.innerHTML = u.username;
    selector.appendChild(opt);
  }

  await loadTables();
};

document.getElementById("createRealmBtn").onclick = async () => {
  let realm = {
    id: document.getElementById("realmIdBx").value,
    name: document.getElementById("realmNameBx").value,
    owner: document.getElementById("ownerSelector").value,
  };

  if (!confirm("Create realm?\n" + JSON.stringify(realm))){
    return;
  }

  let r = await createRealm(realm);
  if (r == null) {
    alert("Failed to create realm.");
  } else {
    alert("Realm created successfully.\n" + JSON.stringify(realm));
  }
}

document.getElementById("createUserBtn").onclick = async () => {
  let user = {
    username: document.getElementById("usernameBx").value,
    email: document.getElementById("emailBx").value,
  };

  if (!confirm("Create user?\n" + JSON.stringify(user))){
    return;
  }

  let r = await createUser(user.username, user.email);
  if (r == null) {
    alert("Failed to create user.");
  } else {
    alert("User created successfully.\n" + JSON.stringify(user));
  }
}

async function loadTables() {
  await loadRealmTable();
  await loadUserTable();
}

async function loadRealmTable() {
  let realms = await getRealms();

  let table = document.getElementById("realmTable");
  for (let r of realms) {
    let tr = document.createElement("tr");

    let id = document.createElement("td");
    id.innerHTML = r.realm.id;
    tr.appendChild(id);

    let name = document.createElement("td");
    name.innerHTML = r.realm.name;
    tr.appendChild(name);

    let owner = document.createElement("td");
    let ownerUser = users.filter(u => u.id == r.realm.owner)[0];
    owner.innerHTML = `${ownerUser.username} (${ownerUser.id})`;
    tr.appendChild(owner);

    let actions = document.createElement("td");


    let editBtn = document.createElement("button");
    editBtn.classList.add("miniBtn");
    editBtn.classList.add("btn-edt");
    editBtn.title = "Edit Realm";
    editBtn.innerHTML = "&nbsp;&nbsp;&nbsp;&nbsp;";
    editBtn.onclick = () => {
      location.href = `/editRealm.html?realm=${encodeURIComponent(r.realm.id)}`;
    };
    actions.appendChild(editBtn);

    let deleteBtn = document.createElement("button");
    deleteBtn.classList.add("miniBtn");
    deleteBtn.classList.add("btn-del");
    deleteBtn.title = "Delete Realm";
    deleteBtn.innerHTML = "&nbsp;&nbsp;&nbsp;&nbsp;";
    deleteBtn.onclick = () => {
      alert("To be implemented.");
    };
    actions.appendChild(deleteBtn);


    tr.appendChild(actions);

    table.appendChild(tr);
  }
}

async function loadUserTable() {
  let table = document.getElementById("userTable");
  for (let u of users) {
    let tr = document.createElement("tr");

    let id = document.createElement("td");
    id.innerHTML = u.id;
    tr.appendChild(id);

    let username = document.createElement("td");
    username.innerHTML = u.username;
    tr.appendChild(username);

    let email = document.createElement("td");
    email.innerHTML = u.email;
    tr.appendChild(email);

    let actions = document.createElement("td");

    let editBtn = document.createElement("button");
    editBtn.classList.add("miniBtn");
    editBtn.classList.add("btn-res");
    editBtn.title = "Reset Password";
    editBtn.innerHTML = "&nbsp;&nbsp;&nbsp;&nbsp;";
    editBtn.onclick = async () => {
      let con = confirm(`Reset password for ${u.username}?`);
      if (!con) { return; }
      let r = await resetPassword(u.id);
      if (r) {
        alert("Password reset. They've been sent an email.");
      } else {
        alert("Failed to reset password.");
      }
    };
    actions.appendChild(editBtn);

    let deleteBtn = document.createElement("button");
    deleteBtn.classList.add("miniBtn");
    deleteBtn.classList.add("btn-edt");
    deleteBtn.title = "Edit User";
    deleteBtn.innerHTML = "&nbsp;&nbsp;&nbsp;&nbsp;";
    deleteBtn.onclick = () => {
      location.href = "/users.html?user=" + u.id;
    };
    actions.appendChild(deleteBtn);


    tr.appendChild(actions);

    table.appendChild(tr);
  }
}