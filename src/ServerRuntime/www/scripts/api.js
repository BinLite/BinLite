function http(method, theUrl, data = null)
{
  return new Promise((resolve, reject) => {
    var xmlHttp = new XMLHttpRequest();
    xmlHttp.open(method, theUrl, true); // false for synchronous request
    xmlHttp.onload = (e) => {
      if (xmlHttp.status == 401 || xmlHttp.status == 302 || xmlHttp.responseText.includes("submitBtn")) {
        console.log({ status: xmlHttp.status, request: method + " " + theUrl, response: xmlHttp.responseText });
        location.href = '/login.html';
        return;
      }
      console.log({ status: xmlHttp.status, request: method + " " + theUrl, response: JSON.parse(xmlHttp.responseText) });
      resolve(xmlHttp.responseText);
    };
    xmlHttp.onerror = (e) => {
      console.log(e);
      location.href = '/login.html';
    };
    if (data != null) {
      xmlHttp.setRequestHeader("Content-Type", "application/json;charset=UTF-8");
      xmlHttp.setRequestHeader("X-Requested-With", "XMLHttpRequest");
    }
    xmlHttp.send(JSON.stringify(data));
  });
}

async function getItems(realm) {
  return JSON.parse(await http("GET", `/api/items?realm=${encodeURIComponent(realm)}`));
}

async function getItem(id) {
  return JSON.parse(await http("GET", `/api/item?id=${encodeURIComponent(id)}`));
}

const deleteItem = async (id) => JSON.parse(await http("DELETE", `/api/item?id=${encodeURIComponent(id)}`));
const createItem = async (item) => JSON.parse(await http("POST", "/api/item", item));
const updateItem = async (item) => JSON.parse(await http("PUT", "/api/item", item));

//

async function getRealms() {
  return JSON.parse(await http("GET", "/api/realms"));
}

async function getRealm(id) {
  return JSON.parse(await http("GET", `/api/realm?id=${id}`));
}

async function createRealm(realm) {
  return JSON.parse(await http("POST", "/api/realm", realm));
}

async function updateRealm(realm) {
  return JSON.parse(await http("PUT", "/api/realm", realm));
}

//

async function getPermission(realmId, userId) {
  return JSON.parse(await http("GET", `/api/realm/user?user=${encodeURIComponent(userId)}&realm=${encodeURIComponent(realmId)}`));
}

async function getPermissions(realmId) {
  return JSON.parse(await http("GET", `/api/realm/users?realm=${encodeURIComponent(realmId)}`));
}

async function setPermission(realmId, userId, perm) {
  return JSON.parse(await http("PUT", `/api/realm/user?user=${encodeURIComponent(userId)}&realm=${encodeURIComponent(realmId)}&permission=${perm}`));
}

//

async function getUsers() {
  return JSON.parse(await http("GET", "/api/users"));
}

async function getUser() {
  return JSON.parse(await http("GET", "/api/user"));
}

async function createUser(username, email) {
  return JSON.parse(await http("POST", `/api/user?username=${encodeURIComponent(username)}&email=${encodeURIComponent(email)}`));
}

async function changePassword(oldPw, newPw) {
  return JSON.parse(await http("PUT", `/api/user/password?oldPass=${encodeURIComponent(oldPw)}&newPass=${encodeURIComponent(newPw)}`));
}

async function changeEmail(user, newEmail) {
  return JSON.parse(await http("PUT", `/api/user/email?email=${encodeURIComponent(newEmail)}&user=${encodeURIComponent(user)}`));
}

async function resetPassword(user) {
  return JSON.parse(await http("PUT", `/api/user/reset?user=${encodeURIComponent(user)}`));
}

//

async function getHistory(page, realm, source) {
  return JSON.parse(await http("GET", `/api/realm/history?realm=${(realm ?? "")}&source=${(source ?? "")}&page=${page}`))
}

async function getHistorySize(page, realm, source) {
  return JSON.parse(await http("GET", `/api/realm/historysize?realm=${(realm ?? "")}&source=${(source ?? "")}&page=${page}`))
}

//

async function getIds(count) {
  return JSON.parse(await http("GET", `/api/ids?count=${encodeURIComponent(count)}`))
}

export {
  http,
  getItems,
  getItem,
  deleteItem,
  createItem,
  updateItem,
  getRealms,
  getRealm,
  createRealm,
  updateRealm,
  getPermission,
  getPermissions,
  setPermission,
  getUsers,
  getUser,
  createUser,
  changePassword,
  changeEmail,
  resetPassword,
  getHistory,
  getHistorySize,
  getIds,
}
