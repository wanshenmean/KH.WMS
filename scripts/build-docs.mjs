import { cp, rm } from 'node:fs/promises';
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
