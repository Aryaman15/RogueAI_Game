import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom'

import { LandingPage } from './pages/LandingPage'
import { TeacherLayout } from './teacher/TeacherLayout'
import { TeacherDashboard } from './teacher/TeacherDashboard'
import { CreateMissionWizard } from './teacher/CreateMissionWizard'
import { MissionReport } from './teacher/MissionReport'
import { StudentDiagnostic } from './teacher/StudentDiagnostic'

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route element={<LandingPage />} path="/" />
        <Route element={<TeacherLayout />} path="/teacher">
          <Route index element={<TeacherDashboard />} />
          <Route element={<TeacherDashboard />} path="missions" />
          <Route element={<CreateMissionWizard />} path="missions/new" />
          <Route element={<MissionReport />} path="missions/:id" />
          <Route element={<StudentDiagnostic />} path="students/:id" />
          <Route element={<Navigate replace to="/teacher" />} path="*" />
        </Route>
      </Routes>
    </BrowserRouter>
  )
}

export default App
