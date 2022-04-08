import { getRealms, getUsers, getUser, getHistory, getItems, getHistorySize } from './api.js';

let user;
let users;
let realms;
let allItems = [];

let realm;

let page = 1;

let typeMap = {
  0: "RealmDisabled",
  1: "RealmDisabled",
  2: "RealmNameChange",
  3: "RealmOwnerChange",
  4: "RealmPermission",

  5: "ItemCreated",
  6: "ItemDeleted",
  7: "ItemUpdate",

  8: "TagCreated",
  9: "TagDeleted",
  10: "TagItemChange",
}

window.onload = async () => {
  const urlParams = new URLSearchParams(window.location.search);
  realm = urlParams.get("realm");

  user = await getUser();
  users = await getUsers();
  realms = await getRealms();

  for (let realm of realms) {
    allItems = allItems.concat(await getItems(realm.realm.id));
  }

  console.log(allItems);
  let size = await getHistorySize(page, realm, null);
  document.getElementById("pageNum").innerHTML = `${page}/${size}`;
  load();
}

document.getElementById("leftArrow").onclick = async () => {
  if (document.getElementById("leftArrow").classList.contains("disabled")) { return; }
  page--;
  page = page < 1 ? 1 : page;
  load();
  let size = await getHistorySize(page, realm, null);
  document.getElementById("pageNum").innerHTML = `${page}/${size}`;
}

document.getElementById("rightArrow").onclick = async () => {
  if (document.getElementById("rightArrow").classList.contains("disabled")) { return; }
  page++;
  load();
  let size = await getHistorySize(page, realm, null);
  document.getElementById("pageNum").innerHTML = `${page}/${size}`;
}

function capitalizeFirstLetter(string) {
  return string.charAt(0).toUpperCase() + string.slice(1);
}

async function load() {
  let size = await getHistorySize(page, realm, null);
  document.getElementById("leftArrow").classList.remove("enabled");
  document.getElementById("rightArrow").classList.remove("enabled");
  document.getElementById("leftArrow").classList.remove("disabled");
  document.getElementById("rightArrow").classList.remove("disabled");

  if (page > 1) {
    document.getElementById("leftArrow").classList.add("enabled");
  } else {
    document.getElementById("leftArrow").classList.add("disabled");
  }
  if (page < size - 1) {
    document.getElementById("rightArrow").classList.add("enabled");
  } else {
    document.getElementById("rightArrow").classList.add("disabled");
  }

  let history = await getHistory(page, realm, null);
  let parent = document.getElementById("ledgerParent");
  parent.innerHTML = "";

  let index = 0;
  for (let h of history) {
    index++;
    let row = document.createElement("div");
    row.classList.add("ledgerRow");
    if (index % 2 == 0) {
      row.classList.add("other");
    }

    let u = users.find(u => u.id == h.source);
    let time = new Date(h.timestamp);
    time = time.toLocaleString();

    let content = `<div class="color">${time} - ${typeMap[h.type]} - ${u.username}</div>`;

    if (h.type == 7) {
      let ent = allItems.find(i => i.id == h.entity);
      content += `<br>${h.entity}${(ent == undefined ? "" : ` <div class="color">-</div> ` + ent.name)}<br>${
        capitalizeFirstLetter(h.field)} <div class="color">-</div> `;
      if (h.field == "parent") {
        let o = allItems.find(i => i.id == h.from);
        let n = allItems.find(i => i.id == h.to);

        o = o == undefined ? "^" : `${o.name} (${o.id})`;
        n = n == undefined ? "^" : `${n.name} (${n.id})`;

        content += `${o} <div class="color">-></div> ${n}`;
      } else {
        content += `(${h.from}) <div class="color">-></div> (${h.to})`;
      }
    } else if (h.type == 5) {
      let ent = JSON.parse(h.to);


      let p = allItems.find(i => i.id == ent.parent);
      p = p == undefined ? "^" : `${p.name} (${p.id})`;

      let realm = realms.find(r => r.realm.id == ent.realm).realm;
      console.log(ent.realm);
      console.log(realms);
      realm = `${realm.name} (${realm.id})`;

      content += `<br>${h.entity}${(ent == undefined ? "" : ` <div class="color">-</div> ` + ent.name)}<br>
      Parent: ${p}<br>
      Description: ${ent.description}<br>
      Realm: ${realm}`;
    } else if (h.type == 6) {
      let ent = JSON.parse(h.from);


      let p = allItems.find(i => i.id == ent.parent);
      p = p == undefined ? "^" : `${p.name} (${p.id})`;

      let realm = realms.find(r => r.realm.id == ent.realm).realm;
      console.log(ent.realm);
      console.log(realms);
      realm = `${realm.name} (${realm.id})`;

      content += `<br>${h.entity}${(ent == undefined ? "" : ` <div class="color">-</div> ` + ent.name)}<br>
      Parent: ${p}<br>
      Description: ${ent.description}<br>
      Realm: ${realm}`;
    }


    row.innerHTML = content;
    parent.appendChild(row);
  }
}