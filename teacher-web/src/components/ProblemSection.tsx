import {
  AlertCircle,
  BarChart3,
  CheckCircle2,
  ClipboardCheck,
  Clock3,
  EyeOff,
  GraduationCap,
  Lightbulb,
  XCircle,
} from 'lucide-react'
import { useState } from 'react'

import './problem-section.css'

type ComparisonMode = 'traditional' | 'classquest'

export function ProblemSection() {
  const [mode, setMode] = useState<ComparisonMode>('traditional')
  const isClassQuest = mode === 'classquest'

  return (
    <section aria-labelledby="problem-title" className="cq-problem">
      <div className="mx-auto w-full max-w-7xl px-6 py-20 sm:px-10 lg:px-14 lg:py-24">
        <div className="mx-auto max-w-3xl text-center">
          <p className="cq-section-eyebrow">The Problem</p>
          <h2 id="problem-title">
            The final answer isn't the whole story.
          </h2>
          <p>
            A correct final answer can hide the student's actual learning
            process.
          </p>
        </div>

        <div className="cq-compare-shell">
          <div
            aria-label="Compare assignment views"
            className="cq-compare-toggle"
            role="tablist"
          >
            <button
              aria-selected={mode === 'traditional'}
              className={mode === 'traditional' ? 'is-active' : ''}
              onClick={() => setMode('traditional')}
              role="tab"
              type="button"
            >
              <ClipboardCheck aria-hidden="true" className="size-4" />
              Traditional
            </button>
            <button
              aria-selected={isClassQuest}
              className={isClassQuest ? 'is-active' : ''}
              onClick={() => setMode('classquest')}
              role="tab"
              type="button"
            >
              <GraduationCap aria-hidden="true" className="size-4" />
              ClassQuest
            </button>
          </div>

          <div
            className={`cq-compare-stage ${isClassQuest ? 'is-classquest' : 'is-traditional'}`}
          >
            <div className="cq-compare-panel cq-traditional-panel">
              <div className="cq-panel-glow" aria-hidden="true" />
              <div className="cq-traditional-paper">
                <div className="cq-paper-header">
                  <div>
                    <p>Traditional Assignment</p>
                    <h3>Question completed.</h3>
                  </div>
                  <ClipboardCheck aria-hidden="true" className="size-6" />
                </div>

                <div className="cq-score-card">
                  <span>Teacher sees</span>
                  <strong>8 / 10</strong>
                </div>

                <div className="cq-nothing-else">
                  <EyeOff aria-hidden="true" className="size-5" />
                  <span>Nothing else.</span>
                </div>
              </div>
            </div>

            <div className="cq-compare-panel cq-classquest-panel">
              <div className="cq-panel-glow" aria-hidden="true" />
              <div className="cq-insight-console">
                <div className="cq-insight-header">
                  <div>
                    <p>ClassQuest Assignment</p>
                    <h3>Python range() mission</h3>
                  </div>
                  <BarChart3 aria-hidden="true" className="size-6" />
                </div>

                <div className="cq-mini-code">
                  <span>What is the output?</span>
                  <code>{`for i in range(1, 4):\n    print(i)`}</code>
                </div>

                <div className="cq-attempt-grid">
                  <article className="cq-attempt-card is-incorrect">
                    <div>
                      <p>Attempt 1</p>
                      <strong>1 2 3 4</strong>
                    </div>
                    <span>
                      <XCircle aria-hidden="true" className="size-4" />
                      Incorrect
                    </span>
                  </article>

                  <article className="cq-attempt-card is-correct">
                    <div>
                      <p>Attempt 2</p>
                      <strong>1 2 3</strong>
                    </div>
                    <span>
                      <CheckCircle2 aria-hidden="true" className="size-4" />
                      Correct
                    </span>
                  </article>
                </div>

                <div className="cq-learning-grid">
                  <div className="cq-learning-metric">
                    <Clock3 aria-hidden="true" className="size-4" />
                    <span>Time Taken</span>
                    <strong>29 sec</strong>
                  </div>
                  <div className="cq-learning-metric">
                    <AlertCircle aria-hidden="true" className="size-4" />
                    <span>Detected misconception</span>
                    <strong>
                      Student likely believes range() includes its stop value.
                    </strong>
                  </div>
                  <div className="cq-learning-metric">
                    <BarChart3 aria-hidden="true" className="size-4" />
                    <span>Concept Mastery</span>
                    <strong>62%</strong>
                    <div className="cq-mastery-bar" aria-hidden="true">
                      <span />
                    </div>
                  </div>
                  <div className="cq-learning-metric">
                    <Lightbulb aria-hidden="true" className="size-4" />
                    <span>Recommended Action</span>
                    <strong>Review range(start, stop).</strong>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <div className="cq-problem-takeaway">
            <span>{isClassQuest ? 'Learning process revealed' : 'Learning process hidden'}</span>
            <strong>
              {isClassQuest
                ? 'ClassQuest shows the path to the final answer.'
                : 'Traditional grading shows the score and loses the trail.'}
            </strong>
          </div>
        </div>
      </div>
    </section>
  )
}
