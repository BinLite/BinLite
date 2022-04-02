import { getUser } from './api.js';

function logout() {
  var xmlHttp = new XMLHttpRequest();
  xmlHttp.open("get", "/api/ping", false, "null", "null"); // false for synchronous request
  xmlHttp.send();
  window.location.replace("/");
}

window.addEventListener("load", async function(event) {
  if (this.window.parent.location.href.includes("unauthorized.html")) { return; }
  if (this.window.parent.location.href.includes("api.html")) { return; }
  let user = await getUser();
  if (user && user.serverAdmin) {
    let admin = this.document.createElement("button");
    admin.classList.add("miniBtn");
    admin.classList.add("btn-set");

    this.document.getElementById("adminA").appendChild(admin);
  }
  if (user) {
    let log = this.document.createElement("button");
    log.classList.add("miniBtn");
    log.classList.add("btn-log");
    log.onclick = logout;

    this.document.getElementById("logoutSpan").appendChild(log);


    let pro = this.document.createElement("button");
    pro.classList.add("miniBtn");
    pro.classList.add("btn-pro");

    this.document.getElementById("userA").appendChild(pro);
  }
},false);
