import {
  BarChart3,
  BrainCircuit,
  LayoutDashboard,
  LibraryBig,
  Plus,
  Sparkles,
  Users,
} from 'lucide-react'
import { NavLink, Outlet } from 'react-router-dom'

import './teacher.css'

const navItems = [
  { label: 'Overview', to: '/teacher', icon: LayoutDashboard, end: true },
  { label: 'Missions', to: '/teacher/missions/mission-python-loops', icon: LibraryBig },
  { label: 'Students', to: '/teacher/students/student-riya-sharma', icon: Users },
  { label: 'Insights', to: '/teacher/missions/mission-python-loops', icon: BrainCircuit },
]

export function TeacherLayout() {
  return (
    <div className="cq-teacher-app">
      <div className="cq-teacher-shell">
        <aside className="cq-teacher-sidebar">
          <NavLink className="cq-teacher-brand" to="/">
            <span>
              <Sparkles aria-hidden="true" className="size-4" />
            </span>
            ClassQuest
          </NavLink>

          <nav aria-label="Teacher navigation" className="cq-teacher-nav">
            {navItems.map((item) => {
              const Icon = item.icon

              return (
                <NavLink end={item.end} key={item.label} to={item.to}>
                  <Icon aria-hidden="true" className="size-4" />
                  {item.label}
                </NavLink>
              )
            })}
          </nav>

          <NavLink className="cq-teacher-create-link" to="/teacher/missions/new">
            <Plus aria-hidden="true" className="size-4" />
            Create Mission
          </NavLink>

          <div className="cq-sidebar-note">
            <p>
              Prototype mode uses local mission data and mock learning
              intelligence for judge walkthroughs.
            </p>
            <p className="mt-3 flex items-center gap-2 text-cq-accent">
              <BarChart3 aria-hidden="true" className="size-4" />
              Command Center online
            </p>
          </div>
        </aside>

        <div className="cq-teacher-main">
          <Outlet />
        </div>
      </div>
    </div>
  )
}
