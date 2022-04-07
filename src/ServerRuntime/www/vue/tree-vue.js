new Vue({
  el: '#content',
  provide() {
    return {
      currentRealm: currentRealm, // may need to change
    }
  }
})