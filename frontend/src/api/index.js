import axios from 'axios'
import { ElMessage } from 'element-plus'
import router from '../router'

const api = axios.create({
  baseURL: '/api',
  timeout: 60000
})

api.interceptors.request.use(config => {
  const token = localStorage.getItem('token')
  if (token) config.headers.Authorization = `Bearer ${token}`
  return config
})

api.interceptors.response.use(
  res => res.data,
  err => {
    const msg = err.response?.data?.message || err.message || '请求失败'
    const isLogin = err.config?.url?.includes('/Auth/login')
    if (err.response?.status === 401 && !isLogin) {
      localStorage.removeItem('token')
      localStorage.removeItem('user')
      router.push('/login')
      ElMessage.warning('登录已过期，请重新登录')
    } else if (err.response?.status === 401 && isLogin) {
      ElMessage.error(typeof msg === 'string' ? msg : '验证码错误')
    } else {
      ElMessage.error(typeof msg === 'string' ? msg : '请求失败')
    }
    return Promise.reject(err)
  }
)

export default api
