<script setup lang="ts">
import { computed } from 'vue';
import { useData, withBase } from 'vitepress';

const { frontmatter } = useData();
const labels: Record<string, string> = {
  current: '当前文档',
  reference: '参考资料',
  training: '培训材料',
  archived: '历史归档'
};
const status = computed(() => frontmatter.value.status as string | undefined);
const replacement = computed(() => frontmatter.value.replacement as string | undefined);
</script>

<template>
  <aside v-if="status" class="doc-meta" :class="`doc-meta-${status}`" :aria-label="labels[status]">
    <div class="doc-meta-heading">
      <span class="doc-meta-status">{{ labels[status] }}</span>
      <span v-if="frontmatter.reviewed">复核于 {{ frontmatter.reviewed }}</span>
    </div>
    <p v-if="status === 'archived'">
      本页保留历史版本全文，不代表当前实现。请优先阅读
      <a v-if="replacement" :href="withBase(replacement)">当前维护文档</a>。
    </p>
    <p v-else-if="frontmatter.audience">适用读者：{{ frontmatter.audience }}</p>
  </aside>
</template>
