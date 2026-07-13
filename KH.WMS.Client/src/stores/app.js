
import { defineStore } from 'pinia'

export const useAppStore = defineStore('app', {
  state: () => ({
    isCollapse: false,
  }),
  actions: {
    toggleSidebar() {
      this.isCollapse = !this.isCollapse
    },
  },
})
