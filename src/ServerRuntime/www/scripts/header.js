import { getUser } from './api.js';

function logout() {
  var xmlHttp = new XMLHttpRequest();
  xmlHttp.open("get", "/api/ping", false, "null", "null"); // false for synchronous request
  xmlHttp.send();
  window.location.replace("/");
}

window.addEventListener("load", async function(event) {
  let user = await getUser();
  if (user && user.serverAdmin) {
    let admin = this.document.createElement("a");
    admin.classList.add("headerItem");
    admin.href = "/admin.html";
    admin.innerText = "Server Admin";

    this.document.getElementById("serverAdminSpan").appendChild(admin);
  }
  if (user) {
    let logout = this.document.createElement("a");
    logout.classList.add("headerItem");
    logout.href = "";
    logout.innerHTML = "Logout";
    logout.onclick = logout;

    this.document.getElementById("logoutSpan").appendChild(logout);
  }
},false);
