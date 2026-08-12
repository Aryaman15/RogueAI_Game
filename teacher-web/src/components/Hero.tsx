import {
  ArrowRight,
  BarChart3,
  BookOpenCheck,
  BrainCircuit,
  DoorOpen,
  Gamepad2,
  Play,
  ShieldCheck,
  Sparkles,
  TerminalSquare,
  Zap,
} from 'lucide-react'
import { type PointerEvent, useRef } from 'react'

import './hero.css'

const pipelineSteps = [
  {
    label: 'Assignment',
    icon: BookOpenCheck,
  },
  {
    label: 'Game Challenge',
    icon: Gamepad2,
  },
  {
    label: 'World Changes',
    icon: DoorOpen,
  },
  {
    label: 'Learning Intelligence',
    icon: BrainCircuit,
  },
] as const

export function Hero() {
  const visualRef = useRef<HTMLDivElement>(null)

  function handlePointerMove(event: PointerEvent<HTMLDivElement>) {
    if (!window.matchMedia('(pointer: fine)').matches) {
      return
    }

    const bounds = event.currentTarget.getBoundingClientRect()
    const x = (event.clientX - bounds.left) / bounds.width - 0.5
    const y = (event.clientY - bounds.top) / bounds.height - 0.5

    visualRef.current?.style.setProperty('--hero-pointer-x', x.toFixed(4))
    visualRef.current?.style.setProperty('--hero-pointer-y', y.toFixed(4))
  }

  function handlePointerLeave() {
    visualRef.current?.style.setProperty('--hero-pointer-x', '0')
    visualRef.current?.style.setProperty('--hero-pointer-y', '0')
  }

  return (
    <section
      aria-labelledby="hero-title"
      className="cq-hero cq-tech-grid cq-radial-glow"
    >
      <div className="cq-learning-lines" aria-hidden="true" />
      <div className="mx-auto grid min-h-screen w-full max-w-7xl items-center gap-12 px-6 py-20 sm:px-10 lg:grid-cols-[0.92fr_1.08fr] lg:px-14 lg:py-24">
        <div className="max-w-2xl">
          <div className="mb-6 inline-flex items-center gap-2 rounded-cq-sm border border-cq-border bg-cq-surface/90 px-3 py-2 text-sm font-medium text-cq-accent shadow-cq-soft">
            <Sparkles aria-hidden="true" className="size-4" />
            ClassQuest
          </div>

          <h1
            className="max-w-3xl text-5xl font-semibold leading-[1.02] text-cq-text-strong sm:text-6xl lg:text-7xl"
            id="hero-title"
          >
            Turn assignments into adventures.
          </h1>

          <p className="mt-6 max-w-xl text-2xl font-medium leading-snug text-cq-text-strong sm:text-3xl">
            Students play the assignment.
            <br />
            You understand how they learn.
          </p>

          <p className="mt-6 max-w-xl text-base leading-8 text-cq-text-muted sm:text-lg">
            ClassQuest transforms teacher-created assignments into interactive
            game missions while tracking how students actually solve them, not
            just the final answer.
          </p>

          <div className="mt-9 flex flex-col gap-3 sm:flex-row">
            <button className="cq-button cq-button-primary" type="button">
              Create your first mission
              <ArrowRight aria-hidden="true" className="size-4" />
            </button>
            <button className="cq-button cq-button-secondary" type="button">
              <Play aria-hidden="true" className="size-4" />
              Experience ClassQuest
            </button>
          </div>
        </div>

        <div
          className="cq-product-visual"
          onPointerLeave={handlePointerLeave}
          onPointerMove={handlePointerMove}
          ref={visualRef}
        >
          <div className="cq-orchestration" aria-label="Assignment transformed into learning intelligence">
            <div className="cq-world-core" aria-hidden="true">
              <span />
              <span />
            </div>

            <div className="cq-layer cq-assignment-card">
              <div className="cq-card-header">
                <div>
                  <p className="cq-kicker">Teacher Assignment</p>
                  <h2>Fractions: unlock the security door</h2>
                </div>
                <BookOpenCheck aria-hidden="true" className="size-5 text-cq-accent" />
              </div>
              <p className="mt-5 text-sm leading-6 text-cq-text-muted">
                What is <span className="font-mono text-cq-text-strong">3/4 + 1/8</span>?
              </p>
              <div className="mt-4 grid grid-cols-2 gap-2 text-sm">
                <span className="cq-answer-option">6/12</span>
                <span className="cq-answer-option cq-answer-option-correct">7/8</span>
              </div>
            </div>

            <div className="cq-layer cq-terminal-card">
              <div className="cq-terminal-top">
                <TerminalSquare aria-hidden="true" className="size-4 text-cq-accent" />
                <span>Generator Terminal</span>
              </div>
              <div className="mt-4 space-y-3 font-mono text-xs leading-5 text-cq-text-muted">
                <p>
                  <span className="text-cq-accent">mission.compile</span>
                  {'  '}assignment_id=CQ-108
                </p>
                <p>
                  status{' '}
                  <span className="rounded-cq-xs bg-cq-accent/10 px-2 py-1 text-cq-accent">
                    puzzle armed
                  </span>
                </p>
                <p>rule: evidence before answer</p>
              </div>
            </div>

            <div className="cq-layer cq-world-card">
              <div className="flex items-center justify-between gap-4">
                <div>
                  <p className="cq-kicker">Rogue AI Headquarters</p>
                  <h2>World reacts to reasoning</h2>
                </div>
                <ShieldCheck aria-hidden="true" className="size-5 text-cq-warning" />
              </div>
              <div className="mt-5 grid gap-3">
                <div className="cq-status-row">
                  <span>Generator</span>
                  <strong>78% restored</strong>
                </div>
                <div className="cq-status-row">
                  <span>Security Door</span>
                  <strong>requires retry</strong>
                </div>
              </div>
            </div>

            <div className="cq-layer cq-insight-card">
              <div className="flex items-start justify-between gap-4">
                <div>
                  <p className="cq-kicker">Teacher Intelligence</p>
                  <h2>Concept struggle detected</h2>
                </div>
                <BarChart3 aria-hidden="true" className="size-5 text-cq-accent-blue" />
              </div>
              <div className="mt-5 space-y-4">
                <div>
                  <div className="cq-meter-label">
                    <span>Common denominator</span>
                    <span>high friction</span>
                  </div>
                  <div className="cq-meter">
                    <span className="w-[72%]" />
                  </div>
                </div>
                <div>
                  <div className="cq-meter-label">
                    <span>Time to first strategy</span>
                    <span>2m 14s</span>
                  </div>
                  <div className="cq-meter cq-meter-blue">
                    <span className="w-[48%]" />
                  </div>
                </div>
              </div>
            </div>

            <div className="cq-layer cq-flow-card" aria-hidden="true">
              {pipelineSteps.map((step, index) => {
                const Icon = step.icon

                return (
                  <div className="cq-flow-step" key={step.label}>
                    <Icon className="size-4" />
                    <span>{step.label}</span>
                    {index < pipelineSteps.length - 1 ? (
                      <ArrowRight className="cq-flow-arrow size-4" />
                    ) : null}
                  </div>
                )
              })}
            </div>

            <div className="cq-layer cq-pulse-chip" aria-hidden="true">
              <Zap className="size-4" />
              Live attempt stream
            </div>
          </div>
        </div>
      </div>
    </section>
  )
}
