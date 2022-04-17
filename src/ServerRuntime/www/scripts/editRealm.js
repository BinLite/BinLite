import { getRealms, getUsers, updateRealm, getPermissions, setPermission } from './api.js';

let users = [];
let editingRealm = {};
let owner = {};

window.onload = async () => {
  const urlParams = new URLSearchParams(window.location.search);
  editingRealm = (await getRealms()).find(r => r.realm.id == urlParams.get('realm'));
  if (editingRealm.permission < 3) { alert('You are not permitted to edit this realm.'); return; }

  let selector = document.getElementById("ownerSelector");
  users = await getUsers();
  for (var u of users) {
    var opt = document.createElement('option');
    opt.value = u.id;
    opt.innerHTML = cleanxss(u.username);
    selector.appendChild(opt);
  }
  owner = users.find(u => u.id == editingRealm.realm.owner);
  selector.value = owner.id;

  document.getElementById("nameBx").value = editingRealm.realm.name;
  document.getElementById("idText").innerText = cleanxss(editingRealm.realm.id);
  loadUserTable();
};

document.getElementById("cancelBtn").onclick = () => history.back();
document.getElementById("updateBtn").onclick = async () => {
  let realm = {
    id: editingRealm.realm.id,
    name: document.getElementById("nameBx").value,
    owner: document.getElementById("ownerSelector").value,
  };

  let newOwner = users.find(u => u.id == realm.owner);
  let con = confirm(`Confirm update for realm ${editingRealm.realm.name}?\n\nName: ${realm.name}\n\nOwner: ${newOwner.username}`);
  if (!con) { return; }

  await updateRealm(realm);
  location.reload();
};

async function loadUserTable() {
  let table = document.getElementById("userTable");
  let perms = await getPermissions(editingRealm.realm.id);
  for (let u of users) {
    let perm = perms.find(p => p.id == u.id).permission;

    let tr = document.createElement("tr");

    let username = document.createElement("td");
    username.innerHTML = cleanxss(u.username);
    tr.appendChild(username);

    let permission = document.createElement("td");

    if (u.id == editingRealm.realm.owner) {
      permission.innerHTML = "Realm Owner - All Permissions";
      tr.appendChild(permission);
      table.appendChild(tr);
      continue;
    } else if (u.serverAdmin) {
      permission.innerHTML = "Server Admin - All Permissions";
      tr.appendChild(permission);
      table.appendChild(tr);
      continue;
    }

    let dropdown = document.createElement("select");

    let opt0 = document.createElement("option");
    opt0.value = 0;
    opt0.innerHTML = "None";
    dropdown.appendChild(opt0);

    let opt1 = document.createElement("option");
    opt1.value = 1;
    opt1.innerHTML = "Read Only";
    dropdown.appendChild(opt1);

    let opt2 = document.createElement("option");
    opt2.value = 2;
    opt2.innerHTML = "Read & Write";
    dropdown.appendChild(opt2);

    let opt3 = document.createElement("option");
    opt3.value = 3;
    opt3.innerHTML = "Admin";
    dropdown.appendChild(opt3);

    dropdown.value = perm;
    dropdown.onchange = async () => {
      await setPermission(editingRealm.realm.id, u.id, dropdown.value);
      alert("Permission set.")
    };

    permission.appendChild(dropdown);
    tr.appendChild(permission);

    table.appendChild(tr);
  }
}