function http(method, theUrl, data = null)
{
  return new Promise((resolve, reject) => {
    var xmlHttp = new XMLHttpRequest();
    xmlHttp.open(method, theUrl, true); // false for synchronous request
    xmlHttp.onload = (e) => {
      if (xmlHttp.status == 401 || xmlHttp.responseText.includes("submitBtn")) {
        location.href = '/login.html';
        return;
      }
      resolve(xmlHttp.responseText);
    };
    xmlHttp.onerror = (e) => {
      console.log(e);
      reject(e);
    };
    if (data != null) {
      xmlHttp.setRequestHeader("Content-Type", "application/json;charset=UTF-8");
      xmlHttp.setRequestHeader("X-Requested-With", "XMLHttpRequest");
    }
    xmlHttp.send(JSON.stringify(data));
  });
}

async function attemptLogin() {
  console.log("Attemping login...");
  let pw = document.getElementById("pwBx").value;
  document.getElementById("pwBx").value = "";
  let un = document.getElementById("unBx").value;

  let r = await http("PUT", `/signin?username=${encodeURIComponent(un)}&password=${encodeURIComponent(pw)}`)
  if (r) {
    location.href = '/tree.html';
  } else {
    alert("Incorrect credentials.");
    location.reload();
  }
};

window.onload = () => {
  document.getElementById("unBx").focus();
}