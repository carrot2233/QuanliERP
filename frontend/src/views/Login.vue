<template>
  <div class="login-page">
    <div class="login-card">
      <div class="login-title">
        <h2>全力模具制造 ERP 系统</h2>
        <p>鹤壁市全力模具制造有限公司</p>
      </div>
      <el-form :model="form" @keyup.enter="doLogin">
        <el-form-item>
          <el-input v-model="form.username" placeholder="用户名" size="large" :prefix-icon="User" />
        </el-form-item>
        <el-form-item>
          <el-input v-model="form.password" type="password" placeholder="密码" size="large" show-password :prefix-icon="Lock" />
        </el-form-item>
        <el-form-item>
          <div class="captcha-row">
            <el-input v-model="form.captchaCode" placeholder="验证码" size="large" maxlength="4" style="flex:1" :prefix-icon="Key" />
            <img v-if="captchaImg" :src="captchaImg" class="captcha-img" @click="loadCaptcha" title="点击刷新" />
          </div>
        </el-form-item>
        <el-form-item>
          <el-button type="primary" size="large" style="width:100%" :loading="loading" @click="doLogin">登 录</el-button>
        </el-form-item>
      </el-form>
      <div class="tips">默认管理员：admin / admin123</div>
    </div>
  </div>
</template>

<script setup>
import { reactive, ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import { User, Lock, Key } from '@element-plus/icons-vue'
import { useAuthStore } from '../stores/auth'
import api from '../api/modules'

const router = useRouter()
const auth = useAuthStore()
const form = reactive({ username: 'admin', password: 'admin123', captchaKey: '', captchaCode: '' })
const loading = ref(false)
const captchaImg = ref('')

async function loadCaptcha() {
  try {
    const res = await api.captcha()
    captchaImg.value = res.image
    form.captchaKey = res.key
    form.captchaCode = ''
  } catch {
    captchaImg.value = ''
  }
}

onMounted(loadCaptcha)

async function doLogin() {
  if (!form.username || !form.password) return ElMessage.warning('请输入用户名和密码')
  if (!form.captchaCode) return ElMessage.warning('请输入验证码')
  loading.value = true
  try {
    await auth.login(form.username, form.password, form.captchaKey, form.captchaCode)
    router.push('/dashboard')
  } catch {
    loadCaptcha()
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
.login-page { height: 100%; display: flex; align-items: center; justify-content: center;
  background: linear-gradient(135deg, rgba(15,34,71,.82) 0%, rgba(31,59,115,.82) 50%, rgba(45,90,160,.82) 100%), url(@/assets/login-bg.jpg) center/cover no-repeat; }
.login-card { width: 400px; padding: 40px; background: #fff; border-radius: 8px; box-shadow: 0 10px 40px rgba(0,0,0,.3); }
.login-title { text-align: center; margin-bottom: 28px; }
.login-title h2 { color: #1f3b73; margin-bottom: 6px; }
.login-title p { color: #999; font-size: 13px; }
.captcha-row { display: flex; gap: 12px; width: 100%; align-items: center; }
.captcha-img { height: 40px; width: 120px; border-radius: 4px; border: 1px solid #dcdfe6; cursor: pointer; flex-shrink: 0; }
.tips { text-align: center; color: #bbb; font-size: 12px; margin-top: 10px; }
</style>
