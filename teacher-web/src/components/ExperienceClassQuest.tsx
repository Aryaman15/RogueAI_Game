import {
  AlertTriangle,
  CheckCircle2,
  Cpu,
  DoorOpen,
  Power,
  RotateCcw,
  ShieldAlert,
  TerminalSquare,
  Zap,
} from 'lucide-react'
import { type FormEvent, useEffect, useRef, useState } from 'react'

import './experience-classquest.css'

type SimulationState = 'idle' | 'denied' | 'granted' | 'generator-online' | 'unlocked'

const correctAnswer = '1 2 3'

function normalizeAnswer(value: string) {
  return value.trim().replace(/[,\n\r\t]+/g, ' ').replace(/\s+/g, ' ')
}

export function ExperienceClassQuest() {
  const [answer, setAnswer] = useState('')
  const [simulationState, setSimulationState] = useState<SimulationState>('idle')
  const timeoutsRef = useRef<number[]>([])

  const isRunning =
    simulationState === 'granted' || simulationState === 'generator-online'
  const isComplete = simulationState === 'unlocked'
  const accessMessage =
    simulationState === 'denied'
      ? 'ACCESS DENIED'
      : simulationState === 'idle'
        ? 'AWAITING INPUT'
        : 'ACCESS GRANTED'

  const generatorOnline =
    simulationState === 'generator-online' || simulationState === 'unlocked'
  const doorUnlocked = simulationState === 'unlocked'

  useEffect(() => {
    return () => {
      timeoutsRef.current.forEach(window.clearTimeout)
    }
  }, [])

  function clearSequence() {
    timeoutsRef.current.forEach(window.clearTimeout)
    timeoutsRef.current = []
  }

  function runUnlockSequence() {
    clearSequence()
    setSimulationState('granted')

    timeoutsRef.current = [
      window.setTimeout(() => setSimulationState('generator-online'), 850),
      window.setTimeout(() => setSimulationState('unlocked'), 1750),
    ]
  }

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()

    if (isRunning) {
      return
    }

    if (normalizeAnswer(answer) === correctAnswer) {
      runUnlockSequence()
      return
    }

    clearSequence()
    setSimulationState('denied')
  }

  function handleReset() {
    clearSequence()
    setAnswer('')
    setSimulationState('idle')
  }

  return (
    <section
      aria-labelledby="experience-title"
      className="cq-experience cq-tech-grid"
    >
      <div className="mx-auto grid w-full max-w-7xl gap-8 px-6 py-20 sm:px-10 lg:grid-cols-[0.95fr_1.05fr] lg:px-14 lg:py-24">
        <form className="cq-terminal-panel" onSubmit={handleSubmit}>
          <div className="cq-panel-heading">
            <div>
              <p className="cq-section-eyebrow">Experience ClassQuest</p>
              <h2 id="experience-title">GENERATOR CONTROL TERMINAL</h2>
            </div>
            <TerminalSquare aria-hidden="true" className="size-6 text-cq-accent" />
          </div>

          <div className="cq-terminal-status">
            <AlertTriangle aria-hidden="true" className="size-5" />
            <div>
              <strong>POWER GRID OFFLINE</strong>
              <span>MANUAL OVERRIDE REQUIRED</span>
            </div>
          </div>

          <div className="cq-code-block">
            <div className="cq-code-bar">
              <span>assignment.challenge.py</span>
              <span>question 01</span>
            </div>
            <p>What is the output?</p>
            <pre>
              <code>{`for i in range(1, 4):\n    print(i)`}</code>
            </pre>
          </div>

          <label className="cq-answer-field">
            <span>Answer input</span>
            <input
              autoComplete="off"
              disabled={isRunning}
              onChange={(event) => setAnswer(event.target.value)}
              placeholder="Type 1 2 3"
              value={answer}
            />
          </label>

          <div className="cq-terminal-actions">
            <button
              className="cq-button cq-button-primary"
              disabled={isRunning}
              type="submit"
            >
              <Power aria-hidden="true" className="size-4" />
              Execute
            </button>
            {isComplete ? (
              <button
                className="cq-button cq-button-secondary"
                onClick={handleReset}
                type="button"
              >
                <RotateCcw aria-hidden="true" className="size-4" />
                Reset / try again
              </button>
            ) : null}
          </div>

          <div
            className={`cq-access-readout cq-access-readout-${simulationState}`}
            role="status"
          >
            {simulationState === 'denied' ? (
              <ShieldAlert aria-hidden="true" className="size-5" />
            ) : (
              <CheckCircle2 aria-hidden="true" className="size-5" />
            )}
            {accessMessage}
          </div>
        </form>

        <div
          className={`cq-world-schematic ${generatorOnline ? 'is-powered' : ''} ${
            doorUnlocked ? 'is-unlocked' : ''
          } ${simulationState === 'denied' ? 'is-denied' : ''}`}
          aria-label="Interactive game world status"
        >
          <div className="cq-schematic-grid" aria-hidden="true" />
          <div className="cq-power-spine" aria-hidden="true">
            <span />
          </div>

          <div className="cq-generator-module">
            <div className="cq-generator-ring">
              <span />
              <Cpu aria-hidden="true" className="size-12" />
            </div>
            <p>GENERATOR</p>
            <strong>{generatorOnline ? 'ONLINE' : 'OFFLINE'}</strong>
          </div>

          <div className="cq-door-module">
            <div className="cq-door-frame" aria-hidden="true">
              <span className="cq-door-panel cq-door-panel-left" />
              <span className="cq-door-panel cq-door-panel-right" />
              <span className="cq-door-lock">
                <Zap className="size-4" />
              </span>
            </div>
            <p>SECURITY DOOR</p>
            <strong>{doorUnlocked ? 'UNLOCKED' : 'LOCKED'}</strong>
          </div>

          <div className="cq-world-message">
            <p>{accessMessage}</p>
            {isComplete ? (
              <>
                <h3>
                  The assignment doesn't sit beside the game. It controls the
                  game world.
                </h3>
                <span>
                  Every answer can unlock a system, restore power, reveal
                  information or change what happens next.
                </span>
              </>
            ) : (
              <span>
                Execute the assignment output to restore power and unlock the
                next path.
              </span>
            )}
          </div>

          <DoorOpen aria-hidden="true" className="cq-door-ghost" />
        </div>
      </div>
    </section>
  )
}
