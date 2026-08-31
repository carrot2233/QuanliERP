<template>
  <el-container class="layout">
    <el-aside :width="collapsed ? '64px' : '220px'" class="aside">
      <div class="logo">
        <el-icon :size="26" color="#fff"><OfficeBuilding /></el-icon>
        <span v-show="!collapsed">全力模具ERP</span>
        <div class="collapse-btn" @click="collapsed = !collapsed">
          <el-icon :size="18"><component :is="collapsed ? Expand : Fold" /></el-icon>
        </div>
      </div>
      <el-menu
        :default-active="$route.path"
        router
        :collapse="collapsed"
        background-color="#001529"
        text-color="#a6adb4"
        active-text-color="#409eff"
        class="menu"
        :collapse-transition="false"
      >
        <el-sub-menu v-if="auth.hasPerm('base:customer') || auth.hasPerm('base:supplier') || auth.hasPerm('base:material') || auth.hasPerm('base:product') || auth.hasPerm('base:warehouse') || auth.hasPerm('base:employee')" index="base">
          <template #title><el-icon><Setting /></el-icon><span>基础数据</span></template>
          <el-menu-item v-if="auth.hasPerm('base:customer')" index="/base/customers">客户管理</el-menu-item>
          <el-menu-item v-if="auth.hasPerm('base:supplier')" index="/base/suppliers">供应商管理</el-menu-item>
          <el-menu-item v-if="auth.hasPerm('base:material')" index="/base/materials">原材料管理</el-menu-item>
          <el-menu-item v-if="auth.hasPerm('base:product')" index="/base/products">产品管理</el-menu-item>
          <el-menu-item v-if="auth.hasPerm('base:warehouse')" index="/base/warehouses">仓库管理</el-menu-item>
          <el-menu-item v-if="auth.hasPerm('base:employee')" index="/base/employees">员工管理</el-menu-item>
        </el-sub-menu>
        <el-sub-menu v-if="auth.hasPerm('sales:order') || auth.hasPerm('sales:delivery')" index="sales">
          <template #title><el-icon><ShoppingCart /></el-icon><span>销售管理</span></template>
          <el-menu-item v-if="auth.hasPerm('sales:order')" index="/sales/orders">销售订单</el-menu-item>
          <el-menu-item v-if="auth.hasPerm('sales:delivery')" index="/sales/deliveries">发货管理</el-menu-item>
        </el-sub-menu>
        <el-sub-menu v-if="auth.hasPerm('purchase:order') || auth.hasPerm('purchase:receipt')" index="purchase">
          <template #title><el-icon><ShoppingCartFull /></el-icon><span>采购管理</span></template>
          <el-menu-item v-if="auth.hasPerm('purchase:order')" index="/purchase/orders">采购订单</el-menu-item>
          <el-menu-item v-if="auth.hasPerm('purchase:receipt')" index="/purchase/receipts">到货管理</el-menu-item>
        </el-sub-menu>
        <el-sub-menu v-if="auth.hasPerm('warehouse:inventory') || auth.hasPerm('warehouse:stock-in') || auth.hasPerm('warehouse:stock-out') || auth.hasPerm('warehouse:ledger') || auth.hasPerm('warehouse:warning')" index="warehouse">
          <template #title><el-icon><Box /></el-icon><span>仓库管理</span></template>
          <el-menu-item v-if="auth.hasPerm('warehouse:inventory')" index="/warehouse/inventory">库存查询</el-menu-item>
          <el-menu-item v-if="auth.hasPerm('warehouse:stock-in')" index="/warehouse/stock-in">入库管理</el-menu-item>
          <el-menu-item v-if="auth.hasPerm('warehouse:stock-out')" index="/warehouse/stock-out">出库管理</el-menu-item>
          <el-menu-item v-if="auth.hasPerm('warehouse:ledger')" index="/warehouse/ledger">库存流水</el-menu-item>
          <el-menu-item v-if="auth.hasPerm('warehouse:warning')" index="/warehouse/warnings">库存预警</el-menu-item>
        </el-sub-menu>
        <el-sub-menu v-if="auth.hasPerm('schedule:shift') || auth.hasPerm('schedule:work')" index="schedule">
          <template #title><el-icon><Calendar /></el-icon><span>排班管理</span></template>
          <el-menu-item v-if="auth.hasPerm('schedule:shift')" index="/schedule/shifts">班次设置</el-menu-item>
          <el-menu-item v-if="auth.hasPerm('schedule:work')" index="/schedule/work">排班计划</el-menu-item>
        </el-sub-menu>
        <el-sub-menu v-if="auth.hasPerm('production:plan') || auth.hasPerm('production:order') || auth.hasPerm('production:daily')" index="production">
          <template #title><el-icon><SetUp /></el-icon><span>生产管理</span></template>
          <el-menu-item v-if="auth.hasPerm('production:plan')" index="/production/plans">生产计划</el-menu-item>
          <el-menu-item v-if="auth.hasPerm('production:order')" index="/production/orders">冲压产量单</el-menu-item>
          <el-menu-item v-if="auth.hasPerm('production:daily')" index="/production/daily">生产日报</el-menu-item>
        </el-sub-menu>
        <el-sub-menu v-if="auth.hasPerm('mold:list') || auth.hasPerm('mold:plan')" index="mold">
          <template #title><el-icon><Tools /></el-icon><span>模具/工装夹具管理</span></template>
          <el-menu-item v-if="auth.hasPerm('mold:list')" index="/mold/list">模具台账</el-menu-item>
          <el-menu-item v-if="auth.hasPerm('mold:plan')" index="/mold/plans">模具制造计划</el-menu-item>
        </el-sub-menu>
        <el-sub-menu v-if="auth.hasPerm('quality:inspection') || auth.hasPerm('quality:tool') || auth.hasPerm('quality:toolapply') || auth.hasPerm('quality:toolscrap') || auth.hasPerm('quality:calibration')" index="quality">
          <template #title><el-icon><CircleCheck /></el-icon><span>质量管理</span></template>
          <el-menu-item v-if="auth.hasPerm('quality:inspection')" index="/quality/inspections">质检记录</el-menu-item>
          <el-menu-item v-if="auth.hasPerm('quality:tool')" index="/quality/tools">计量器具台账</el-menu-item>
          <el-menu-item v-if="auth.hasPerm('quality:toolapply')" index="/quality/toolapply">量具申购</el-menu-item>
          <el-menu-item v-if="auth.hasPerm('quality:toolscrap')" index="/quality/toolscrap">器具报废</el-menu-item>
          <el-menu-item v-if="auth.hasPerm('quality:calibration')" index="/quality/calibration">检定处理</el-menu-item>
        </el-sub-menu>
        <el-sub-menu v-if="auth.hasPerm('equipment:list') || auth.hasPerm('equipment:maintenance')" index="equipment">
          <template #title><el-icon><Cpu /></el-icon><span>设备管理</span></template>
          <el-menu-item v-if="auth.hasPerm('equipment:list')" index="/equipment/list">设备台账</el-menu-item>
          <el-menu-item v-if="auth.hasPerm('equipment:maintenance')" index="/equipment/maintenance">维护记录</el-menu-item>
        </el-sub-menu>
        <el-sub-menu v-if="auth.hasPerm('oa:notice') || auth.hasPerm('oa:message') || auth.hasPerm('oa:myflow') || auth.hasPerm('oa:todo') || auth.hasPerm('oa:flowdesign') || auth.hasPerm('oa:done') || auth.hasPerm('oa:file')" index="oa">
          <template #title><el-icon><EditPen /></el-icon><span>协同办公</span></template>
          <el-menu-item v-if="auth.hasPerm('oa:notice')" index="/oa/notices">通知公告</el-menu-item>
          <el-menu-item v-if="auth.hasPerm('oa:message')" index="/oa/messages">消息中心</el-menu-item>
          <el-menu-item v-if="auth.hasPerm('oa:myflow')" index="/oa/my-flow">我的流程</el-menu-item>
          <el-menu-item v-if="auth.hasPerm('oa:todo')" index="/oa/todo">待办事项</el-menu-item>
          <el-menu-item v-if="auth.hasPerm('oa:flowdesign')" index="/oa/flow-design">流程设计</el-menu-item>
          <el-menu-item v-if="auth.hasPerm('oa:done')" index="/oa/done">已办事项</el-menu-item>
          <el-menu-item v-if="auth.hasPerm('oa:file')" index="/oa/files">文件管理</el-menu-item>
        </el-sub-menu>
        <el-sub-menu v-if="auth.hasPerm('hr:employee') || auth.hasPerm('hr:attendance') || auth.hasPerm('hr:leave') || auth.hasPerm('hr:payroll') || auth.hasPerm('hr:training')" index="hr">
          <template #title><el-icon><UserFilled /></el-icon><span>人力资源管理</span></template>
          <el-menu-item v-if="auth.hasPerm('hr:employee')" index="/hr/employees">员工档案</el-menu-item>
          <el-menu-item v-if="auth.hasPerm('hr:attendance')" index="/hr/attendance">考勤管理</el-menu-item>
          <el-menu-item v-if="auth.hasPerm('hr:leave')" index="/hr/leave">请假管理</el-menu-item>
          <el-menu-item v-if="auth.hasPerm('hr:payroll')" index="/hr/payroll">薪资管理</el-menu-item>
          <el-menu-item v-if="auth.hasPerm('hr:training')" index="/hr/training">培训管理</el-menu-item>
        </el-sub-menu>
        <el-menu-item v-if="auth.hasPerm('dashboard')" index="/dashboard">
          <el-icon><Odometer /></el-icon><span>驾驶舱管理</span>
        </el-menu-item>
        <el-sub-menu v-if="auth.role === 'admin'" index="system">
          <template #title><el-icon><User /></el-icon><span>系统管理</span></template>
          <el-menu-item index="/system/users">用户管理</el-menu-item>
          <el-menu-item index="/system/roles">角色权限</el-menu-item>
        </el-sub-menu>
      </el-menu>
    </el-aside>
    <el-container>
      <el-header class="header">
        <div class="breadcrumb">
          <el-breadcrumb separator="/">
            <el-breadcrumb-item>鹤壁市全力模具制造有限公司</el-breadcrumb-item>
            <el-breadcrumb-item>{{ $route.meta.title }}</el-breadcrumb-item>
          </el-breadcrumb>
        </div>
        <div class="user">
          <el-tag v-if="auth.role" size="small" type="success" effect="plain">{{ roleName }}</el-tag>
          <el-popover placement="bottom-end" :width="320" trigger="click" @show="loadUnread">
            <template #reference>
              <el-badge :value="unreadCount" :hidden="unreadCount === 0" :max="99" class="bell-badge">
                <el-icon :size="20" class="bell-icon"><Bell /></el-icon>
              </el-badge>
            </template>
            <div class="unread-panel">
              <div class="unread-header">
                <span>未读消息</span>
                <el-button v-if="unreadList.length" link type="primary" size="small" @click="goMessages">查看全部</el-button>
              </div>
              <div v-if="unreadList.length === 0" class="unread-empty">暂无未读消息</div>
              <div v-for="m in unreadList" :key="m.id" class="unread-item" @click="openMessage(m)">
                <el-tag :type="typeColor(m.msgType)" size="small" class="unread-type">{{ m.msgType }}</el-tag>
                <span class="unread-content">{{ m.content }}</span>
                <span class="unread-time">{{ fmtTime(m.createdAt) }}</span>
              </div>
            </div>
          </el-popover>
          <el-dropdown @command="handleCommand">
            <span class="user-name">
              <el-avatar :size="30" style="background:#409eff">{{ auth.displayName?.charAt(0) }}</el-avatar>
              {{ auth.displayName }}
              <el-icon><ArrowDown /></el-icon>
            </span>
            <template #dropdown>
              <el-dropdown-menu>
                <el-dropdown-item command="logout">退出登录</el-dropdown-item>
              </el-dropdown-menu>
            </template>
          </el-dropdown>
        </div>
      </el-header>
      <div class="tabs-bar">
        <button v-show="canScrollLeft" class="tabs-arrow left" @click="scrollTabs(-200)"><el-icon><ArrowLeft /></el-icon></button>
        <div ref="tabsWrapRef" class="tabs-wrap" @scroll="updateScrollBtns">
          <div
            v-for="tab in tabs.list"
            :key="tab.path"
            class="tab-item"
            :class="{ active: $route.path === tab.path }"
            @click="router.push(tab.path)"
            @contextmenu.prevent="openCtx($event, tab)"
          >
            <el-icon v-if="routeIconMap[tab.path]" class="tab-icon"><component :is="routeIconMap[tab.path]" /></el-icon>
            <span class="tab-title">{{ tab.title }}</span>
            <el-icon v-if="tab.path !== '/dashboard'" class="tab-close" @click.stop="closeTab(tab.path)"><Close /></el-icon>
          </div>
        </div>
        <button v-show="canScrollRight" class="tabs-arrow right" @click="scrollTabs(200)"><el-icon><ArrowRight /></el-icon></button>
      </div>
      <div v-if="ctxMenu.visible" class="ctx-overlay" @click="ctxMenu.visible = false" @contextmenu.prevent="ctxMenu.visible = false"></div>
      <div v-if="ctxMenu.visible" class="ctx-menu" :style="{ left: ctxMenu.x + 'px', top: ctxMenu.y + 'px' }">
        <div class="ctx-item" @click="closeCtxTab(ctxMenu.path)">关闭当前</div>
        <div class="ctx-item" @click="closeCtxOthers(ctxMenu.path)">关闭其他</div>
        <div class="ctx-item" @click="closeCtxAll">关闭全部</div>
      </div>
      <el-main class="main">
        <router-view />
      </el-main>
    </el-container>
  </el-container>
