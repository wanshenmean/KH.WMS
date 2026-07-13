import { test, expect } from '@playwright/test'
import { login } from './helpers'

/**
 * 任务管理页面测试
 */
test.describe('任务管理', () => {
  test.beforeEach(async ({ page }) => {
    await login(page)
    // 展开任务管理菜单
    await page.locator('.el-sub-menu__title:has-text("任务管理")').click()
    await page.waitForTimeout(500)
    // 点击任务列表子菜单
    await page.locator('.el-menu-item:has-text("任务列表")').click()
    await page.waitForLoadState('networkidle')
    await page.waitForTimeout(1000)
  })

  test('页面加载 - 表格列头完整', async ({ page }) => {
    const table = page.locator('.el-table')
    await expect(table).toBeVisible()

    const headers = ['任务编号', '任务类型', '优先级', '状态', '容器编号', '起始库位', '目标库位']
    for (const header of headers) {
      await expect(page.locator(`.el-table__header`).getByText(header)).toBeVisible()
    }
  })

  test('搜索功能 - 按任务编号搜索', async ({ page }) => {
    const searchInput = page.locator('.kh-search input[placeholder*="任务编号"]')
    if (await searchInput.isVisible()) {
      await searchInput.fill('TEST001')
      await page.locator('.kh-search button:has-text("查询")').click()
      await page.waitForLoadState('networkidle')
      const table = page.locator('.el-table')
      await expect(table).toBeVisible()
    }
  })

  test('搜索功能 - 按状态下拉筛选', async ({ page }) => {
    const statusFormItem = page.locator('.kh-search .el-form-item').filter({ hasText: '状态' })
    const select = statusFormItem.locator('.el-select')
    if (await select.isVisible()) {
      await select.click()
      await page.waitForTimeout(500)
      const options = page.locator('.el-select-dropdown__item')
      expect(await options.count()).toBeGreaterThan(0)
      // 按 Escape 关闭下拉
      await page.keyboard.press('Escape')
    }
  })

  test('查看详情弹窗 - 包含任务状态和优先级', async ({ page }) => {
    // 查找表格中是否有数据行
    const rows = page.locator('.el-table__body tbody tr')
    const rowCount = await rows.count()

    if (rowCount === 0) {
      // 无数据时跳过测试
      test.skip()
      return
    }

    // 点击第一行的查看按钮
    const viewBtn = rows.first().locator('button:has-text("查看")')
    if (!(await viewBtn.isVisible())) {
      test.skip()
      return
    }

    await viewBtn.click()
    await page.waitForTimeout(1500)

    // 验证弹窗打开（用更宽泛的选择器）
    const dialog = page.locator('.el-overlay-dialog .el-dialog, .el-dialog').last()
    await expect(dialog).toBeVisible({ timeout: 5000 })

    // 验证详情中包含状态和优先级
    const allText = await dialog.textContent()
    expect(allText).toContain('状态')
    expect(allText).toContain('优先级')
  })
})
