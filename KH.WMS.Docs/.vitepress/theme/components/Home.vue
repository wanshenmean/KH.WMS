<script setup lang="ts">
import { computed, inject } from 'vue';
import { useData, withBase } from 'vitepress';
import { VPNavBarSearch } from 'vitepress/theme';

const { site, isDark } = useData();
const toggleAppearance = inject<() => void>('toggle-appearance', () => {
  isDark.value = !isDark.value;
});
const handleAppearanceKeydown = (event: KeyboardEvent) => {
  if (event.key === 'Enter' || event.key === ' ') {
    event.preventDefault();
    toggleAppearance();
  }
};
const appearanceLabel = computed(() => isDark.value ? '切换到浅色模式' : '切换到深色模式');
const routes = computed(() => [
  { title: '前端实战', description: '从启动、路由、权限到组件、状态、请求与 E2E。', href: withBase('/backend/KH.WMS前端开发指引%20V3.0'), icon: '⌘' },
  { title: '后端主线', description: '用完整 CRUD、Contract、事务与校验串起真实业务开发。', href: withBase('/backend/后端开发指引V3教程/01-KH.WMS后端整体地图'), icon: '↗' },
  { title: '底层概念', description: '理解启动、依赖注入、AOP、运行时配置和可观测性。', href: withBase('/backend/后端底层概念/01-启动入口与程序集扫描'), icon: '◌' }
]);
</script>

<template>
  <div class="academy-home">
    <header class="academy-header">
      <a class="academy-brand" :href="withBase('/')" aria-label="KH.WMS 开发学院首页">
        <img :src="withBase('/mark.svg')" alt="" width="28" height="28" />
        <span>{{ site.title }}</span>
      </a>
      <nav aria-label="主导航">
        <a :href="withBase('/learning-path')">学习路径</a>
        <a :href="withBase('/backend/KH.WMS前端开发指引%20V3.0')">前端开发</a>
        <a :href="withBase('/backend/后端开发指引V3教程/01-KH.WMS后端整体地图')">后端开发</a>
        <a :href="withBase('/backend/后端底层概念/01-启动入口与程序集扫描')">底层概念</a>
      </nav>
      <div class="academy-tools">
        <div class="academy-search"><VPNavBarSearch /></div>
        <button
          class="academy-appearance"
          type="button"
          :aria-label="appearanceLabel"
          :title="appearanceLabel"
          :aria-pressed="isDark"
          @click="toggleAppearance"
          @keydown="handleAppearanceKeydown"
        >
          <span class="vpi-sun theme-icon theme-sun" aria-hidden="true"></span>
          <span class="vpi-moon theme-icon theme-moon" aria-hidden="true"></span>
        </button>
      </div>
    </header>

    <main>
      <section class="home-hero">
        <div class="home-hero-copy">
          <h1>把 WMS 开发讲透</h1>
          <p>从页面开发到服务链路，再到底层运行机制，一站式掌握 KH.WMS。</p>
          <div class="home-actions">
            <a class="button button-primary" :href="withBase('/learning-path')">开始学习</a>
            <a class="button button-secondary" :href="withBase('/backend/后端开发指引V3教程/01-KH.WMS后端整体地图')">查看学习地图</a>
          </div>
        </div>
        <div class="flow-illustration" aria-label="从前端界面到业务服务与数据存储的系统流程图">
          <div class="flow-node node-ui">页面与交互</div>
          <span class="flow-line line-one"></span>
          <div class="flow-node node-api">API 契约</div>
          <span class="flow-line line-two"></span>
          <div class="flow-node node-service">业务服务</div>
          <span class="flow-line line-three"></span>
          <div class="flow-node node-data">数据与设备</div>
        </div>
      </section>

      <section class="route-section" aria-labelledby="route-title">
        <div class="section-heading">
          <h2 id="route-title">按你的工作进入文档</h2>
          <p>不必从头读完。先选当前任务，再沿着相关教程深入。</p>
        </div>
        <div class="route-list">
          <a v-for="route in routes" :key="route.title" class="route-row" :href="route.href">
            <span class="route-icon" aria-hidden="true">{{ route.icon }}</span>
            <span><strong>{{ route.title }}</strong><small>{{ route.description }}</small></span>
            <svg viewBox="0 0 20 20" aria-hidden="true"><path d="M4 10h11M11 5l5 5-5 5" /></svg>
          </a>
        </div>
      </section>

      <section class="path-section" aria-labelledby="newcomer-title">
        <div>
          <h2 id="newcomer-title">新成员推荐路径</h2>
          <p>用一条可执行的学习顺序，先建立系统地图，再完成第一个功能。</p>
        </div>
        <ol>
          <li><span>01</span><a :href="withBase('/backend/KH.WMS前端配置与启动指引')">启动前端与认识项目结构</a></li>
          <li><span>02</span><a :href="withBase('/backend/后端开发指引V3教程/01-KH.WMS后端整体地图')">理解后端模块与职责边界</a></li>
          <li><span>03</span><a :href="withBase('/backend/后端底层概念/03-依赖注入自动注册与AOP代理')">追踪请求链路与底层机制</a></li>
        </ol>
        <a class="inline-link" :href="withBase('/learning-path')">打开完整学习路径 <span>→</span></a>
      </section>

      <section class="resource-strip" aria-label="补充学习资源">
        <a :href="withBase('/api/README')"><strong>API 参考</strong><span>查阅公开类型与模块接口</span></a>
        <a :href="withBase('/training-materials')"><strong>培训资料</strong><span>下载原始 PPTX 与 DOCX</span></a>
      </section>
    </main>

    <footer>KH.WMS 内部技术文档</footer>
  </div>
</template>
