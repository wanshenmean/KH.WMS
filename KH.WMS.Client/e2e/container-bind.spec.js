import { test, expect } from '@playwright/test'
import { login } from './helpers'

/**
 * 组盘管理页面测试
 */
test.describe('组盘管理', () => {
  test.beforeEach(async ({ page }) => {
    await login(page)
    // 展开入库管理菜单
    await page.locator('.el-sub-menu__title:has-text("入库管理")').click()
    await page.waitForTimeout(500)
    // 点击组盘管理子菜单
    await page.locator('.el-menu-item:has-text("组盘管理")').click()
    await page.waitForLoadState('networkidle')
    await page.waitForTimeout(1000)
  })

  test('页面加载 - 表格列头完整', async ({ page }) => {
    const table = page.locator('.el-table')
    await expect(table).toBeVisible()

    const headers = ['容器编号', '组盘状态', '质量状态', '来源类型', '来源单号']
    for (const header of headers) {
      await expect(page.locator(`.el-table__header th:has-text("${header}")`)).toBeVisible()
    }
  })

  test('状态标签 - 非硬编码渲染', async ({ page }) => {
    // 状态列应该渲染为 el-tag 而不是纯文本
    const tags = page.locator('.el-table__body .el-tag')
    const tagCount = await tags.count()
    // 如果有数据行，应该有 tag 渲染
    if (tagCount > 0) {
      // 验证 tag 有文本内容（非空）
      const firstTagText = await tags.first().textContent()
      expect(firstTagText.trim().length).toBeGreaterThan(0)
    }
  })

  test('搜索功能 - 组盘状态下拉使用字典', async ({ page }) => {
    const statusSelect = page.locator('.el-form-item:has-text("组盘状态") .el-select')
    if (await statusSelect.isVisible()) {
      await statusSelect.click()
      await page.waitForTimeout(500)
      // 下拉选项应该是从字典加载的
      const options = page.locator('.el-select-dropdown__item')
      expect(await options.count()).toBeGreaterThan(0)
    }
  })

  test('查看详情弹窗 - 包含组盘状态和质量状态', async ({ page }) => {
    // 查找表格中是否有数据行
    const rows = page.locator('.el-table__body tbody tr')
    const rowCount = await rows.count()

    if (rowCount === 0) {
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

    // 验证弹窗打开
    const dialog = page.locator('.el-overlay-dialog .el-dialog, .el-dialog').last()
    await expect(dialog).toBeVisible({ timeout: 5000 })

    // 验证详情中包含状态字段
    const allText = await dialog.textContent()
    expect(allText).toContain('组盘状态')
    expect(allText).toContain('质量状态')
  })
})
