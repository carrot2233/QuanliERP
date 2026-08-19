import { defineStore } from 'pinia'
import api from '../api/modules'

export const useAuthStore = defineStore('auth', {
  state: () => ({
    token: localStorage.getItem('token') || '',
    user: JSON.parse(localStorage.getItem('user') || 'null')
  }),
  getters: {
    isLogin: s => !!s.token,
    displayName: s => s.user?.displayName || s.user?.username || '',
    role: s => s.user?.role || ''
  },
  actions: {
    async login(username, password, captchaKey, captchaCode) {
      const res = await api.login({ username, password, captchaKey, captchaCode })
      this.token = res.token
      this.user = { username: res.username, displayName: res.displayName, role: res.role }
      localStorage.setItem('token', res.token)
      localStorage.setItem('user', JSON.stringify(this.user))
    },
    logout() {
      this.token = ''
      this.user = null
      localStorage.removeItem('token')
      localStorage.removeItem('user')
    }
  }
})
