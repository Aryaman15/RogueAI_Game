import { AlertTriangle, ArrowRight, Clock, Target, Users } from 'lucide-react'
import { useEffect, useMemo, useState } from 'react'
import { Link, Navigate, useParams } from 'react-router-dom'

import { getMission } from '../api/missions'
import { getMissionReport } from '../api/reports'
import type { BackendMissionReport } from '../api/types'
import type { Mission, StudentStatus } from '../data/models'

const metricIcons = [Users, Target, AlertTriangle, Clock]

export function MissionReport() {
  const { id } = useParams()
  const [mission, setMission] = useState<Mission | null>(null)
  const [report, setReport] = useState<BackendMissionReport | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [notFound, setNotFound] = useState(false)
  const [error, setError] = useState('')

  useEffect(() => {
    let ignore = false

    async function loadReport() {
      if (!id) {
        setNotFound(true)
        setIsLoading(false)
        return
      }

      try {
        setIsLoading(true)
        const [missionDetails, missionReport] = await Promise.all([
          getMission(id),
          getMissionReport(id),
        ])

        if (!ignore) {
          setMission(missionDetails)
          setReport(missionReport)
          setError('')
          setNotFound(false)
        }
      } catch {
        if (!ignore) {
          setNotFound(true)
          setError('Could not load mission report from ClassQuest Mission Server.')
        }
      } finally {
        if (!ignore) {
          setIsLoading(false)
        }
      }
    }

    void loadReport()

    return () => {
      ignore = true
    }
  }, [id])

  const conceptRows = useMemo(() => {
    if (!report) {
      return []
    }

    return report.conceptPerformance.map((item) => ({
      concept: item.concept,
      mastery:
        item.studentsAttempted === 0
          ? 0
          : Math.round((item.correctStudents / item.studentsAttempted) * 100),
    }))
  }, [report])

  const studentRows = useMemo(() => {
    if (!mission || !report) {
      return []
    }

    return report.studentSummaries.map((summary) => {
      const completion =
        mission.challenges.length === 0
          ? 0
          : Math.round((summary.correctChallenges / mission.challenges.length) * 100)

      return {
        ...summary,
        completion,
        mastery: completion,
        timeMinutes: Math.round(summary.totalTimeSeconds / 60),
        status: getStudentStatus(completion),
      }
    })
  }, [mission, report])

  if (!isLoading && notFound && !mission) {
    return <Navigate replace to="/teacher" />
  }

  if (isLoading || !mission || !report) {
    return (
      <section className="cq-app-card cq-app-card-pad">
        <p className="cq-app-eyebrow">Mission Report</p>
        <h1 className="cq-app-title">Loading report</h1>
        <p className="cq-app-subtitle">
          Fetching mission data from ClassQuest Mission Server.
        </p>
      </section>
    )
  }

  const metrics = [
    ['Completed', `${report.completedStudents} / ${report.uniqueStudents}`],
    ['Average Score', `${getAverageScore(report)}%`],
    ['Average Attempts', String(report.averageAttempts)],
    ['Average Time', formatSeconds(report.averageTime)],
  ]
  const hasStudentActivity = report.uniqueStudents > 0

  return (
    <>
      <header className="cq-app-header">
        <div>
          <p className="cq-app-eyebrow">Mission Report</p>
          <h1 className="cq-app-title">{mission.name}</h1>
          <p className="cq-app-subtitle">
            Class {mission.className} - {mission.worldName} / {mission.mapName}
          </p>
          {error ? <p className="mt-2 text-cq-warning">{error}</p> : null}
        </div>
        <Link className="cq-app-link-button" to="/teacher">
          Back to Command Center
        </Link>
      </header>

      <section aria-label="Mission metrics" className="cq-metric-grid">
        {metrics.map(([label, value], index) => {
          const Icon = metricIcons[index]

          return (
            <article className="cq-app-card cq-stat-card" key={label}>
              <span className="flex items-center gap-2">
                <Icon aria-hidden="true" className="size-4 text-cq-accent" />
                {label}
              </span>
              <strong>{value}</strong>
            </article>
          )
        })}
      </section>

      {!hasStudentActivity ? (
        <section className="cq-app-card cq-app-card-pad cq-empty-state">
          <p className="cq-app-eyebrow">AWAITING STUDENT ACTIVITY</p>
          <h2>Mission {mission.missionCode} has been deployed.</h2>
          <p>Student attempts will appear here after gameplay begins.</p>
        </section>
      ) : null}

      <div className="cq-report-grid">
        <div className="grid gap-4">
          <section className="cq-app-card cq-app-card-pad">
            <div className="cq-card-heading">
              <div>
                <p className="cq-app-eyebrow">Concept Mastery</p>
                <h2>Where the class is strong and where it needs help</h2>
              </div>
            </div>

            {conceptRows.length === 0 ? (
              <p className="text-cq-text-muted">Concept results will appear after attempts arrive.</p>
            ) : null}
            {conceptRows.map((item) => (
              <div className="cq-mastery-row" key={item.concept}>
                <strong>{item.concept}</strong>
                <div className={`cq-progress-track ${item.mastery < 65 ? 'is-warning' : ''}`}>
                  <span style={{ width: `${item.mastery}%` }} />
                </div>
                <span className={item.mastery < 65 ? 'text-cq-warning' : 'text-cq-accent'}>
                  {item.mastery}%
                </span>
              </div>
            ))}
          </section>

          <section className="cq-app-card cq-app-card-pad">
            <div className="cq-card-heading">
              <div>
                <p className="cq-app-eyebrow">Student Results</p>
                <h2>Class performance by learner</h2>
              </div>
            </div>
            <div className="cq-table-wrap">
              <table className="cq-table">
                <thead>
                  <tr>
                    <th>Student</th>
                    <th>Completion</th>
                    <th>Mastery</th>
                    <th>Attempts</th>
                    <th>Time</th>
                    <th>Status</th>
                  </tr>
                </thead>
                <tbody>
                  {studentRows.map((result) => (
                    <tr key={result.studentId}>
                      <td>
                        <Link
                          className="cq-link-reset font-semibold text-cq-text-strong"
                          to={`/teacher/students/${result.studentId}`}
                        >
                          {result.studentName}
                        </Link>
                      </td>
                      <td>{result.completion}%</td>
                      <td>{result.mastery}%</td>
                      <td>{result.attempts}</td>
                      <td>{result.timeMinutes}m</td>
                      <td>
                        <span
                          className={`cq-pill ${
                            result.status === 'Attention' ||
                            result.status === 'Needs Review'
                              ? 'cq-pill-warning'
                              : 'cq-pill-accent'
                          }`}
                        >
                          {result.status}
                        </span>
                      </td>
                    </tr>
                  ))}
                  {studentRows.length === 0 ? (
                    <tr>
                      <td colSpan={6}>No student attempts have been recorded yet.</td>
                    </tr>
                  ) : null}
                </tbody>
              </table>
            </div>
          </section>
        </div>

        <aside>
          {report.challengePerformance.map((challenge) => (
            <article
              className="cq-app-card cq-app-card-pad cq-teacher-insight-card"
              key={challenge.challengeId}
            >
              <div className="flex items-center justify-between gap-3">
                <h3>{challenge.concept}</h3>
                <span className="cq-pill cq-pill-warning">{challenge.slotId}</span>
              </div>
              <p>
                <strong className="text-cq-text-strong">Students attempted:</strong>{' '}
                {challenge.studentsAttempted}
              </p>
              <p>
                <strong className="text-cq-text-strong">Correct students:</strong>{' '}
                {challenge.correctStudents}
              </p>
              <p>
                <strong className="text-cq-text-strong">Average time:</strong>{' '}
                {formatSeconds(challenge.averageTime)}
              </p>
            </article>
          ))}

          <Link
            className="cq-app-link-button cq-app-link-button-primary mt-4 w-full"
            to="/teacher/students/student-riya-sharma"
          >
            Inspect student attempts
            <ArrowRight aria-hidden="true" className="size-4" />
          </Link>
        </aside>
      </div>
    </>
  )
}

function getAverageScore(report: BackendMissionReport): number {
  const attemptedChallenges = report.challengePerformance.filter(
    (challenge) => challenge.studentsAttempted > 0,
  )

  if (attemptedChallenges.length === 0) {
    return 0
  }

  const total = attemptedChallenges.reduce((sum, challenge) => {
    return sum + challenge.correctStudents / challenge.studentsAttempted
  }, 0)

  return Math.round((total / attemptedChallenges.length) * 100)
}

function formatSeconds(seconds: number): string {
  if (seconds <= 0) {
    return '0m'
  }

  const minutes = Math.floor(seconds / 60)
  const remainder = Math.round(seconds % 60)

  if (minutes === 0) {
    return `${remainder}s`
  }

  return `${minutes}m ${String(remainder).padStart(2, '0')}s`
}

function getStudentStatus(completion: number): StudentStatus {
  if (completion >= 85) {
    return 'Strong'
  }

  if (completion >= 70) {
    return 'On Track'
  }

  if (completion >= 50) {
    return 'Needs Review'
  }

  return 'Attention'
}
