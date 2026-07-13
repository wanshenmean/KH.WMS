import { test, expect } from '@playwright/test'
import { login } from './helpers'

// ==================== 测试用例 ====================

const credentials = [
  { username: 'admin', password: '123456', desc: '超级管理员' },
]

for (const { username, password, desc } of credentials) {
  test(`登录 - ${desc} (${username})`, async ({ page }) => {
    await login(page, username, password)
    // 验证已进入首页
    await expect(page).toHaveURL(/\/home/)
  })
}

test('登录 - 错误密码提示', async ({ page }) => {
  await page.goto('/login')
  await page.fill('input[placeholder*="用户名"], input[placeholder*="账号"]', 'admin')
  await page.fill('input[placeholder*="密码"]', 'wrongpassword')
  await page.click('button:has-text("登")')
  // 应该还在登录页，或出现错误提示
  await page.waitForTimeout(2000)
  const stillOnLogin = page.url().includes('/login')
  const hasError = await page.locator('.el-message--error, .el-message-box').count()
  expect(stillOnLogin || hasError > 0).toBeTruthy()
})

test('登录 - 未登录访问受保护页面跳转到登录页', async ({ page }) => {
  // 清除 token
  await page.goto('/')
  await page.evaluate(() => localStorage.removeItem('token'))
  // 访问首页应重定向到登录页
  await page.goto('/home')
  await page.waitForURL('**/login**', { timeout: 10000 })
  await expect(page).toHaveURL(/\/login/)
})
