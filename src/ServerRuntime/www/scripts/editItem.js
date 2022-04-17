import { getItems, createItem, getIds, getRealm, updateItem } from './api.js';
import {cleanxss} from './utils.js';

let items = [];
let parent = {};
let realm = {};
let mode = "create";

async function getParents(items, item) {
  if (item.parent === null) {
    return [];
  } else {
    let parent = items.filter(i => i.id == item.parent)[0];
    return [parent].concat(await getParents(items, parent));
  }
}

window.onload = async () => {
  const urlParams = new URLSearchParams(window.location.search);
  mode = urlParams.get("mode");

  document.getElementById("createBtn").innerHTML = mode == "create" ? "Create" : "Edit";
  document.getElementById("title").innerHTML = mode == "create" ? "Create Item" : "Edit Item";

  realm = await getRealm(urlParams.get('realm'));
  items = await getItems(realm.id);

  parent = urlParams.get('parent') == "null" ? null : items.find(i => i.id == urlParams.get('parent'));

  let parentP = document.getElementById("parentP")
  if (parent != null) {
    let parents = await getParents(items, parent);
    parentP.innerHTML = cleanxss(realm.name + " - " + parents.map(p => p.name).join("/") + (parents.length > 0 ? "/" : "") + parent.name);
  } else {
    parentP.innerHTML = cleanxss(realm.name);
  }

  if (mode == "create") {
    document.getElementById("idBx").value = (await getIds(1))[0];
  } else if (mode == "edit") {
    let toEdit = items.find(i => i.id == urlParams.get("item"));

    document.getElementById("idBx").value = toEdit.id;
    document.getElementById("nameBx").value = toEdit.name;
    document.getElementById("descriptionBx").value = toEdit.description;
    document.getElementById("idBx").readOnly = true;
  }

  let nameBx = document.getElementById("nameBx");
  nameBx.focus();
  nameBx.select();
};

document.getElementById("createBtn").onclick = async () => {
  let item = {
    id: document.getElementById("idBx").value,
    name: document.getElementById("nameBx").value,
    realm: realm.id,
    parent: parent == null ? null : parent.id,
    description: document.getElementById("descriptionBx").value
  };

  if (mode == "create") {
    await createItem(item);
  } else if (mode == "edit") {
    await updateItem(item);
  }
  location.href = "/";
};

document.getElementById("cancelBtn").onclick = async () => {
  location.href = "/";
};