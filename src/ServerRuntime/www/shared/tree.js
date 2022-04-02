import { getItems, getRealms, deleteItem } from '/shared/api.js';

let allRealms = [];
let currentRealm = 0;
let currentRealmItems = [];

function getHiararchy(items) {
  if (items == null || items == undefined || items.length == 0) { return []; }
  let h = {};

  function add(parent) {
    parent.children = [];
    for (let item of items) {
      if (item.parent == parent?.id) {
        parent.children.push(item);
        add(item);
      }
    }
  }

  add(h);
  return h.children;
}

window.onload = async () => {
  allRealms = await getRealms();
  var selector = document.getElementById("realmSelector");
  for (var r of allRealms) {
    var opt = document.createElement('option');
    opt.value = r.realm.id;
    opt.innerHTML = r.realm.name;
    selector.appendChild(opt);
  }
  currentRealm = document.getElementById("realmSelector").value;

  document.getElementById("realmSelector").onchange = realmSelectorChange;
  realmSelectorChange();
  document.getElementById("searchBox").oninput = searchChanged;
  document.getElementById("searchBox").focus();
  document.getElementById("addToRealm").onclick = () => addClicked(null);
};

async function realmSelectorChange() {
  var currentRealm = document.getElementById("realmSelector").value;
  currentRealmItems = await getItems(currentRealm);
  await setupHiararchy(currentRealmItems, false);
}

async function setupHiararchy(items, expanded) {
  let hair = getHiararchy(items);
  document.getElementById("hiararchyParent").innerHTML = "";

  for (let item of hair) {
    hiararchyItem(item, document.getElementById("hiararchyParent"), expanded);
  }

  var toggler = document.getElementsByClassName("caret");
  var i;

  for (i = 0; i < toggler.length; i++) {
    toggler[i].addEventListener("click", function() {
      this.parentElement.parentElement.parentElement.querySelector(".nested").classList.toggle("active");
      this.classList.toggle("caret-down");
    });
  }
}

function hiararchyItem(item, htmlParent, expanded) {
  let li = document.createElement("li");

  let outerRow = document.createElement("div");
  outerRow.classList.add("flex");
  outerRow.classList.add("hover-tree-color");
  outerRow.classList.add("space-between");

  let rowLeft = document.createElement("div");
  outerRow.appendChild(rowLeft)
  let rowRight = document.createElement("div");
  outerRow.appendChild(rowRight)

  let span = document.createElement("span");
  span.classList.add(item.children.length > 0 ? "caret" : "emptyCaret");
  if (item.match) {
    span.classList.add("match");
  }
  span.innerHTML += item.name;
  rowLeft.appendChild(span);

  addButtons(rowRight, item);
  li.appendChild(outerRow);

  if (item.children.length > 0) {

    let ul = document.createElement("ul");
    ul.classList.add("nested");
    if (expanded) { ul.classList.add("active"); }
    for (let sub of item.children) {
      hiararchyItem(sub, ul, expanded);
    }
    li.appendChild(ul);
  }
  htmlParent.append(li);
}

function addButtons(li, item) {
  let addBtn = document.createElement("button");
  addBtn.innerHTML = "&nbsp;&nbsp;&nbsp;&nbsp;";
  addBtn.onclick = () => { addClicked(item); };
  addBtn.classList.add("miniBtn");
  addBtn.classList.add("addBtn");
/*   addBtn.style.cssFloat = 'right'; */
  li.appendChild(addBtn);

  let realm = allRealms.find(r => r.realm.id == currentRealm);

  if (realm.permission > 1) {
    let editBtn = document.createElement("button");
    editBtn.innerHTML = "&nbsp;&nbsp;&nbsp;&nbsp;";
    editBtn.onclick = () => { editClicked(item); };
    editBtn.classList.add("miniBtn");
    editBtn.classList.add("editBtn");
/*     editBtn.style.cssFloat = 'right'; */
    li.appendChild(editBtn);

    let subBtn = document.createElement("button");
    subBtn.innerHTML = "&nbsp;&nbsp;&nbsp;&nbsp;";
    subBtn.onclick = () => { subClicked(item); };
    subBtn.classList.add("miniBtn");
    subBtn.classList.add("subBtn");
/*     subBtn.style.cssFloat = 'right'; */
    li.appendChild(subBtn);
  }
}

function addClicked(item) {
  if (currentRealm == 0) { alert("No valid realm selected."); return; }
  item = item == null || item == undefined ? null : item.id;
  location.href = '/editItem.html?parent=' + item + "&mode=create&realm=" + currentRealm;
}

async function subClicked(item) {
  //let count = getChildren(currentRealmItems, item).length;

  let text = `Are you sure you want to remove '${item.name}'? (${item.id})`;
  if (confirm(text)) {
    await deleteItem(item.id);
    location.href = "/";
  }
}

async function editClicked(item) {
  location.href = '/editItem.html?parent=' + item.parent + "&mode=edit&realm=" + currentRealm + "&item=" + item.id;
}

async function searchChanged() {
  currentRealmItems.forEach(i => i.match = false);
  let term = document.getElementById("searchBox").value.trim();

  let items = currentRealmItems;
  if (term.length === 0) {
    setupHiararchy(items, false);
    return;
  }

  let results = items.filter(i => i.id == term || i.name.toLowerCase().includes(term.toLowerCase()));
  let results2 = []
  for (var r of results) {
    r.match = true;
    results2.push(r);
    let parents = await getParents(items, r);
    let children = getChildren(items, r);
    for (let p of parents.concat(children)) {
      if (results2.filter(j => j.id === p.id).length === 0) {
        results2.push(p);
      }
    }
  }

  results2 = results2.filter((value, index, self) =>
  index === self.findIndex((t) => (
    t.id === value.id
  )));

  setupHiararchy(results2, true);
}

async function getParents(items, item) {
  if (item.parent === null) {
    return [];
  } else {
    let parent = items.filter(i => i.id == item.parent)[0];
    return [parent].concat(await getParents(items, parent));
  }
}

function getChildren(items, item) {
  let children = items.filter(i => i.parent == item.id);
  if (children.length == 0) { return []; }
  let allChildren = children.concat(flatten(children.map(c => getChildren(items, c))));
  return allChildren;
}

const flatten = function(arr, result = []) {
  for (let i = 0, length = arr.length; i < length; i++) {
    const value = arr[i];
    if (Array.isArray(value)) {
      flatten(value, result);
    } else {
      result.push(value);
    }
  }
  return result;
};

document.getElementById("editRealm").onclick = () => {
  location.href = `/editRealm.html?realm=${currentRealm}`;
};

document.getElementById("historyBtn").onclick = () => {
  location.href = `/history.html?realm=${currentRealm}`;
};