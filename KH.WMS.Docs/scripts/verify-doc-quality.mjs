import { access, readFile, readdir } from 'node:fs/promises';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import { allMappedFiles, archivedFiles } from '../content-map.mjs';

const siteRoot = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '..');
const workspaceRoot = path.resolve(siteRoot, '..');
const docsRoot = path.join(workspaceRoot, 'docs');
const expectedCount = 78;
const validStatuses = new Set(['current', 'reference', 'training', 'archived']);
const failures = [];

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

function parseFrontmatter(source) {
  const normalized = source.replace(/^\uFEFF/, '').replace(/\r\n/g, '\n');
  if (!normalized.startsWith('---\n')) return { data: {}, body: normalized };
  const end = normalized.indexOf('\n---\n', 4);
  if (end === -1) return { data: {}, body: normalized };
  const data = {};
  let arrayKey = '';
  for (const line of normalized.slice(4, end).split('\n')) {
    const arrayItem = line.match(/^\s+-\s+(.+)$/);
    if (arrayItem && arrayKey) {
      data[arrayKey].push(JSON.parse(arrayItem[1]));
      continue;
    }
    const field = line.match(/^([A-Za-z][\w-]*):(?:\s+(.*))?$/);
    if (!field) continue;
    const [, key, raw = ''] = field;
    if (!raw) {
      data[key] = [];
      arrayKey = key;
    } else {
      arrayKey = '';
      if (raw === 'true' || raw === 'false') data[key] = raw === 'true';
      else if (raw.startsWith('"')) data[key] = JSON.parse(raw);
      else data[key] = raw;
    }
  }
  return { data, body: normalized.slice(end + 5) };
}

function inspectFences(body) {
  const lines = [];
  let inFence = false;
  for (const line of body.split('\n')) {
    if (/^\s*(```|~~~)/.test(line)) {
      inFence = !inFence;
      continue;
    }
    if (!inFence) lines.push(line);
  }
  return { prose: lines.join('\n'), balanced: !inFence };
}

const sourceFiles = (await Promise.all(['backend', 'api'].map((root) => walk(path.join(docsRoot, root))))).flat();
if (sourceFiles.length !== expectedCount) failures.push(`Expected ${expectedCount} source documents, found ${sourceFiles.length}.`);

const mapped = new Map();
for (const relative of allMappedFiles) mapped.set(relative, (mapped.get(relative) ?? 0) + 1);
for (const [relative, count] of mapped) if (count !== 1) failures.push(`Content map includes ${relative} ${count} times.`);
for (const file of sourceFiles) {
  const relative = path.relative(docsRoot, file).split(path.sep).join('/');
  if (!mapped.has(relative)) failures.push(`Content map is missing ${relative}.`);
}
for (const relative of mapped.keys()) {
  try { await access(path.join(docsRoot, relative)); } catch { failures.push(`Content map points to missing ${relative}.`); }
}

const titles = new Map();
const mermaidBlocks = [];
for (const file of sourceFiles) {
  const relative = path.relative(docsRoot, file).split(path.sep).join('/');
  const source = await readFile(file, 'utf8');
  const { data, body } = parseFrontmatter(source);
  for (const key of ['title', 'description', 'status', 'audience', 'reviewed']) {
    if (!data[key]) failures.push(`${relative}: missing frontmatter ${key}.`);
  }
  if (data.status && !validStatuses.has(data.status)) failures.push(`${relative}: invalid status ${data.status}.`);
  if (data.title) {
    const existing = titles.get(data.title);
    if (existing) failures.push(`${relative}: duplicate title with ${existing}: ${data.title}.`);
    else titles.set(data.title, relative);
  }
  if (archivedFiles.includes(relative)) {
    if (data.status !== 'archived') failures.push(`${relative}: historical document must use archived status.`);
    if (data.search !== false) failures.push(`${relative}: archived document must set search: false.`);
    if (!data.replacement) failures.push(`${relative}: archived document must define replacement.`);
  }
  if (['current', 'reference'].includes(data.status) && (!Array.isArray(data.sourcePaths) || !data.sourcePaths.length)) {
    failures.push(`${relative}: current/reference document must include sourcePaths.`);
  }
  for (const sourcePath of data.sourcePaths ?? []) {
    try { await access(path.join(workspaceRoot, sourcePath)); } catch { failures.push(`${relative}: missing sourcePath ${sourcePath}.`); }
  }
  const inspected = inspectFences(body);
  const prose = inspected.prose;
  if (!inspected.balanced) failures.push(`${relative}: unbalanced Markdown code fence.`);
  const h1Count = (prose.match(/^#\s+.+$/gm) ?? []).length;
  if (h1Count !== 1) failures.push(`${relative}: expected exactly one H1, found ${h1Count}.`);
  if (/^\s*<\/?[A-Za-z][^>]*>\s*$/m.test(prose)) failures.push(`${relative}: raw HTML found outside a code fence.`);
  if (['current', 'reference'].includes(data.status) && !/^##\s+继续阅读\s*$/m.test(prose)) failures.push(`${relative}: missing 继续阅读 section.`);
  const fencePattern = /^```mermaid\s*\n([\s\S]*?)^```\s*$/gm;
  for (const match of body.matchAll(fencePattern)) mermaidBlocks.push({ relative, definition: match[1] });
  const opens = (body.match(/^```mermaid\s*$/gm) ?? []).length;
  const parsed = [...body.matchAll(fencePattern)].length;
  if (opens !== parsed) failures.push(`${relative}: unbalanced Mermaid fence.`);
}

if (mermaidBlocks.length) {
  const mermaid = (await import('mermaid')).default;
  for (const [index, block] of mermaidBlocks.entries()) {
    try { await mermaid.parse(block.definition, { suppressErrors: true }); }
    catch (error) { failures.push(`${block.relative}: Mermaid block ${index + 1} failed to parse: ${error.message}`); }
  }
}

if (failures.length) {
  console.error(`Document quality verification failed (${failures.length} issue(s)):\n${failures.join('\n')}`);
  process.exit(1);
}

console.log(`Verified ${sourceFiles.length} documents, ${allMappedFiles.length} mapped routes, and ${mermaidBlocks.length} Mermaid diagrams.`);
