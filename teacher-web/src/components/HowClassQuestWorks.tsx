import {
  BarChart3,
  BrainCircuit,
  ClipboardPlus,
  Gamepad2,
  RadioTower,
  Route,
  Sparkles,
} from 'lucide-react'

import './how-classquest-works.css'

const journeyStages = [
  {
    id: '01',
    title: 'Create',
    bridge: 'Mission deployed',
    description: 'Teacher creates an assignment and chooses a game world.',
    icon: ClipboardPlus,
    detail: 'Assignment + world',
  },
  {
    id: '02',
    title: 'Play',
    bridge: 'Gameplay events',
    description: 'Students complete the assignment as a mission inside the game.',
    icon: Gamepad2,
    detail: 'Answers drive events',
  },
  {
    id: '03',
    title: 'Understand',
    bridge: 'Learning intelligence',
    description: 'ClassQuest analyzes attempts, mistakes and concept mastery.',
    icon: BrainCircuit,
    detail: 'Teacher insight',
  },
] as const

export function HowClassQuestWorks() {
  return (
    <section aria-labelledby="how-title" className="cq-how cq-tech-grid">
      <div className="mx-auto w-full max-w-7xl px-6 py-20 sm:px-10 lg:px-14 lg:py-24">
        <div className="max-w-3xl">
          <p className="cq-section-eyebrow">How ClassQuest Works</p>
          <h2 id="how-title">One assignment. One mission loop.</h2>
          <p>
            Create the learning task, let students play through it, then see
            what their choices reveal.
          </p>
        </div>

        <div className="cq-journey" aria-label="How ClassQuest works">
          <div className="cq-journey-track" aria-hidden="true">
            <span />
          </div>

          {journeyStages.map((stage, index) => {
            const Icon = stage.icon

            return (
              <article className="cq-journey-stage" key={stage.id}>
                <div className="cq-stage-node">
                  <span>{stage.id}</span>
                  <Icon aria-hidden="true" className="size-6" />
                </div>

                <div className="cq-stage-body">
                  <div className="cq-stage-title-row">
                    <h3>{stage.title}</h3>
                    <span>{stage.detail}</span>
                  </div>
                  <p>{stage.description}</p>
                </div>

                {index < journeyStages.length - 1 ? (
                  <div className="cq-stage-bridge" aria-hidden="true">
                    <RadioTower className="size-4" />
                    <span>{stage.bridge}</span>
                  </div>
                ) : (
                  <div className="cq-stage-bridge cq-stage-bridge-final" aria-hidden="true">
                    <BarChart3 className="size-4" />
                    <span>{stage.bridge}</span>
                  </div>
                )}
              </article>
            )
          })}

          <div className="cq-journey-core" aria-hidden="true">
            <Route className="size-7" />
            <span />
            <Sparkles className="size-5" />
          </div>
        </div>
      </div>
    </section>
  )
}
