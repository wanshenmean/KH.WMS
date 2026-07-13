import { defineStore } from 'pinia'
import { getDictDataByType } from '@/api/system'

export const useDictStore = defineStore('dict', {
  state: () => ({
    /** 字典缓存 { dictType: [{ label, value, tagType }] } */
    cache: {},
    /** 正在加载的请求 { dictType: Promise } 用于去重 */
    loading: {},
  }),

  actions: {
    /**
     * 获取字典数据（带缓存和去重）
     * @param {string} dictType - 字典类型编码
     * @returns {Promise<Array<{label: string, value: *, tagType: string}>>}
     */
    async getDict(dictType) {
      if (this.cache[dictType]) return this.cache[dictType]
      if (this.loading[dictType]) return this.loading[dictType]

      const promise = getDictDataByType(dictType).then(res => {
        const list = (res.data || []).map(item => ({
          label: item.itemLabel,
          value: item.itemValue,
          tagType: item.tagColor || '',
        }))
        if (list.length > 0) {
          this.cache[dictType] = list
        }
        delete this.loading[dictType]
        return list
      }).catch(() => {
        delete this.loading[dictType]
        return []
      })

      this.loading[dictType] = promise
      return promise
    },

    /**
     * 刷新指定字典类型（字典管理页增删改后调用）
     * @param {string} dictType
     */
    async refreshDict(dictType) {
      delete this.cache[dictType]
      delete this.loading[dictType]
      return this.getDict(dictType)
    },

    /**
     * 清空所有缓存（登出时调用）
     */
    clearDict() {
      this.cache = {}
      this.loading = {}
    },
  },
})
