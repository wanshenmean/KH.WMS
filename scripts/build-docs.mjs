import { cp, mkdir, rm, writeFile } from 'node:fs/promises';
import { spawnSync } from 'node:child_process';

const result = spawnSync('npm', ['--prefix', 'KH.WMS.Docs', 'run', 'build'], {
  shell: process.platform === 'win32',
  stdio: 'inherit',
});

if (result.error || result.status !== 0) {
  if (result.error) {
    console.error(result.error);
  }
  process.exit(result.status ?? 1);
}

await rm('dist', { recursive: true, force: true });
await cp('KH.WMS.Docs/.vitepress/dist', 'dist', { recursive: true });

// Sites publishes through a Cloudflare Worker. The documentation itself is
// pre-rendered by VitePress, so the worker only delegates requests to the
// platform's static-assets binding.
await mkdir('dist/server', { recursive: true });
await writeFile(
  'dist/server/index.js',
  'export default { async fetch(request, env) { return env.ASSETS.fetch(request); } };\n',
);
