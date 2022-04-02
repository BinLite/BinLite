import { getUser } from './api.js';

function logout() {
  var xmlHttp = new XMLHttpRequest();
  xmlHttp.open("get", "/api/ping", false, "null", "null"); // false for synchronous request
  xmlHttp.send();
  window.location.replace("/");
}

function createHeader() {
  console.log("Loading header...");
  console.log("Header loaded.");
}

window.addEventListener("load", async function(event) {
  createHeader();

  let user = await getUser();
  if (user && user.serverAdmin) {
    let a = this.document.createElement("a");
    a.classList.add("headerItem");
    a.href = "/admin.html";
    a.innerText = "Server Admin";

    let parent = this.document.getElementById("serverAdminDiv");
    parent.appendChild(a);
  }
  if (user) {

    this.document.getElementById("logoutBtn").onclick = logout;
  }
},false);