</template>

<script setup>
import { computed, watch, ref, reactive, nextTick, onMounted, onUpdated } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'
import { useTabsStore } from '../stores/tabs'
import { Odometer, ShoppingCart, ShoppingCartFull, Box, Calendar, SetUp, CircleCheck, Cpu, Tools, Setting, User, OfficeBuilding, ArrowDown, ArrowLeft, ArrowRight, Close, Fold, Expand, EditPen, UserFilled, Bell } from '@element-plus/icons-vue'
import api from '../api/modules'

const routeIconMap = {
  '/dashboard': Odometer,
  '/sales/orders': ShoppingCart,
  '/sales/deliveries': ShoppingCart,
  '/purchase/orders': ShoppingCartFull,
  '/purchase/receipts': ShoppingCartFull,
  '/warehouse/inventory': Box,
  '/warehouse/stock-in': Box,
  '/warehouse/stock-out': Box,
  '/warehouse/ledger': Box,
  '/warehouse/warnings': Box,
  '/schedule/shifts': Calendar,
  '/schedule/work': Calendar,
  '/production/plans': SetUp,
  '/production/orders': SetUp,
  '/production/daily': SetUp,
  '/quality/inspections': CircleCheck,
  '/quality/tools': CircleCheck,
  '/quality/toolapply': CircleCheck,
  '/quality/toolscrap': CircleCheck,
  '/quality/calibration': CircleCheck,
  '/equipment/list': Cpu,
  '/equipment/maintenance': Cpu,
  '/mold/list': Tools,
  '/mold/plans': Tools,
  '/base/customers': Setting,
  '/base/suppliers': Setting,
  '/base/materials': Setting,
  '/base/products': Setting,
  '/base/warehouses': Setting,
  '/base/employees': Setting,
  '/system/users': User,
  '/system/roles': User,
  '/oa/notices': EditPen,
  '/oa/messages': EditPen,
  '/oa/my-flow': EditPen,
  '/oa/todo': EditPen,
  '/oa/flow-design': EditPen,
  '/oa/done': EditPen,
  '/oa/files': EditPen,
  '/hr/employees': UserFilled,
  '/hr/attendance': UserFilled,
  '/hr/leave': UserFilled,
  '/hr/payroll': UserFilled,
  '/hr/training': UserFilled
}

