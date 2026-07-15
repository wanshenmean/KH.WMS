import { defineConfig } from "vitepress";
import { readFile } from "node:fs/promises";
import path from "node:path";
import { withMermaid } from "vitepress-plugin-mermaid";
import {
  apiFiles,
  architectureFiles,
  archivedFiles,
  backendTutorialFiles,
  conceptGroups,
  frontendFiles,
  referenceFiles,
  trainingFiles,
} from "../content-map.mjs";

const siteRoot = path.resolve(import.meta.dirname, "..");
const sourceRoot = path.join(siteRoot, ".site-content");

const toUrl = (relativePath: string) =>
  `/${relativePath.replace(/\\/g, "/").replace(/\.md$/, "").split("/").map(encodeURIComponent).join("/")}`;

async function pageTitle(relative: string) {
  const source = await readFile(path.join(sourceRoot, relative), "utf8");
  const frontmatterTitle = source.match(
    /^---\n[\s\S]*?^title:\s+"([\s\S]*?)"\s*$/m,
  )?.[1];
  return (
    frontmatterTitle ??
    source.match(/^#\s+(.+)$/m)?.[1]?.replace(/`/g, "") ??
    path.basename(relative, ".md")
  );
}

async function items(
  files: string[],
  formatTitle: (title: string) => string = (title) => title,
) {
  return Promise.all(
    files.map(async (relative) => {
      const title = await pageTitle(relative);
      return {
        text: formatTitle(title),
        link: toUrl(relative),
      };
    }),
  );
}

const backendSidebarFiles = backendTutorialFiles.filter(
  (relative) => !relative.endsWith("/README.md"),
);

const backendSidebarTitle = (title: string) =>
  title
    .replace(/^第\s*\d+\s*章\s*/, "")
    .replace(/^附录\s*[A-Z]\s*/, "")
    .replace(/\s*教程$/, "");

const architectureItems = await items(architectureFiles);
const frontendItems = await items(frontendFiles);
const backendTutorialItems = await items(
  backendSidebarFiles,
  backendSidebarTitle,
);
const apiItems = await items(apiFiles);
const referenceItems = await items(referenceFiles.slice(0, 4));
const engineeringItems = await items(referenceFiles.slice(4, 11));
const domainItems = await items(referenceFiles.slice(11));
const trainingItems = await items(trainingFiles);
const archivedItems = await items(archivedFiles);
const conceptItems = await Promise.all(
  conceptGroups.map(async (group) => ({
    text: group.text,
    collapsed: true,
    items: await items(group.files),
  })),
);

export default withMermaid(
  defineConfig({
    title: "KH.WMS 开发学院",
    description: "KH.WMS 架构、前端、后端、底层机制与 API 技术文档。",
    head: [["link", { rel: "icon", href: "/mark.svg", type: "image/svg+xml" }]],
    srcDir: ".site-content",
    cleanUrls: true,
    lastUpdated: true,
    ignoreDeadLinks: [/^\/downloads\//],
    markdown: { html: false },
    mermaid: {
      securityLevel: "strict",
      theme: "base",
      fontFamily: "Inter, Microsoft YaHei, sans-serif",
      themeVariables: {
        primaryColor: "#eaf4ff",
        primaryTextColor: "#10233e",
        primaryBorderColor: "#7ab5e7",
        lineColor: "#6488aa",
        secondaryColor: "#e9f8f5",
        tertiaryColor: "#f7fafc",
        noteBkgColor: "#fff8db",
        noteTextColor: "#43556b",
      },
    },
    themeConfig: {
      logo: "/mark.svg",
      nav: [
        { text: "架构总览", items: architectureItems },
        { text: "学习路径", link: "/learning-path" },
        {
          text: "开发文档",
          items: [
            { text: "前端开发", link: toUrl(frontendFiles[0]) },
            { text: "后端开发", link: toUrl(backendTutorialFiles[0]) },
            { text: "底层机制", link: toUrl(conceptGroups[0].files[0]) },
          ],
        },
        { text: "API / 参考", link: toUrl(apiFiles[0]) },
        {
          text: "培训与历史",
          items: [
            { text: "培训资料下载", link: "/training-materials" },
            { text: "阶段培训计划", link: toUrl(trainingFiles[0]) },
            { text: "历史版本：前端 V1.0", link: toUrl(archivedFiles[0]) },
            { text: "历史版本：后端 V1.0", link: toUrl(archivedFiles[1]) },
          ],
        },
      ],
      sidebar: {
        "/backend/": [
          { text: "架构总览", collapsed: false, items: architectureItems },
          { text: "前端开发", collapsed: true, items: frontendItems },
          { text: "后端开发", collapsed: true, items: backendTutorialItems },
          { text: "底层机制", collapsed: true, items: conceptItems },
          { text: "API 与扩展参考", collapsed: true, items: referenceItems },
          { text: "工程实践", collapsed: true, items: engineeringItems },
          { text: "业务与配置说明", collapsed: true, items: domainItems },
          {
            text: "培训与考核",
            collapsed: true,
            items: [
              ...trainingItems,
              { text: "PPT / Word 下载", link: "/training-materials" },
            ],
          },
          { text: "历史归档", collapsed: true, items: archivedItems },
        ],
        "/api/": [
          { text: "API / 参考", collapsed: false, items: apiItems },
          {
            text: "关联入口",
            items: [
              { text: "架构总览", link: toUrl(architectureFiles[0]) },
              { text: "跨模块 Contract", link: toUrl(referenceFiles[2]) },
              { text: "学习路径", link: "/learning-path" },
            ],
          },
        ],
      },
      outline: { level: [2, 3], label: "本页导航" },
      docFooter: { prev: "上一篇", next: "下一篇" },
      editLink: { pattern: "", text: "" },
      footer: {
        message: "KH.WMS 内部技术文档",
        copyright: "仅限授权团队成员访问",
      },
      search: {
        provider: "local",
        options: {
          locales: {
            root: {
              translations: {
                button: { buttonText: "搜索文档", buttonAriaLabel: "搜索文档" },
                modal: {
                  noResultsText: "没有找到相关内容",
                  resetButtonTitle: "重置搜索",
                  backButtonTitle: "关闭搜索",
                  footer: {
                    selectText: "选择",
                    selectKeyAriaLabel: "回车",
                    navigateText: "切换",
                    navigateUpKeyAriaLabel: "上箭头",
                    navigateDownKeyAriaLabel: "下箭头",
                    closeText: "关闭",
                    closeKeyAriaLabel: "Esc",
                  },
                },
              },
            },
          },
        },
      },
      socialLinks: [],
    },
  }),
);
