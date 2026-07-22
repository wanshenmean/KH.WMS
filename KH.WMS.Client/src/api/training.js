import request from '@/utils/request'

export const getTrainingCarriers = () => request.get('/api/training-carrier/all')
export const getTrainingOwners = () => request.get('/api/training-owner-profile/all')

export const importTrainingCarriers = (file) => {
  const data = new FormData()
  data.append('file', file)
  return request.post('/api/training-carrier/import', data, {
    headers: { 'Content-Type': 'multipart/form-data' },
  })
}
