<template>
  <div class="placeholder-page">
    <el-empty description="页面开发中...">
      <template #image>
        <el-icon :size="64" color="#c0c4cc"><Tools /></el-icon>
      </template>
    </el-empty>
  </div>
</template>

<script setup>
</script>

<style scoped>
.placeholder-page {
  display: flex;
  align-items: center;
  justify-content: center;
  height: 100%;
  min-height: 400px;
}
</style>
