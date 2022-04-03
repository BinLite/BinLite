import { getItems, getRealms, deleteItem } from './api.js';

let allRealms = [];
let currentRealm = 0;
let currentRealmItems = [];

window.onload = async () => {
  // Get realms from API
  allRealms = await getRealms();

  if (allRealms.length <= 0) {
    document.getElementById("content").innerHTML = "You do not have access to any realms. Please contact a server admin.";
    return;
  }

  // Load realms into selector
  var selector = document.getElementById("realmSelector");
  for (var r of allRealms) {
    var opt = document.createElement('option');
    opt.value = r.realm.id;
    opt.innerHTML = r.realm.name;
    selector.appendChild(opt);
  }
  selector.value = allRealms[0].realm.id;
  currentRealm = allRealms[0].realm.id;

  //Event subscriptions
  document.getElementById("realmSelector").onchange = realmSelectorChange;
  document.getElementById("searchBox").oninput = searchBxChanged;
  document.getElementById("searchBox").focus();
  document.getElementById("addToRealm").onclick = () => addClicked(null);
  realmSelectorChange();
};

function addClicked(item) {
  if (currentRealm == 0) { alert("No valid realm selected."); return; }
  item = item == null || item == undefined ? null : item.id;
  location.href = '/editItem.html?parent=' + item + "&mode=create&realm=" + currentRealm;
}

async function realmSelectorChange() {
  // Load the items of the selected realm
  var currentRealm = document.getElementById("realmSelector").value;
  let realm = allRealms.find(r => r.realm.id == currentRealm);
  let editSpan = document.getElementById("editRealmSpan");
  if (realm.permission < 3) { editSpan.innerHTML = ""; } else {
    editSpan.innerHTML = `<button id="editRealm" class="miniBtn btn-edt"></button>`;
    document.getElementById("editRealm").onclick = editRealmClick;
  }

  currentRealmItems = await getItems(currentRealm);
  await setupHierarchy(currentRealmItems, false);
}

let lastHierarchyItems;

// Taking a hierarchy array, load the tree view HTML
async function setupHierarchy(items, expanded) {
  // Convert a list of items into a hierarchy
  function convertListToHierarchy(items) {
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
    console.log(h.children);
    return h.children;
  }

  // Create the HTML for a single item, recursively
  function hierarchyItem(item, htmlParent, expanded) {
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
    if (expanded ) {span.classList.add("caret-down");}
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
        hierarchyItem(sub, ul, expanded);
      }
      li.appendChild(ul);
    }
    htmlParent.append(li);
  }

  lastHierarchyItems = items;
  let hair = convertListToHierarchy(items);
  document.getElementById("hierarchyParent").innerHTML = "";

  for (let item of hair) {
    hierarchyItem(item, document.getElementById("hierarchyParent"), expanded);
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

function addButtons(li, item) {
  let addBtn = document.createElement("button");
  addBtn.onclick = () => { addClicked(item) };
  addBtn.classList.add("miniBtn");
  addBtn.classList.add("btn-add");
/*   addBtn.style.cssFloat = 'right'; */
  li.appendChild(addBtn);

  let realm = allRealms.find(r => r.realm.id == currentRealm);

  if (realm.permission > 1) {
    let editBtn = document.createElement("button");
    editBtn.onclick = async () => {
      location.href = '/editItem.html?parent=' + item.parent + "&mode=edit&realm=" + currentRealm + "&item=" + item.id;
    };
    editBtn.classList.add("miniBtn");
    editBtn.classList.add("btn-edt");
/*     editBtn.style.cssFloat = 'right'; */
    li.appendChild(editBtn);

    let subBtn = document.createElement("button");
    subBtn.onclick = async () => {
      let text = `Are you sure you want to remove '${item.name}'? (${item.id})`;
      if (confirm(text)) {
        await deleteItem(item.id);
        location.href = "/";
      }
    };
    subBtn.classList.add("miniBtn");
    subBtn.classList.add("btn-del");
/*     subBtn.style.cssFloat = 'right'; */
    li.appendChild(subBtn);
  }
}

async function searchBxChanged() {
  currentRealmItems.forEach(i => i.match = false);
  let term = document.getElementById("searchBox").value.trim();

  let items = currentRealmItems;
  if (term.length === 0) {
    setupHierarchy(items, false);
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

  setupHierarchy(results2, true);
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
  let allChildren = children.concat(children.map(c => getChildren(items, c)).flat(1));
  return allChildren;
}

function editRealmClick () {
  if (allRealms.find(r => r.realm.id == currentRealm).permission < 3) {
    alert("You don't have permission to edit this realm.");
    return;
  }
  location.href = `/editRealm.html?realm=${currentRealm}`;
};

document.getElementById("historyBtn").onclick = () => { location.href = `/history.html?realm=${currentRealm}`; };
document.getElementById("expandAll").onclick = () => { setupHierarchy(lastHierarchyItems, true); }
document.getElementById("contractAll").onclick = () => { setupHierarchy(lastHierarchyItems, false); }