function cleanxss(text) {
  let c = {
    "<": "&lt;",
    ">": "&gt;",
    "'": "&#39;",
    "\"": "&#34;"
  };

  Object.keys(c).forEach(key => {
    text = text.replaceAll(key, c[key]);
  });
  return text;
}

export {
  cleanxss
};