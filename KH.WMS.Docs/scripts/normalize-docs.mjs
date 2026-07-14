import { access, readFile, readdir, writeFile } from 'node:fs/promises';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import { apiFiles, architectureFiles, backendTutorialFiles, conceptGroups, frontendFiles, referenceFiles } from '../content-map.mjs';

const siteRoot = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '..');
const workspaceRoot = path.resolve(siteRoot, '..');
const docsRoot = path.join(workspaceRoot, 'docs');
const reviewed = '2026-07-14';
const requestedGroup = process.argv.find((argument) => argument.startsWith('--group='))?.split('=')[1] ?? 'all';

const archived = new Map([
  ['backend/KH.WMS前端开发指引 V1.0.md', '/backend/KH.WMS前端开发指引%20V3.0'],
  ['backend/KH.WMS后端开发指引 V1.0.md', '/backend/后端开发指引V3教程/README'],
  ['backend/KH.WMS项目技术栈与目录指引 V2.0.md', '/backend/架构设计/KH.WMS架构总览'],
  ['backend/KH.WMS后端开发指引V2.0培训考题.md', '/backend/后端开发指引V3教程/README'],
  ['backend/KH.WMS第一次培训考题 -.md', '/backend/KH.WMS第一次培训考题']
]);

const titleOverrides = new Map([
  ['backend/KH.WMS前端开发指引 V1.0.md', 'KH.WMS 前端开发指引 V1.0（历史）'],
  ['backend/KH.WMS后端开发指引 V1.0.md', 'KH.WMS 后端开发指引 V1.0（历史）'],
  ['backend/KH.WMS项目技术栈与目录指引 V2.0.md', 'KH.WMS 项目技术栈与目录指引 V2.0（历史）'],
  ['backend/KH.WMS后端开发指引V2.0培训考题.md', 'KH.WMS 后端开发指引 V2.0 培训考题（历史）'],
  ['backend/KH.WMS第一次培训考题 -.md', 'KH.WMS 第一次培训考题（历史副本）']
]);

const training = new Set([
  'backend/KH.WMS 阶段培训计划与文档.md',
  'backend/KH.WMS第一次培训考题.md',
  'backend/培训PPT/新版WMS前端页面开发培训_题库100题.md'
]);

const referenceNames = [
  'API', '参考文档', '外部调用', '全局配置', '物料属性', '部署', '联调', '排错', '测试',
  '检查清单', '常用命令', '常见坑', '业务流程', 'Contract', '配置驱动'
];

const relatedByArea = {
  architecture: [
    ['架构总览', '/backend/架构设计/KH.WMS架构总览'],
    ['学习路径', '/learning-path'],
    ['后端整体地图', '/backend/后端开发指引V3教程/01-KH.WMS后端整体地图']
  ],
  frontend: [
    ['前端开发指引 V3.0', '/backend/KH.WMS前端开发指引%20V3.0'],
    ['前端架构设计思路', '/backend/架构设计/KH.WMS前端架构设计思路'],
    ['前后端联调与接口契约', '/backend/KH.WMS前后端联调与接口契约指引']
  ],
  tutorial: [
    ['后端 V3 教程目录', '/backend/后端开发指引V3教程/README'],
    ['后端架构设计思路', '/backend/架构设计/KH.WMS后端架构设计思路'],
    ['底层机制索引', '/backend/后端底层概念/README']
  ],
  concepts: [
    ['底层机制索引', '/backend/后端底层概念/README'],
    ['后端 V3 教程', '/backend/后端开发指引V3教程/README'],
    ['后端排错与日志追踪', '/backend/KH.WMS后端排错与日志追踪指引']
  ],
  api: [
    ['API 参考首页', '/api/README'],
    ['公开类型索引', '/api/PUBLIC-TYPE-INDEX'],
    ['跨模块 Contract', '/backend/KH.WMS后端Contract与模块协作指引']
  ],
  resources: [
    ['学习路径', '/learning-path'],
    ['培训资料下载', '/training-materials'],
    ['架构总览', '/backend/架构设计/KH.WMS架构总览']
  ]
};

const toPosix = (value) => value.split(path.sep).join('/');

async function walk(directory) {
  const entries = await readdir(directory, { withFileTypes: true });
  const files = [];
  for (const entry of entries) {
    const fullPath = path.join(directory, entry.name);
    if (entry.isDirectory()) files.push(...await walk(fullPath));
    else if (entry.isFile() && entry.name.endsWith('.md')) files.push(fullPath);
  }
  return files;
}

function areaOf(relative) {
  if (relative.startsWith('api/')) return 'api';
  if (relative.includes('/架构设计/')) return 'architecture';
  if (relative.includes('/后端底层概念/')) return 'concepts';
  if (relative.includes('/后端开发指引V3教程/')) return 'tutorial';
  if (relative.includes('前端')) return 'frontend';
  return 'resources';
}

function groupOf(relative) {
  if ([...architectureFiles, ...frontendFiles, ...backendTutorialFiles].includes(relative)) return 'core';
  if ([...apiFiles, ...conceptGroups.flatMap((group) => group.files), ...referenceFiles.slice(0, 4)].includes(relative)) return 'mechanisms';
  return 'resources';
}

function statusOf(relative) {
  if (archived.has(relative)) return 'archived';
  if (training.has(relative)) return 'training';
  if (referenceNames.some((fragment) => relative.includes(fragment)) || ['api', 'concepts'].includes(areaOf(relative))) return 'reference';
  return 'current';
}

