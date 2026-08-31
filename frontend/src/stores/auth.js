import { defineStore } from 'pinia'
import api from '../api/modules'

export const useAuthStore = defineStore('auth', {
  state: () => ({
    token: localStorage.getItem('token') || '',
    user: JSON.parse(localStorage.getItem('user') || 'null'),
    permissions: JSON.parse(localStorage.getItem('permissions') || '[]')
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
      this.permissions = res.permissions || []
      localStorage.setItem('token', res.token)
      localStorage.setItem('user', JSON.stringify(this.user))
      localStorage.setItem('permissions', JSON.stringify(this.permissions))
    },
    async refreshPermissions() {
      if (!this.token) return
      try {
        const res = await api.myPermissions()
        this.permissions = res.permissions || []
        localStorage.setItem('permissions', JSON.stringify(this.permissions))
      } catch { /* ignore */ }
    },
    hasPerm(code) {
      if (this.role === 'admin') return true
      return this.permissions.includes(code)
    },
    logout() {
      this.token = ''
      this.user = null
      this.permissions = []
      localStorage.removeItem('token')
      localStorage.removeItem('user')
      localStorage.removeItem('permissions')
    }
  }
})