const auth = useAuthStore()
const tabs = useTabsStore()
const router = useRouter()
const route = useRoute()
const tabsWrapRef = ref(null)
const collapsed = ref(false)
const canScrollLeft = ref(false)
const canScrollRight = ref(false)
const unreadCount = ref(0)
const unreadList = ref([])

async function loadUnreadCount() {
  try {
    const res = await api.unreadCount(auth.displayName)
    unreadCount.value = res.count
  } catch { /* ignore */ }
}

async function loadUnread() {
  try {
    unreadList.value = await api.unreadMessages(auth.displayName, 8)
  } catch { /* ignore */ }
}

async function openMessage(m) {
  try {
    await api.messageRead(m.id)
    loadUnreadCount()
    loadUnread()
  } catch { /* ignore */ }
  router.push('/oa/messages')
}

function goMessages() {
  router.push('/oa/messages')
}

function typeColor(t) {
  return { 系统消息: 'info', 审批消息: 'success', 待办消息: 'warning' }[t] || 'primary'
}

function fmtTime(v) {
  if (!v) return ''
  const s = String(v).replace('T', ' ').slice(5, 16)
  return s
}

onMounted(() => {
  checkScroll()
  loadUnreadCount()
})

watch(() => auth.displayName, loadUnreadCount)

