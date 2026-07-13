<template>
  <div
    v-loading="loading"
    :element-loading-text="text"
    :element-loading-background="background"
    :element-loading-fullscreen="fullscreen"
    class="kh-loading"
  >
    <slot />
  </div>
</template>
<script setup>
defineProps({
  loading: { type: Boolean, default: false },
  text: { type: String, default: '' },
  fullscreen: { type: Boolean, default: false },
  background: { type: String, default: 'rgba(255,255,255,0.8)' },
})
</script>
<style scoped>
.kh-loading {
  min-height: 100px;
  position: relative;
}
</style>
