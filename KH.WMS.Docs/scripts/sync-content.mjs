import { cp, mkdir, readdir, readFile, rm, stat, writeFile } from 'node:fs/promises';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const siteRoot = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '..');
const workspaceRoot = path.resolve(siteRoot, '..');
const outputRoot = path.join(siteRoot, '.site-content');
const sourceRoots = ['backend', 'api'];
const downloadExtensions = new Set(['.pptx', '.docx']);
// Sites source uploads have a conservative request-size limit. The large
// template copies are not linked from the tutorials, so keep the lightweight
// courseware and Word handouts in the deployed download centre.
const maxDownloadBytes = 2 * 1024 * 1024;

const toPosix = (value) => value.split(path.sep).join('/');
const escapeMarkdown = (value) => value.replace(/([\\`*_{}\[\]<>])/g, '\\$1');

async function walk(directory) {
  const entries = await readdir(directory, { withFileTypes: true });
  const files = [];
  for (const entry of entries) {
    const fullPath = path.join(directory, entry.name);
    if (entry.isDirectory()) files.push(...await walk(fullPath));
    else if (entry.isFile()) files.push(fullPath);
  }
  return files;
}

function rewriteDownloadLinks(markdown, sourceFile) {
  return markdown.replace(/\]\(([^)\s]+)(\s+"[^"]*")?\)/g, (match, rawTarget, title = '') => {
    if (/^(?:https?:|mailto:|#|\/)/i.test(rawTarget)) return match;
    const [targetPath, hash = ''] = rawTarget.split('#', 2);
    const extension = path.extname(targetPath).toLowerCase();
    if (!downloadExtensions.has(extension)) return match;
    const resolved = path.resolve(path.dirname(sourceFile), targetPath);
    const relativeToDocs = path.relative(path.join(workspaceRoot, 'docs'), resolved);
    if (relativeToDocs.startsWith('..') || path.isAbsolute(relativeToDocs)) return match;
    return `](/downloads/${encodeURI(toPosix(relativeToDocs))}${hash ? `#${hash}` : ''}${title})`;
  });
}

function createLearningPath() {
  return `---\nlayout: doc\ntitle: 学习路径\ndescription: 为 KH.WMS 开发者设计的推荐学习顺序。\n---\n\n# 学习路径\n\n按角色选择一条路径，先建立系统地图，再进入具体业务场景。\n\n## 新成员：建立整体认知\n\n1. 先读[KH.WMS 架构总览](/backend/架构设计/KH.WMS架构总览)，理解前后端边界、设计原因和完整请求链路。\n2. 从[前端配置与启动](/backend/KH.WMS前端配置与启动指引)了解本地运行方式。\n3. 阅读[KH.WMS 后端整体地图](/backend/后端开发指引V3教程/01-KH.WMS后端整体地图)，理解模块边界。\n4. 继续阅读[启动入口与程序集扫描](/backend/后端底层概念/01-启动入口与程序集扫描)，串起运行时机制。\n\n## 前端开发：从页面到联调\n\n依次学习路由菜单、组件体系、状态管理、请求封装与 E2E 质量检查；完成页面后回到[前后端联调与接口契约](/backend/KH.WMS前后端联调与接口契约指引)。\n\n## 后端开发：从标准 CRUD 到业务流程\n\n先完成 V3 教程第 1–8 章，再学习跨模块 Contract、事务与校验扩展；遇到运行时问题时按底层概念专题检索。\n\n## 深入排障：按症状定位\n\n接口契约、鉴权缓存、性能观测、限流、后台服务与登录安全均已拆分为可独立查阅的专题。\n`;
}

function createHome() {
  return `---\nlayout: home\ntitle: KH.WMS 开发学院\ndescription: KH.WMS 前端、后端与底层概念技术文档。\n---\n`;
}

function createTrainingMaterials(downloads) {
  const grouped = new Map();
  for (const download of downloads) {
    const group = path.dirname(download.relative).split(path.sep).slice(1).join(' / ') || '其他资料';
    if (!grouped.has(group)) grouped.set(group, []);
    grouped.get(group).push(download);
  }

  const sections = [...grouped.entries()].sort(([left], [right]) => left.localeCompare(right, 'zh-CN')).map(([group, files]) => {
    const links = files.sort((left, right) => left.relative.localeCompare(right.relative, 'zh-CN')).map((file) =>
      `- [${escapeMarkdown(path.basename(file.relative))}](/downloads/${encodeURI(toPosix(file.relative))})`
    ).join('\n');
    return `## ${group}\n\n${links}`;
  });

  return `---\nlayout: doc\ntitle: 培训资料下载\ndescription: KH.WMS 培训课件与原始 Word 资料。\n---\n\n# 培训资料下载\n\n以下资料保持原始 PPTX/DOCX 格式，适用于线下培训、复盘和补充阅读。\n\n${sections.join('\n\n')}\n`;
}

async function main() {
  await rm(outputRoot, { recursive: true, force: true });
  await mkdir(path.join(outputRoot, 'public', 'downloads'), { recursive: true });
  await cp(path.join(siteRoot, '.vitepress', 'public', 'mark.svg'), path.join(outputRoot, 'public', 'mark.svg'));

  const downloads = [];
  let markdownCount = 0;

  for (const rootName of sourceRoots) {
    const sourceRoot = path.join(workspaceRoot, 'docs', rootName);
    const files = await walk(sourceRoot);
    for (const sourceFile of files) {
      const relative = path.relative(path.join(workspaceRoot, 'docs'), sourceFile);
      const extension = path.extname(sourceFile).toLowerCase();
      if (extension === '.md') {
        const destination = path.join(outputRoot, relative);
        await mkdir(path.dirname(destination), { recursive: true });
        const markdown = await readFile(sourceFile, 'utf8');
        await writeFile(destination, rewriteDownloadLinks(markdown, sourceFile), 'utf8');
        markdownCount += 1;
      } else if (rootName === 'backend' && downloadExtensions.has(extension)) {
        if ((await stat(sourceFile)).size > maxDownloadBytes) continue;
        const destination = path.join(outputRoot, 'public', 'downloads', relative);
        await mkdir(path.dirname(destination), { recursive: true });
        await cp(sourceFile, destination);
        downloads.push({ relative });
      }
    }
  }

  // Existing READMEs are the directory landing pages in the source tree. Add
  // an index alias so relative links ending in a directory keep working after
  // VitePress turns Markdown files into clean URLs.
  const generatedMarkdown = (await walk(outputRoot)).filter((file) => path.extname(file).toLowerCase() === '.md');
  for (const readme of generatedMarkdown.filter((file) => path.basename(file).toLowerCase() === 'readme.md')) {
    const indexFile = path.join(path.dirname(readme), 'index.md');
    try {
      await stat(indexFile);
    } catch {
      await cp(readme, indexFile);
    }
  }

  await writeFile(path.join(outputRoot, 'index.md'), createHome(), 'utf8');
  await writeFile(path.join(outputRoot, 'learning-path.md'), createLearningPath(), 'utf8');
  await writeFile(path.join(outputRoot, 'training-materials.md'), createTrainingMaterials(downloads), 'utf8');
  await writeFile(path.join(outputRoot, 'backend', 'KH.WMS前端开发指引.md'), `---\ntitle: KH.WMS 前端开发指引\n---\n\n# KH.WMS 前端开发指引\n\n本入口保留给旧专题文档的相对链接。请阅读当前维护版本：[KH.WMS 前端开发指引 V3.0](./KH.WMS前端开发指引%20V3.0.md)。\n`, 'utf8');
  await writeFile(path.join(outputRoot, '.content-manifest.json'), JSON.stringify({ markdownCount, downloadCount: downloads.length }, null, 2), 'utf8');

  console.log(`Synced ${markdownCount} Markdown files and ${downloads.length} downloadable files.`);
}

main().catch((error) => {
  console.error(error);
  process.exitCode = 1;
});
