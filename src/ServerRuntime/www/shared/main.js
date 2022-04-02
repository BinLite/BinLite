import { getUser } from "/shared/api.js";

function logout() {
  var xmlHttp = new XMLHttpRequest();
  xmlHttp.open("get", "/api/ping", false, "null", "null"); // false for synchronous request
  xmlHttp.send();
  window.location.replace("/");
}

window.addEventListener("load", async function(event) {
  function zoomOutMobile() {
    var viewport = document.querySelector('meta[name="viewport"]');

    if ( viewport ) {
      viewport.content = "initial-scale=0.1";
      viewport.content = "width=400";
    }
  }

  if( /Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent) ) {
    zoomOutMobile();
  }

  let user = await getUser();
  if (user.serverAdmin) {
    // Add:
    //<a class="headerItem" href="/admin.html">ServerAdmin</a>

    let a = this.document.createElement("a");
    a.classList.add("headerItem");
    a.href = "/admin.html";
    a.innerText = "ServerAdmin";

    let parent = this.document.getElementById("serverAdminDiv");
    parent.appendChild(a);
  }

  this.document.getElementById("logoutBtn").onclick = logout;
},false);
