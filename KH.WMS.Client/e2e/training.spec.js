import { test, expect } from '@playwright/test'
import { login } from './helpers.js'

async function api(page, method, url, data) {
  const token = await page.evaluate(() => localStorage.getItem('token'))
  const response = await page.request.fetch(url, {
    method,
    data,
    headers: { Authorization: `Bearer ${token}` },
  })
  const body = await response.json()
  expect(response.ok(), body.message).toBeTruthy()
  return body
}

async function apiFailure(page, method, url, data) {
  const token = await page.evaluate(() => localStorage.getItem('token'))
  const response = await page.request.fetch(url, {
    method,
    data,
    headers: { Authorization: `Bearer ${token}` },
  })
  const body = await response.json()
  expect(response.ok(), JSON.stringify(body)).toBeFalsy()
  return body
}

async function findAppointments(page, appointmentNo) {
  const result = await api(page, 'POST', '/api/training-arrival-appointment/pagelist', {
    pageIndex: 1,
    pageSize: 10,
    sortConditions: [],
    filters: [{ field: 'appointmentNo', operator: 'equals', value: appointmentNo }],
  })
  return result.data?.items || []
}

async function expectAppointmentMissing(page, appointmentNo) {
  await expect.poll(async () => (await findAppointments(page, appointmentNo)).length).toBe(0)
}