function updateScrollBtns() {
  const el = tabsWrapRef.value
  if (!el) return
  canScrollLeft.value = el.scrollLeft > 0
  canScrollRight.value = el.scrollLeft + el.clientWidth < el.scrollWidth - 1
}

function scrollTabs(offset) {
  const el = tabsWrapRef.value
  if (el) el.scrollBy({ left: offset, behavior: 'smooth' })
}

function checkScroll() {
  nextTick(updateScrollBtns)
}

watch(() => tabs.list.length, checkScroll)
onUpdated(checkScroll)

watch(() => route.path, () => {
  if (route.meta.title) tabs.add({ path: route.path, title: route.meta.title, icon: routeIconMap[route.path]?.name || '' })
  nextTick(() => {
    const el = tabsWrapRef.value
    if (!el) return
    const active = el.querySelector('.tab-item.active')
    if (active) {
      const aL = active.offsetLeft
      const aR = aL + active.offsetWidth
      if (aR > el.scrollLeft + el.clientWidth) el.scrollLeft = aR - el.clientWidth + 10
      else if (aL < el.scrollLeft) el.scrollLeft = aL - 10
    }
    updateScrollBtns()
  })
}, { immediate: true })

const roleName = computed(() => ({
  admin: '系统管理员', production: '生产', warehouse: '仓库', quality: '质量', sales: '销售'
}[auth.role] || auth.role))

