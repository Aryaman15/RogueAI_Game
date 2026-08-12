import { AlertTriangle, ArrowRight, Clock, Target, Users } from 'lucide-react'
import { Link, Navigate, useParams } from 'react-router-dom'

import { getMissionById } from '../data/missionStore'
import {
  conceptMastery,
  learningInsights,
  missionPerformance,
  missionResults,
  students,
} from '../data/mockData'

const metricIcons = [Users, Target, AlertTriangle, Clock]

export function MissionReport() {
  const { id } = useParams()
  const mission = id ? getMissionById(id) : undefined

  if (!mission) {
    return <Navigate replace to="/teacher" />
  }

  const performance =
    missionPerformance[mission.id as keyof typeof missionPerformance] ??
    missionPerformance['mission-python-loops']
  const reportResults = missionResults.map((result) => ({
    ...result,
    student: students.find((student) => student.id === result.studentId),
  }))

  const metrics = [
    ['Completed', 'completionLabel' in performance ? performance.completionLabel : '24 / 32'],
    ['Average Score', 'averageScore' in performance ? `${performance.averageScore}%` : '73%'],
    ['Average Attempts', 'averageAttempts' in performance ? String(performance.averageAttempts) : '1.8'],
    ['Average Time', 'averageTime' in performance ? performance.averageTime : '12m 41s'],
  ]

  return (
    <>
      <header className="cq-app-header">
        <div>
          <p className="cq-app-eyebrow">Mission Report</p>
          <h1 className="cq-app-title">{mission.name}</h1>
          <p className="cq-app-subtitle">
            Class {mission.className} - {mission.worldName} / {mission.mapName}
          </p>
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

      <div className="cq-report-grid">
        <div className="grid gap-4">
          <section className="cq-app-card cq-app-card-pad">
            <div className="cq-card-heading">
              <div>
                <p className="cq-app-eyebrow">Concept Mastery</p>
                <h2>Where the class is strong and where it needs help</h2>
              </div>
            </div>

            {conceptMastery.map((item) => (
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
                  {reportResults.map((result) => (
                    <tr key={result.studentId}>
                      <td>
                        <Link
                          className="cq-link-reset font-semibold text-cq-text-strong"
                          to={`/teacher/students/${result.studentId}`}
                        >
                          {result.student?.name}
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
                </tbody>
              </table>
            </div>
          </section>
        </div>

        <aside>
          {learningInsights.map((insight) => (
            <article className="cq-app-card cq-app-card-pad cq-teacher-insight-card" key={insight.id}>
              <div className="flex items-center justify-between gap-3">
                <h3>{insight.title}</h3>
                <span className="cq-pill cq-pill-warning">
                  {insight.affectedStudents} students
                </span>
              </div>
              <p>{insight.evidence}</p>
              <p>
                <strong className="text-cq-text-strong">Likely misconception:</strong>{' '}
                {insight.misconception}
              </p>
              <p>
                <strong className="text-cq-text-strong">Recommended teacher action:</strong>{' '}
                {insight.recommendedAction}
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