test('training pages and CRUD flows', async ({ page }) => {
  test.setTimeout(120000)
  page.on('pageerror', error => console.error('[pageerror]', error.message))
  page.on('console', message => {
    if (message.type() === 'error') console.error('[console]', message.text())
  })
  await login(page)

  for (const route of [
    '/training/carrier',
    '/training/owner-profile',
    '/training/arrival-appointment',
  ]) {
    await page.goto(route)
    await expect(page).toHaveURL(new RegExp(route.replaceAll('/', '\\/')))
    await expect(page.locator('.kh-page')).toBeVisible()
  }

  const stamp = Date.now()
  let carrierId
  let ownerId
  let appointmentId
  let secondAppointmentId

  try {
    const carrier = await api(page, 'POST', '/api/training-carrier/create', {
      carrierCode: `E2E-CAR-${stamp}`,
      carrierName: '自动化承运商',
      transportMode: 'ROAD',
      status: 1,
    })
    carrierId = carrier.data
    await api(page, 'PUT', `/api/training-carrier/status/${carrierId}`, { status: 0 })

    const owner = await api(page, 'POST', '/api/training-owner-profile/create', {
      ownerCode: `E2E-OWN-${stamp}`,
      ownerName: '自动化货主',
      extDataRaw: JSON.stringify({ customerLevel: 'A', creditLimit: 1000, requiresColdChain: true }),
    })
    ownerId = owner.data
    let ownerDetail = await api(page, 'GET', `/api/training-owner-profile/${ownerId}`)
    expect(ownerDetail.data.customerLevel).toBe('A')
    await api(page, 'POST', '/api/training-owner-profile/update', {
      id: ownerId,
      ownerCode: `E2E-OWN-${stamp}`,
      ownerName: '自动化货主',
      extDataRaw: '{}',
    })
    ownerDetail = await api(page, 'GET', `/api/training-owner-profile/${ownerId}`)
    expect(ownerDetail.data.customerLevel).toBeUndefined()

    const invalidAppointments = [
      {
        appointmentNo: `E2E-APT-EMPTY-${stamp}`,
        carrierId,
        ownerId,
        appointmentDate: '2026-08-03',
        items: [],
      },
      {
        appointmentNo: `E2E-APT-DUPLICATE-${stamp}`,
        carrierId,
        ownerId,
        appointmentDate: '2026-08-03',
        items: [
          { id: 0, lineNo: 1, materialCode: 'E2E-MAT-D1', materialName: '重复行号物料一', expectedQty: 1 },
          { id: 0, lineNo: 1, materialCode: 'E2E-MAT-D2', materialName: '重复行号物料二', expectedQty: 2 },
        ],
      },
      {
        appointmentNo: `E2E-APT-QTY-${stamp}`,
        carrierId,
        ownerId,
        appointmentDate: '2026-08-03',
        items: [{ id: 0, lineNo: 1, materialCode: 'E2E-MAT-QTY', materialName: '无效数量物料', expectedQty: 0 }],
      },
      {
        appointmentNo: `E2E-APT-ID-${stamp}`,
        carrierId,
        ownerId,
        appointmentDate: '2026-08-03',
        items: [{ id: 999999999, lineNo: 1, materialCode: 'E2E-MAT-ID', materialName: '非法明细 ID 物料', expectedQty: 1 }],
      },
    ]
    for (const invalidAppointment of invalidAppointments) {
      await apiFailure(page, 'POST', '/api/training-arrival-appointment/create', invalidAppointment)
      await expectAppointmentMissing(page, invalidAppointment.appointmentNo)
    }

    const appointment = await api(page, 'POST', '/api/training-arrival-appointment/create', {
      appointmentNo: `E2E-APT-${stamp}`,
      carrierId,
      ownerId,
      appointmentDate: '2026-08-03',
      items: [{ id: 0, lineNo: 1, materialCode: 'E2E-MAT-1', materialName: '自动化物料', expectedQty: 10 }],
    })
    appointmentId = appointment.data
    const detail = await api(page, 'GET', `/api/training-arrival-appointment/${appointmentId}`)
    expect(detail.data.items).toHaveLength(1)
    expect(detail.data.items[0].id).toBeGreaterThan(0)
    const originalLineId = detail.data.items[0].id
    await api(page, 'POST', '/api/training-arrival-appointment/update', {
      ...detail.data,
      items: [
        { ...detail.data.items[0], expectedQty: 12 },
        { id: 0, lineNo: 2, materialCode: 'E2E-MAT-2', materialName: '新增物料', expectedQty: 5 },
      ],
    })
    const updated = await api(page, 'GET', `/api/training-arrival-appointment/${appointmentId}`)
    expect(updated.data.items).toHaveLength(2)
    expect(updated.data.items.find(x => x.id === originalLineId)?.expectedQty).toBe(12)
    expect(updated.data.items.find(x => x.lineNo === 2)?.id).toBeGreaterThan(0)

    const secondAppointment = await api(page, 'POST', '/api/training-arrival-appointment/create', {
      appointmentNo: `E2E-APT-SECOND-${stamp}`,
      carrierId,
      ownerId,
      appointmentDate: '2026-08-04',
      items: [{ id: 0, lineNo: 1, materialCode: 'E2E-MAT-OTHER', materialName: '其他预约物料', expectedQty: 3 }],
    })
    secondAppointmentId = secondAppointment.data
    const secondDetail = await api(page, 'GET', `/api/training-arrival-appointment/${secondAppointmentId}`)
    const rejected = await apiFailure(page, 'POST', '/api/training-arrival-appointment/update', {
      ...updated.data,
      items: [{ ...secondDetail.data.items[0], lineNo: 1 }],
    })
    expect(rejected.message).toContain('不属于当前')

    await api(page, 'POST', '/api/training-arrival-appointment/update', {
      ...updated.data,
      items: [{ ...updated.data.items[0], expectedQty: 15 }],
    })
    const pruned = await api(page, 'GET', `/api/training-arrival-appointment/${appointmentId}`)
    expect(pruned.data.items).toHaveLength(1)
    expect(pruned.data.items[0].id).toBe(originalLineId)
    expect(pruned.data.items[0].expectedQty).toBe(15)

    await api(page, 'DELETE', `/api/training-arrival-appointment/delete/${appointmentId}`)
    const deleted = await api(page, 'GET', `/api/training-arrival-appointment/${appointmentId}`)
    expect(deleted.code).toBe(404)
    appointmentId = undefined
  } finally {
    if (secondAppointmentId) await api(page, 'DELETE', `/api/training-arrival-appointment/delete/${secondAppointmentId}`)
    if (appointmentId) await api(page, 'DELETE', `/api/training-arrival-appointment/delete/${appointmentId}`)
    if (ownerId) await api(page, 'DELETE', `/api/training-owner-profile/delete/${ownerId}`)
    if (carrierId) await api(page, 'DELETE', `/api/training-carrier/delete/${carrierId}`)
  }
})
