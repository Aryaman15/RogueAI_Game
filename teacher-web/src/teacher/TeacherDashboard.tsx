import { ArrowRight, Clock, Target, TrendingUp, Users } from 'lucide-react'
import { Link } from 'react-router-dom'

import { getMissions } from '../data/missionStore'
import { missionPerformance } from '../data/mockData'

const stats = [
  { label: 'Students', value: '32', icon: Users },
  { label: 'Missions', value: '8', icon: Target },
  { label: 'Overall Mastery', value: '76%', icon: TrendingUp },
  { label: 'Need Attention', value: '5', icon: Clock },
]

export function TeacherDashboard() {
  const allMissions = getMissions()
  const activeMission =
    allMissions.find((mission) => mission.name === 'Python Loops Revision') ??
    allMissions[0]
  const activePerformance = missionPerformance['mission-python-loops']

  return (
    <>
      <header className="cq-app-header">
        <div>
          <p className="cq-app-eyebrow">Teacher Command Center</p>
          <h1 className="cq-app-title">Educational intelligence command center</h1>
          <p className="cq-app-subtitle">
            Monitor missions, identify weak concepts and inspect the learning
            process behind each student answer.
          </p>
        </div>
        <Link className="cq-app-link-button cq-app-link-button-primary" to="/teacher/missions/new">
          Create Mission
          <ArrowRight aria-hidden="true" className="size-4" />
        </Link>
      </header>

      <section aria-label="Summary metrics" className="cq-stat-grid">
        {stats.map((stat) => {
          const Icon = stat.icon

          return (
            <article className="cq-app-card cq-stat-card" key={stat.label}>
              <span className="flex items-center gap-2">
                <Icon aria-hidden="true" className="size-4 text-cq-accent" />
                {stat.label}
              </span>
              <strong>{stat.value}</strong>
            </article>
          )
        })}
      </section>

      <div className="cq-dashboard-grid">
        <section className="cq-app-card cq-app-card-pad">
          <div className="cq-card-heading">
            <div>
              <p className="cq-app-eyebrow">Active Mission</p>
              <h2>{activeMission.name}</h2>
            </div>
            <span className="cq-pill cq-pill-accent">Live class data</span>
          </div>

          <div className="grid gap-4 md:grid-cols-3">
            <div>
              <p className="cq-mini-label">World</p>
              <strong className="mt-2 block text-cq-text-strong">
                {activeMission.worldName}
              </strong>
              <p className="mt-1 text-cq-text-muted">{activeMission.mapName}</p>
            </div>
            <div>
              <p className="cq-mini-label">Completed</p>
              <strong className="mt-2 block text-3xl text-cq-text-strong">
                {activePerformance.completionLabel} completed
              </strong>
            </div>
            <div>
              <p className="cq-mini-label">Average Attempts</p>
              <strong className="mt-2 block text-3xl text-cq-text-strong">
                {activePerformance.averageAttempts}
              </strong>
            </div>
          </div>

          <div className="mt-6">
            <div className="mb-2 flex justify-between text-sm text-cq-text-muted">
              <span>Average Mastery</span>
              <strong className="text-cq-accent">{activePerformance.averageMastery}%</strong>
            </div>
            <div className="cq-progress-track is-warning">
              <span style={{ width: `${activePerformance.averageMastery}%` }} />
            </div>
          </div>

          <div className="cq-action-row">
            <Link
              className="cq-app-link-button cq-app-link-button-primary"
              to={`/teacher/missions/${activeMission.id}`}
            >
              View Report
              <ArrowRight aria-hidden="true" className="size-4" />
            </Link>
          </div>
        </section>

        <section className="cq-app-card cq-app-card-pad">
          <div className="cq-card-heading">
            <div>
              <p className="cq-app-eyebrow">Past Missions</p>
              <h2>Recent class missions</h2>
            </div>
          </div>

          <div className="grid gap-3">
            {allMissions.slice(0, 4).map((mission) => {
              const performance =
                missionPerformance[mission.id as keyof typeof missionPerformance] ??
                missionPerformance['mission-python-loops']

              return (
                <Link
                  className="cq-link-reset cq-app-card cq-app-card-pad"
                  key={mission.id}
                  to={`/teacher/missions/${mission.id}`}
                >
                  <div className="flex items-start justify-between gap-3">
                    <div>
                      <strong className="text-cq-text-strong">{mission.name}</strong>
                      <p className="mt-1 text-sm text-cq-text-muted">{mission.className}</p>
                    </div>
                    <ArrowRight aria-hidden="true" className="size-4 text-cq-accent" />
                  </div>
                  <div className="mt-3 grid grid-cols-2 gap-2 text-sm text-cq-text-muted">
                    <span>{performance.completionPercent}% completion</span>
                    <span>{performance.averageMastery}% mastery</span>
                  </div>
                </Link>
              )
            })}
          </div>
        </section>
      </div>
    </>
  )
}