function closeTab(path) {
  const next = tabs.remove(path)
  if (route.path === path) router.push(next)
}

const ctxMenu = reactive({ visible: false, x: 0, y: 0, path: '' })

function openCtx(e, tab) {
  ctxMenu.x = e.clientX
  ctxMenu.y = e.clientY
  ctxMenu.path = tab.path
  ctxMenu.visible = true
}

function closeCtxTab(path) {
  ctxMenu.visible = false
  closeTab(path)
}

function closeCtxOthers(path) {
  ctxMenu.visible = false
  tabs.closeOthers(path)
  if (route.path !== path && !tabs.list.some(t => t.path === route.path)) {
    router.push(path)
  }
}

function closeCtxAll() {
  ctxMenu.visible = false
  tabs.closeAll()
  if (route.path !== '/dashboard') router.push('/dashboard')
}

function handleCommand(cmd) {
  if (cmd === 'logout') {
    auth.logout()
    router.push('/login')
  }
}
</script>

<style scoped>
.layout { height: 100%; }
.aside { background: #001529; flex-shrink: 0; transition: width .25s ease; display: flex; flex-direction: column; overflow: hidden; }
.logo { display: flex; align-items: center; gap: 8px; color: #fff; font-size: 18px; font-weight: bold; height: 44px; padding: 0 14px; flex-shrink: 0; position: relative; }
.collapse-btn { position: absolute; right: 0; top: 0; bottom: 0; width: 32px; display: flex; align-items: center; justify-content: center; cursor: pointer; color: #a6adb4; transition: all .2s; }
.collapse-btn:hover { color: #fff; background: rgba(255,255,255,.08); }
.menu { border-right: none; overflow-y: auto; flex: 1; min-height: 0; }
.menu::-webkit-scrollbar { width: 4px; }
.header { background: #fff; display: flex; align-items: center; justify-content: space-between; box-shadow: 0 1px 4px rgba(0,21,41,.08); height: 44px; padding: 0 16px; flex-shrink: 0; }
.user { display: flex; align-items: center; gap: 10px; }
.user-name { display: flex; align-items: center; gap: 6px; cursor: pointer; font-size: 14px; }
.bell-badge { cursor: pointer; display: flex; align-items: center; }
.bell-icon { color: #666; }
.bell-icon:hover { color: #409eff; }
.unread-panel { max-height: 360px; overflow-y: auto; }
.unread-header { display: flex; align-items: center; justify-content: space-between; font-weight: bold; margin-bottom: 8px; }
.unread-empty { color: #999; text-align: center; padding: 16px 0; font-size: 13px; }
.unread-item { display: flex; align-items: center; gap: 8px; padding: 8px 4px; border-bottom: 1px solid #f0f0f0; cursor: pointer; }
.unread-item:hover { background: #f5f7fa; }
.unread-type { flex-shrink: 0; }
.unread-content { flex: 1; min-width: 0; font-size: 13px; color: #333; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
.unread-time { flex-shrink: 0; font-size: 12px; color: #999; }
.tabs-bar { background: #fff; border-bottom: 1px solid #e8e8e8; padding: 0; display: flex; align-items: center; position: relative; height: 42px; flex-shrink: 0; }
.tabs-wrap { display: flex; gap: 6px; overflow-x: auto; padding: 0 12px; flex: 1; min-width: 0; align-items: center; }
.tabs-wrap::-webkit-scrollbar { height: 0; }
.tabs-arrow { flex-shrink: 0; width: 26px; height: 28px; border: 1px solid #d9d9d9; background: #fff; cursor: pointer; display: flex; align-items: center; justify-content: center; border-radius: 4px; color: #666; font-size: 12px; z-index: 1; }
.tabs-arrow:hover { color: #409eff; border-color: #409eff; background: #ecf5ff; }
.tabs-arrow.left { margin-left: 4px; }
.tabs-arrow.right { margin-right: 4px; }
.tab-item { display: flex; align-items: center; gap: 6px; padding: 6px 16px; border-radius: 6px; cursor: pointer; font-size: 14px; white-space: nowrap; border: 1px solid #d9d9d9; background: #fafafa; color: #666; transition: all .2s; line-height: 1; }
.tab-item:hover { color: #409eff; border-color: #b3d8ff; background: #ecf5ff; }
.tab-item.active { color: #fff; background: #409eff; border-color: #409eff; }
.tab-icon { font-size: 15px; flex-shrink: 0; }
.tab-close { font-size: 12px; border-radius: 50%; padding: 1px; flex-shrink: 0; }
.tab-close:hover { background: rgba(0,0,0,.15); }
.tab-item.active .tab-close:hover { background: rgba(255,255,255,.3); }
.main { padding: 16px 24px; overflow-y: auto; flex: 1; min-height: 0; scrollbar-gutter: stable; }
.ctx-overlay { position: fixed; inset: 0; z-index: 1999; }
.ctx-menu { position: fixed; z-index: 2000; background: #fff; border-radius: 6px; box-shadow: 0 4px 16px rgba(0,0,0,.18); padding: 4px 0; min-width: 130px; }
.ctx-item { padding: 7px 16px; font-size: 13px; color: #333; cursor: pointer; white-space: nowrap; }
.ctx-item:hover { background: #ecf5ff; color: #409eff; }
</style>