function audienceOf(area, status) {
  if (status === 'archived') return '维护历史版本或执行迁移的开发人员';
  if (status === 'training') return '新成员、培训讲师与参与考核的开发人员';
  return {
    architecture: '新成员、技术负责人和模块维护者',
    frontend: '前端开发人员与联调负责人',
    tutorial: '后端开发人员与代码评审人员',
    concepts: '后端开发人员、排障人员与底座维护者',
    api: '接口调用方、扩展开发人员与模块维护者',
    resources: '参与 KH.WMS 开发、测试与运维的团队成员'
  }[area];
}

function sourcePathsOf(area, status) {
  if (status === 'archived' || status === 'training') return [];
  if (area === 'frontend') return ['KH.WMS.Client/src'];
  if (area === 'architecture') return ['KH.WMS.Client/src', 'KH.WMS/KH.WMS.Server'];
  if (area === 'api') return ['KH.WMS'];
  return ['KH.WMS/KH.WMS.Server', 'KH.WMS/KH.WMS.Core', 'KH.WMS/Modules'];
}

function stripFrontmatter(source) {
  const normalized = source.replace(/^\uFEFF/, '').replace(/\r\n/g, '\n');
  if (!normalized.startsWith('---\n')) return normalized;
  const end = normalized.indexOf('\n---\n', 4);
  return end === -1 ? normalized : normalized.slice(end + 5);
}

function titleFrom(source, relative) {
  let inFence = false;
  for (const line of source.split('\n')) {
    if (/^\s*(```|~~~)/.test(line)) inFence = !inFence;
    if (!inFence) {
      const match = line.match(/^#\s+(.+)$/);
      if (match) return match[1].replace(/`/g, '').trim();
    }
  }
  return path.basename(relative, '.md').replace(/^\d+-/, '').replace(/^README$/i, '文档索引');
}

function normalizeHeadings(source, title, relative) {
  const lines = source.split('\n');
  let inFence = false;
  let h1Seen = false;
  const normalized = [];
  for (let line of lines) {
    if (/^\s*(```|~~~)/.test(line)) {
      inFence = !inFence;
      normalized.push(line);
      continue;
    }
    if (!inFence && /^#\s+/.test(line)) {
      if (!h1Seen) h1Seen = true;
      else {
        const repeatedTitle = line.slice(2).replace(/`/g, '').trim();
        if (relative.includes('/后端开发指引V3教程/') && repeatedTitle === title.replace(/\s+教程$/, '')) continue;
        line = `## ${line.slice(2)}`;
      }
    }
    if (!inFence && relative.includes('/后端开发指引V3教程/')) {
      if (/^##\s+原章节内容\s*$/.test(line)) continue;
      const levelTwoTitle = line.match(/^##\s+(.+)$/)?.[1]?.replace(/`/g, '').trim();
      if (levelTwoTitle === title.replace(/\s+教程$/, '')) continue;
    }
    normalized.push(line);
  }
  if (!h1Seen) {
    while (normalized[0] === '') normalized.shift();
    normalized.unshift(`# ${title}`, '');
  }
  return normalized.join('\n').replace(/\n{4,}/g, '\n\n\n').trimEnd();
}

function quote(value) {
  return JSON.stringify(value);
}

function frontmatter(metadata) {
  const lines = [
    '---',
    `title: ${quote(metadata.title)}`,
    `description: ${quote(metadata.description)}`,
    `status: ${metadata.status}`,
    `audience: ${quote(metadata.audience)}`,
    `reviewed: ${quote(reviewed)}`
  ];
  if (metadata.sourcePaths.length) {
    lines.push('sourcePaths:');
    for (const sourcePath of metadata.sourcePaths) lines.push(`  - ${quote(sourcePath)}`);
  }
  if (metadata.status === 'archived') {
    lines.push('search: false', `replacement: ${quote(metadata.replacement)}`);
  }
  lines.push('---', '');
  return lines.join('\n');
}

function navigation(area, relative, replacement) {
  const links = replacement
    ? [['当前维护版本', replacement], ...relatedByArea[area].slice(0, 2)]
    : relatedByArea[area];
  const unique = [...new Map(links.filter(([, href]) => !decodeURI(href).endsWith(relative.replace(/\.md$/, ''))).map((item) => [item[1], item])).values()];
  return `\n\n## 继续阅读\n\n${unique.map(([label, href]) => `- [${label}](${href})`).join('\n')}`;
}

async function assertSources(paths) {
  for (const sourcePath of paths) await access(path.join(workspaceRoot, sourcePath));
}

const files = (await Promise.all(['backend', 'api'].map((root) => walk(path.join(docsRoot, root))))).flat();
let changed = 0;
for (const file of files) {
  const relative = toPosix(path.relative(docsRoot, file));
  if (requestedGroup !== 'all' && groupOf(relative) !== requestedGroup) continue;
  const original = await readFile(file, 'utf8');
  const bodyWithoutFrontmatter = stripFrontmatter(original);
  const title = titleOverrides.get(relative) ?? titleFrom(bodyWithoutFrontmatter, relative);
  const area = areaOf(relative);
  const status = statusOf(relative);
  const sourcePaths = sourcePathsOf(area, status);
  await assertSources(sourcePaths);
  let body = normalizeHeadings(bodyWithoutFrontmatter, title, relative);
  body = body.replace(/\n## 继续阅读\n[\s\S]*$/, '');
  body += navigation(area, relative, archived.get(relative));
  const description = `${title}：说明适用场景、当前实现、设计边界与开发或排障入口。`;
  const output = `${frontmatter({
    title,
    description,
    status,
    audience: audienceOf(area, status),
    sourcePaths,
    replacement: archived.get(relative)
  })}\n${body}\n`;
  if (output !== original.replace(/\r\n/g, '\n')) {
    await writeFile(file, output, 'utf8');
    changed += 1;
  }
}

console.log(`Normalized ${changed} document(s) in group ${requestedGroup}.`);
