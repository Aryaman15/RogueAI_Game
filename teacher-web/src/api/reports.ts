import { apiRequest } from './client'
import type { BackendMissionReport, BackendStudentReport } from './types'

export async function getMissionReport(missionId: string): Promise<BackendMissionReport> {
  return apiRequest<BackendMissionReport>(`/api/missions/${missionId}/report`)
}

export async function getStudentReport(studentId: string): Promise<BackendStudentReport> {
  return apiRequest<BackendStudentReport>(`/api/students/${studentId}/report`)
}
