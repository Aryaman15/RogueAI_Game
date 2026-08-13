import { ArrowRight, Clock, Target, TrendingUp, Users } from 'lucide-react'
import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'

import { listMissions } from '../api/missions'
import { missionPerformance, missions as demoMissions } from '../data/mockData'
import type { Mission } from '../data/models'

const stats = [
  { label: 'Students', value: '32', icon: Users },
  { label: 'Missions', value: '8', icon: Target },
  { label: 'Overall Mastery', value: '76%', icon: TrendingUp },
  { label: 'Need Attention', value: '5', icon: Clock },
]

export function TeacherDashboard() {
  const [backendMissions, setBackendMissions] = useState<Mission[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState('')
  const allMissions = backendMissions
  const activeMission =
    allMissions.find((mission) => mission.name === 'Python Loops Revision') ?? allMissions[0]
  const activePerformance =
    activeMission && activeMission.id in missionPerformance
      ? missionPerformance[activeMission.id as keyof typeof missionPerformance]
      : null

  useEffect(() => {
    let ignore = false

    async function loadMissions() {
      try {
        setIsLoading(true)
        const missions = await listMissions()

        if (!ignore) {
          setBackendMissions(missions)
          setError('')
        }
      } catch {
        if (!ignore) {
          setError('Could not reach ClassQuest Mission Server.')
        }
      } finally {
        if (!ignore) {
          setIsLoading(false)
        }
      }
    }

    void loadMissions()

    return () => {
      ignore = true
    }
  }, [])

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
            <h2>{activeMission?.name ?? 'No backend missions yet'}</h2>
            </div>
            <span className="cq-pill cq-pill-accent">
              {activeMission ? 'Backend mission' : 'Mission server'}
            </span>
          </div>

          {isLoading ? (
            <p className="text-cq-text-muted">Loading missions from ClassQuest Mission Server.</p>
          ) : error ? (
            <p className="text-cq-warning">{error}</p>
          ) : activeMission ? (
            <>
              <div className="grid gap-4 md:grid-cols-3">
                <div>
                  <p className="cq-mini-label">World</p>
                  <strong className="mt-2 block text-cq-text-strong">
                    {activeMission.worldName}
                  </strong>
                  <p className="mt-1 text-cq-text-muted">{activeMission.mapName}</p>
                </div>
                <div>
                  <p className="cq-mini-label">Mission Code</p>
                  <strong className="mt-2 block text-3xl text-cq-text-strong">
                    {activeMission.missionCode}
                  </strong>
                </div>
                <div>
                  <p className="cq-mini-label">Questions</p>
                  <strong className="mt-2 block text-3xl text-cq-text-strong">
                    {activeMission.challenges.length}
                  </strong>
                </div>
              </div>

              <div className="mt-6">
                <div className="mb-2 flex justify-between text-sm text-cq-text-muted">
                  <span>Backend Status</span>
                  <strong className="text-cq-accent">{activeMission.status}</strong>
                </div>
                <div className="cq-progress-track">
                  <span style={{ width: `${activePerformance?.completionPercent ?? 100}%` }} />
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
            </>
          ) : (
            <div>
              <p className="text-cq-text-muted">
                Published backend missions will appear here after deployment.
              </p>
              <div className="cq-action-row">
                <Link
                  className="cq-app-link-button cq-app-link-button-primary"
                  to="/teacher/missions/new"
                >
                  Create Mission
                  <ArrowRight aria-hidden="true" className="size-4" />
                </Link>
              </div>
            </div>
          )}
        </section>

        <section className="cq-app-card cq-app-card-pad">
          <div className="cq-card-heading">
            <div>
              <p className="cq-app-eyebrow">Published Missions</p>
              <h2>Backend missions</h2>
            </div>
          </div>

          <div className="grid gap-3">
            {isLoading ? <p className="text-cq-text-muted">Loading mission list.</p> : null}
            {!isLoading && error ? <p className="text-cq-warning">{error}</p> : null}
            {!isLoading && !error && allMissions.length === 0 ? (
              <p className="text-cq-text-muted">No backend missions published yet.</p>
            ) : null}
            {allMissions.slice(0, 4).map((mission) => {
              const performance =
                missionPerformance[mission.id as keyof typeof missionPerformance] ??
                null

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
                    <span>{mission.missionCode}</span>
                    <span>
                      {performance
                        ? `${performance.averageMastery}% mastery`
                        : `${mission.challenges.length} questions`}
                    </span>
                  </div>
                </Link>
              )
            })}
          </div>
        </section>
      </div>

      <section className="cq-app-card cq-app-card-pad mt-4">
        <div className="cq-card-heading">
          <div>
            <p className="cq-app-eyebrow">Demo Data</p>
            <h2>Sample missions</h2>
          </div>
        </div>
        <div className="grid gap-3 md:grid-cols-2">
          {demoMissions.slice(0, 4).map((mission) => (
            <article className="cq-app-card cq-app-card-pad" key={mission.id}>
              <div className="flex items-start justify-between gap-3">
                <div>
                  <strong className="text-cq-text-strong">{mission.name}</strong>
                  <p className="mt-1 text-sm text-cq-text-muted">
                    Demo only - {mission.missionCode}
                  </p>
                </div>
                <span className="cq-pill">Demo</span>
              </div>
            </article>
          ))}
        </div>
      </section>
    </>
  )
}
