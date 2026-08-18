import { defineStore } from 'pinia'

export const useTabsStore = defineStore('tabs', {
  state: () => ({
    list: JSON.parse(sessionStorage.getItem('tabs') || 'null') || [{ path: '/dashboard', title: '驾驶舱管理' }]
  }),
  actions: {
    _save() {
      sessionStorage.setItem('tabs', JSON.stringify(this.list))
    },
    add(tab) {
      if (!this.list.some(t => t.path === tab.path)) {
        this.list.push(tab)
        this._save()
      }
    },
    remove(path) {
      const idx = this.list.findIndex(t => t.path === path)
      if (idx > -1) {
        this.list.splice(idx, 1)
        this._save()
      }
      return this.list.length ? this.list[this.list.length - 1].path : '/dashboard'
    },
    closeOthers(path) {
      this.list = this.list.filter(t => t.path === path || t.path === '/dashboard')
      this._save()
    },
    closeAll() {
      this.list = [{ path: '/dashboard', title: '驾驶舱管理' }]
      this._save()
    }
  }
})
