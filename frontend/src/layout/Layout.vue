<template>
  <el-container class="layout">
    <el-aside width="220px" class="aside">
      <div class="logo">
        <el-icon :size="26" color="#fff"><OfficeBuilding /></el-icon>
        <span>全力模具ERP</span>
      </div>
      <el-menu
        :default-active="$route.path"
        router
        background-color="#001529"
        text-color="#a6adb4"
        active-text-color="#409eff"
        class="menu"
      >
        <el-menu-item index="/dashboard">
          <el-icon><Odometer /></el-icon><span>驾驶舱管理</span>
        </el-menu-item>
        <el-sub-menu index="sales">
          <template #title><el-icon><ShoppingCart /></el-icon><span>销售管理</span></template>
          <el-menu-item index="/sales/orders">销售订单</el-menu-item>
          <el-menu-item index="/sales/deliveries">发货管理</el-menu-item>
        </el-sub-menu>
        <el-sub-menu index="purchase">
          <template #title><el-icon><ShoppingCartFull /></el-icon><span>采购管理</span></template>
          <el-menu-item index="/purchase/orders">采购订单</el-menu-item>
          <el-menu-item index="/purchase/receipts">到货管理</el-menu-item>
        </el-sub-menu>
        <el-sub-menu index="warehouse">
          <template #title><el-icon><Box /></el-icon><span>仓库管理</span></template>
          <el-menu-item index="/warehouse/inventory">库存查询</el-menu-item>
          <el-menu-item index="/warehouse/stock">出入库操作</el-menu-item>
          <el-menu-item index="/warehouse/ledger">库存流水</el-menu-item>
          <el-menu-item index="/warehouse/warnings">库存预警</el-menu-item>
        </el-sub-menu>
        <el-sub-menu index="schedule">
          <template #title><el-icon><Calendar /></el-icon><span>排班管理</span></template>
          <el-menu-item index="/schedule/shifts">班次设置</el-menu-item>
          <el-menu-item index="/schedule/work">排班计划</el-menu-item>
        </el-sub-menu>
        <el-sub-menu index="production">
          <template #title><el-icon><SetUp /></el-icon><span>生产管理</span></template>
          <el-menu-item index="/production/plans">生产计划</el-menu-item>
          <el-menu-item index="/production/orders">冲压产量单</el-menu-item>
          <el-menu-item index="/production/daily">生产日报</el-menu-item>
        </el-sub-menu>
        <el-sub-menu index="quality">
          <template #title><el-icon><CircleCheck /></el-icon><span>质量管理</span></template>
          <el-menu-item index="/quality/inspections">质检记录</el-menu-item>
          <el-menu-item index="/quality/tools">计量器具台账</el-menu-item>
          <el-menu-item index="/quality/toolapply">量具申购</el-menu-item>
          <el-menu-item index="/quality/toolscrap">器具报废</el-menu-item>
          <el-menu-item index="/quality/calibration">检定处理</el-menu-item>
        </el-sub-menu>
        <el-sub-menu index="equipment">
          <template #title><el-icon><Cpu /></el-icon><span>设备管理</span></template>
          <el-menu-item index="/equipment/list">设备台账</el-menu-item>
          <el-menu-item index="/equipment/maintenance">维护记录</el-menu-item>
        </el-sub-menu>
        <el-sub-menu index="mold">
          <template #title><el-icon><Tools /></el-icon><span>模具/工装夹具管理</span></template>
          <el-menu-item index="/mold/list">模具台账</el-menu-item>
          <el-menu-item index="/mold/plans">模具制造计划</el-menu-item>
        </el-sub-menu>
        <el-sub-menu index="base">
          <template #title><el-icon><DataBase /></el-icon><span>基础数据</span></template>
          <el-menu-item index="/base/customers">客户管理</el-menu-item>
          <el-menu-item index="/base/suppliers">供应商管理</el-menu-item>
          <el-menu-item index="/base/materials">原材料管理</el-menu-item>
          <el-menu-item index="/base/products">产品管理</el-menu-item>
          <el-menu-item index="/base/warehouses">仓库管理</el-menu-item>
          <el-menu-item index="/base/employees">员工管理</el-menu-item>
        </el-sub-menu>
        <el-menu-item v-if="auth.role === 'admin'" index="/system/users">
          <el-icon><User /></el-icon><span>系统管理</span>
        </el-menu-item>
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
      <el-main class="main">
        <router-view />
      </el-main>
    </el-container>
  </el-container>
</template>

<script setup>
import { computed } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'

const auth = useAuthStore()
const router = useRouter()
const roleName = computed(() => ({
  admin: '系统管理员', production: '生产', warehouse: '仓库', quality: '质量', sales: '销售'
}[auth.role] || auth.role))

function handleCommand(cmd) {
  if (cmd === 'logout') {
    auth.logout()
    router.push('/login')
  }
}
</script>

<style scoped>
.layout { height: 100%; }
.aside { background: #001529; overflow-y: auto; }
.aside::-webkit-scrollbar { width: 4px; }
.logo { display: flex; align-items: center; gap: 8px; color: #fff; font-size: 18px; font-weight: bold; height: 60px; padding: 0 18px; }
.menu { border-right: none; }
.header { background: #fff; display: flex; align-items: center; justify-content: space-between; box-shadow: 0 1px 4px rgba(0,21,41,.08); height: 60px; }
.user { display: flex; align-items: center; gap: 10px; }
.user-name { display: flex; align-items: center; gap: 6px; cursor: pointer; font-size: 14px; }
.main { padding: 16px; overflow-y: auto; }
</style>
