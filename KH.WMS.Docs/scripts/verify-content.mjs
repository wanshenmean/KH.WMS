import { access, readFile, readdir } from 'node:fs/promises';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const siteRoot = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '..');
const contentRoot = path.join(siteRoot, '.site-content');
const sourceRoot = path.resolve(siteRoot, '..', 'docs');

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

const localLink = /\[[^\]]*\]\((?!https?:|mailto:|#)([^)\s]+)(?:\s+"[^"]*")?\)/g;
const failures = [];
const markdownFiles = await walk(contentRoot);

for (const markdownFile of markdownFiles.filter((file) => file.endsWith('.md'))) {
  const text = await readFile(markdownFile, 'utf8');
  for (const match of text.matchAll(localLink)) {
    const target = decodeURI(match[1].split('#')[0]);
    if (!target) continue;
    const resolved = target.startsWith('/')
      ? path.join(contentRoot, target.slice(1))
      : path.resolve(path.dirname(markdownFile), target);
    const publicResolved = target.startsWith('/')
      ? path.join(contentRoot, 'public', target.slice(1))
      : null;
    // Some KH.WMS document names contain a dot in the basename. Always try a
    // Markdown page variant instead of inferring an extension from that dot.
    const possibilities = [
      resolved,
      `${resolved}.md`,
      path.join(resolved, 'index.md'),
      ...(publicResolved ? [publicResolved, `${publicResolved}.md`, path.join(publicResolved, 'index.md')] : [])
    ];
    try {
      await Promise.any(possibilities.map((candidate) => access(candidate)));
    } catch {
      failures.push(`${path.relative(contentRoot, markdownFile)} -> ${match[1]}`);
    }
  }
}

const manifest = JSON.parse(await readFile(path.join(contentRoot, '.content-manifest.json'), 'utf8'));
const sourceMarkdown = (await Promise.all(['backend', 'api'].map((directory) => walk(path.join(sourceRoot, directory))))).flat().filter((file) => file.endsWith('.md')).length;
if (manifest.markdownCount !== sourceMarkdown) failures.push(`Expected ${sourceMarkdown} source Markdown files but synchronized ${manifest.markdownCount}.`);

if (failures.length) {
  console.error(`Content verification failed (${failures.length} issue(s)):\n${failures.join('\n')}`);
  process.exit(1);
}

console.log(`Verified ${manifest.markdownCount} source Markdown files, ${manifest.downloadCount} downloads, and local links.`);
