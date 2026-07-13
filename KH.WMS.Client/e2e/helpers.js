import { expect } from '@playwright/test'

/**
 * 登录公共方法，供所有测试文件复用
 */
export async function login(page, username = 'admin', password = '123456') {
  await page.goto('/login')
  await page.fill('input[placeholder*="用户名"], input[placeholder*="账号"]', username)
  await page.fill('input[placeholder*="密码"]', password)
  await page.click('button:has-text("登")')
  await page.waitForURL('**/home**', { timeout: 15000 })
}
