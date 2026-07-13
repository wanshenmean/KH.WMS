import { defineConfig } from 'vitepress';
import { readdir, readFile } from 'node:fs/promises';
import { existsSync } from 'node:fs';
import path from 'node:path';

const siteRoot = path.resolve(import.meta.dirname, '..');
const sourceRoot = path.join(siteRoot, '.site-content');

const toUrl = (relativePath: string) => `/${relativePath.replace(/\\/g, '/').replace(/\.md$/, '').split('/').map(encodeURIComponent).join('/')}`;

async function walkMarkdown(directory: string): Promise<string[]> {
  const entries = await readdir(directory, { withFileTypes: true });
  const files: string[] = [];
  for (const entry of entries) {
    const fullPath = path.join(directory, entry.name);
    if (entry.isDirectory()) files.push(...await walkMarkdown(fullPath));
    else if (entry.isFile() && entry.name.endsWith('.md')) files.push(fullPath);
  }
  return files;
}

async function pageTitle(file: string) {
  const source = await readFile(file, 'utf8');
  return source.match(/^#\s+(.+)$/m)?.[1]?.replace(/`/g, '') ?? path.basename(file, '.md');
}

async function createItems(relativeDirectory: string, exclude = new Set<string>()) {
  const directory = path.join(sourceRoot, relativeDirectory);
  if (!existsSync(directory)) return [];
  const files = await walkMarkdown(directory);
  return Promise.all(files
    .map((file) => path.relative(sourceRoot, file))
    .filter((relative) => !exclude.has(relative.replace(/\\/g, '/')))
    .sort((left, right) => left.localeCompare(right, 'zh-CN', { numeric: true }))
    .map(async (relative) => ({ text: await pageTitle(path.join(sourceRoot, relative)), link: toUrl(relative) })));
}

const frontendFiles = [
  'backend/KH.WMS前端开发指引 V3.0.md',
  'backend/KH.WMS前端配置与启动指引.md',
  'backend/KH.WMS前端路由菜单与权限开发指引.md',
  'backend/KH.WMS前端请求封装与接口开发指引.md',
  'backend/KH.WMS前端组件体系与页面开发指引.md',
  'backend/KH.WMS前端状态管理与公共工具指引.md',
  'backend/KH.WMS前端常用组件详细使用文档.md',
  'backend/KH.WMS前端E2E测试与质量检查指引.md'
];
const architectureFiles = [
  'backend/架构设计/KH.WMS架构总览.md',
  'backend/架构设计/KH.WMS前端架构设计思路.md',
  'backend/架构设计/KH.WMS后端架构设计思路.md'
];
const backendTutorialDirectory = 'backend/后端开发指引V3教程';
const conceptDirectory = 'backend/后端底层概念';
const apiDirectory = 'api';
const primaryFiles = new Set([...architectureFiles, ...frontendFiles, ...await walkMarkdown(path.join(sourceRoot, backendTutorialDirectory)).then((files) => files.map((file) => path.relative(sourceRoot, file).replace(/\\/g, '/'))), ...await walkMarkdown(path.join(sourceRoot, conceptDirectory)).then((files) => files.map((file) => path.relative(sourceRoot, file).replace(/\\/g, '/')))]);
const architectureItems = await Promise.all(architectureFiles.map(async (relative) => ({
  text: await pageTitle(path.join(sourceRoot, relative)),
  link: toUrl(relative)
})));

export default defineConfig({
  title: 'KH.WMS 开发学院',
  description: 'KH.WMS 前端、后端与底层概念技术文档。',
  srcDir: '.site-content',
  cleanUrls: true,
  lastUpdated: true,
  // Static binary downloads are copied and checked by scripts/verify-content.
  // VitePress's route checker only resolves generated HTML pages.
  ignoreDeadLinks: [/^\/downloads\//],
  // Repository tutorials contain angle-bracket notation in prose and examples.
  // Render it as text rather than treating it as Vue template markup.
  markdown: { html: false },
  themeConfig: {
    logo: '/mark.svg',
    nav: [
      { text: '架构设计', items: architectureItems },
      { text: '学习路径', link: '/learning-path' },
      { text: '前端开发', link: toUrl(frontendFiles[0]) },
      { text: '后端开发', link: '/backend/后端开发指引V3教程/01-KH.WMS后端整体地图' },
      { text: '底层概念', link: '/backend/后端底层概念/01-启动入口与程序集扫描' },
      { text: 'API 参考', link: '/api/README' }
    ],
    sidebar: {
      '/backend/': [
        { text: '架构设计', collapsed: false, items: architectureItems },
        { text: '前端开发', collapsed: false, items: await Promise.all(frontendFiles.map(async (relative) => ({ text: await pageTitle(path.join(sourceRoot, relative)), link: toUrl(relative) }))) },
        { text: '后端 V3 教程', collapsed: false, items: await createItems(backendTutorialDirectory) },
        { text: '底层概念', collapsed: true, items: await createItems(conceptDirectory) },
        { text: '补充资料', collapsed: true, items: await createItems('backend', primaryFiles) },
        { text: '培训资料', items: [{ text: 'PPT / Word 下载', link: '/training-materials' }] }
      ],
      '/api/': [
        { text: '架构设计', collapsed: false, items: architectureItems },
        { text: 'API 参考', collapsed: false, items: await createItems(apiDirectory) },
        { text: '继续学习', items: [{ text: '学习路径', link: '/learning-path' }, { text: '培训资料下载', link: '/training-materials' }] }
      ]
    },
    outline: { level: [2, 3], label: '本页导航' },
    docFooter: { prev: '上一篇', next: '下一篇' },
    editLink: { pattern: '', text: '' },
    footer: { message: 'KH.WMS 内部技术文档', copyright: '仅限授权团队成员访问' },
    search: {
      provider: 'local',
      options: {
        locales: {
          root: {
            translations: {
              button: { buttonText: '搜索文档', buttonAriaLabel: '搜索文档' },
              modal: {
                noResultsText: '没有找到相关内容',
                resetButtonTitle: '重置搜索',
                backButtonTitle: '关闭搜索',
                footer: { selectText: '选择', selectKeyAriaLabel: '回车', navigateText: '切换', navigateUpKeyAriaLabel: '上箭头', navigateDownKeyAriaLabel: '下箭头', closeText: '关闭', closeKeyAriaLabel: 'Esc' }
              }
            }
          }
        }
      }
    },
    socialLinks: []
  }
});
