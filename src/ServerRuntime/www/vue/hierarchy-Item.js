Vue.component('hierarchy-item', {
  template: `
    <li>
      <div class="flex space-between hover-tree-color">
        <div>
          <span :class="[
              item.children.length > 0 ? 'caret' : 'emptyCaret',
              {'caret-down': expanded},
              {'match': item.match}
            ]"
          >
            {{item.name}}
          </span>
        </div>
        <div>
          <button class="miniBtn btn-add" @click="addRealm"></button>
          <button class="miniBtn btn-edit" @click="editRealm"></button>
          <button class="miniBtn btn-delete" @click="deleteRealm"></button>
        </div>
      </div>
      <ul v-if="expanded" :class="['nested', {active: expanded}]">
        <hierarchy-item
          v-for="sub of item.children"
          :item="sub"
          :expanded="expanded"
          :key="sub.id"
        />
      </ul>
    </li>
  `,
  inject: ['currentRealm'],
  props: {
    item: {
      type: Object,
      required: true
    }
  },
  data() {
    return {
      expanded: false
    }
  },
  computed: {
    currentRealm() {

    }
  },
  methods: {
    addRealm: () => {
      if (this.currentRealmId == 0) { alert("No valid realm selected."); return; }
      let item = this.item == null || this.item == undefined ? null : this.item.id;
        window.location.href = '/editItem.html?parent=' + item + "&mode=create&realm=" + currentRealm;
    },
    editRealm: async () => {

    },
    deleteRealm: async () => {

    }
  }
})