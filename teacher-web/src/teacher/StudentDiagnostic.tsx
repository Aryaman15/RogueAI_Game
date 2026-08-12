import { ArrowLeft, CheckCircle2, XCircle } from 'lucide-react'
import { Link, Navigate, useParams } from 'react-router-dom'

import { missionResults, studentDiagnostics, students } from '../data/mockData'

export function StudentDiagnostic() {
  const { id } = useParams()
  const student = students.find((item) => item.id === id)
  const diagnostic =
    studentDiagnostics.find((item) => item.studentId === id) ??
    studentDiagnostics[0]
  const result =
    missionResults.find((item) => item.studentId === id && item.attemptHistory.length) ??
    missionResults[0]

  if (!student) {
    return <Navigate replace to="/teacher" />
  }

  return (
    <>
      <header className="cq-app-header">
        <div>
          <p className="cq-app-eyebrow">Student Diagnostic</p>
          <h1 className="cq-app-title">{student.name}</h1>
          <p className="cq-app-subtitle">
            Individual learning profile, concept breakdown and attempt-level
            evidence from the Rogue AI mission.
          </p>
        </div>
        <Link className="cq-app-link-button" to="/teacher/missions/mission-python-loops">
          <ArrowLeft aria-hidden="true" className="size-4" />
          Mission Report
        </Link>
      </header>

      <section aria-label="Student summary" className="cq-stat-grid">
        <article className="cq-app-card cq-stat-card">
          <span>Overall Mastery</span>
          <strong>{diagnostic.overallMastery}%</strong>
        </article>
        <article className="cq-app-card cq-stat-card">
          <span>Assignments Completed</span>
          <strong>{diagnostic.assignmentsCompleted}</strong>
        </article>
        <article className="cq-app-card cq-stat-card">
          <span>Current Trend</span>
          <strong className="text-cq-accent">{diagnostic.currentTrend}</strong>
        </article>
        <article className="cq-app-card cq-stat-card">
          <span>Current Status</span>
          <strong>{result.status}</strong>
        </article>
      </section>

      <div className="cq-diagnostic-grid mt-4">
        <section className="cq-app-card cq-app-card-pad">
          <div className="cq-card-heading">
            <div>
              <p className="cq-app-eyebrow">Skill Breakdown</p>
              <h2>Concept-level mastery</h2>
            </div>
          </div>

          {diagnostic.skillBreakdown.map((skill) => (
            <div className="cq-skill-row" key={skill.concept}>
              <strong>{skill.concept}</strong>
              <div className={`cq-progress-track ${skill.mastery < 65 ? 'is-warning' : ''}`}>
                <span style={{ width: `${skill.mastery}%` }} />
              </div>
              <span className={skill.mastery < 65 ? 'text-cq-warning' : 'text-cq-accent'}>
                {skill.mastery}%
              </span>
            </div>
          ))}
        </section>

        <section className="cq-app-card cq-app-card-pad">
          <div className="cq-card-heading">
            <div>
              <p className="cq-app-eyebrow">Attempt History</p>
              <h2>{result.attemptHistory[0]?.checkpoint ?? 'Generator Terminal'}</h2>
            </div>
            <span className="cq-pill cq-pill-warning">Pattern detected</span>
          </div>

          <p className="cq-mini-label">Question</p>
          <pre className="cq-code-snippet mt-2">
            {result.attemptHistory[0]?.codeSnippet ?? 'for i in range(1,4):\n    print(i)'}
          </pre>

          <div className="mt-4">
            {result.attemptHistory.map((attempt, index) => (
              <article className="cq-attempt-card" key={attempt.id}>
                <div className="flex items-start justify-between gap-3">
                  <div>
                    <p className="cq-mini-label">Attempt {index + 1}</p>
                    <p className="mt-2 font-mono text-cq-text-strong">{attempt.answer}</p>
                  </div>
                  <span className={`cq-pill ${attempt.isCorrect ? 'cq-pill-accent' : 'cq-pill-warning'}`}>
                    {attempt.isCorrect ? (
                      <CheckCircle2 aria-hidden="true" className="size-4" />
                    ) : (
                      <XCircle aria-hidden="true" className="size-4" />
                    )}
                    {attempt.isCorrect ? 'Correct' : 'Incorrect'}
                  </span>
                </div>
                <p className="mt-3 text-sm text-cq-text-muted">Time: {attempt.timeSeconds} sec</p>
              </article>
            ))}
          </div>

          <article className="cq-app-card cq-app-card-pad cq-teacher-insight-card mt-4">
            <h3>Detected Pattern</h3>
            <p>{diagnostic.pattern}</p>
            <p>
              <strong className="text-cq-text-strong">Recommended action:</strong>{' '}
              {diagnostic.recommendedAction}
            </p>
          </article>
        </section>
      </div>
    </>
  )
}
