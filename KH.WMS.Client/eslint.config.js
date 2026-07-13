/* eslint-env node */
module.exports = {
  rules: {
    // Vue 3 编译器要求 <template v-for> 的 key 必须放在 <template> 上，
    // 与 eslint-plugin-vue 的 vue/no-v-for-template-key 规则冲突，此处关闭
    'vue/no-v-for-template-key': 'off',
  },
}
