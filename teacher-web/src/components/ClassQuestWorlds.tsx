import {
  ArrowRight,
  Binary,
  Compass,
  LockKeyhole,
  Orbit,
  RadioTower,
  ScanLine,
  ShieldCheck,
  Sparkles,
} from 'lucide-react'
import { type PointerEvent } from 'react'

import './classquest-worlds.css'

type World = {
  id: string
  title: string
  theme: string
  status: 'AVAILABLE' | 'COMING SOON'
  description: string
  icon: typeof LockKeyhole
  variant: 'rogue' | 'orbital' | 'detective' | 'temple'
}

const worlds: World[] = [
  {
    id: 'WORLD 01',
    title: 'ROGUE AI HEADQUARTERS',
    theme: 'AI Lockdown • Sci-Fi Escape',
    status: 'AVAILABLE',
    description:
      'Students restore systems, unlock secure sectors and retrieve shutdown hardware by solving educational challenges.',
    icon: LockKeyhole,
    variant: 'rogue',
  },
  {
    id: 'FUTURE WORLD',
    title: 'ORBITAL RESCUE',
    theme: 'Space Survival',
    status: 'COMING SOON',
    description:
      'A product-vision world for emergency repairs, resource choices and survival logic in orbit.',
    icon: Orbit,
    variant: 'orbital',
  },
  {
    id: 'FUTURE WORLD',
    title: 'DIGITAL DETECTIVE',
    theme: 'Mystery Investigation',
    status: 'COMING SOON',
    description:
      'A product-vision world for clues, evidence trails and deduction-driven learning missions.',
    icon: ScanLine,
    variant: 'detective',
  },
  {
    id: 'FUTURE WORLD',
    title: 'LOST TEMPLE',
    theme: 'Adventure Puzzle',
    status: 'COMING SOON',
    description:
      'A product-vision world for ancient mechanisms, exploration puzzles and challenge gates.',
    icon: Compass,
    variant: 'temple',
  },
]

const [featuredWorld, ...futureWorlds] = worlds

function handleWorldPointerMove(event: PointerEvent<HTMLElement>) {
  if (!window.matchMedia('(pointer: fine)').matches) {
    return
  }

  const bounds = event.currentTarget.getBoundingClientRect()
  const x = ((event.clientX - bounds.left) / bounds.width) * 100
  const y = ((event.clientY - bounds.top) / bounds.height) * 100

  event.currentTarget.style.setProperty('--world-spotlight-x', `${x.toFixed(2)}%`)
  event.currentTarget.style.setProperty('--world-spotlight-y', `${y.toFixed(2)}%`)
}

function handleWorldPointerLeave(event: PointerEvent<HTMLElement>) {
  event.currentTarget.style.setProperty('--world-spotlight-x', '50%')
  event.currentTarget.style.setProperty('--world-spotlight-y', '35%')
}

export function ClassQuestWorlds() {
  return (
    <section aria-labelledby="worlds-title" className="cq-worlds cq-tech-grid">
      <div className="mx-auto w-full max-w-7xl px-6 py-20 sm:px-10 lg:px-14 lg:py-24">
        <div className="grid gap-6 lg:grid-cols-[0.72fr_1fr] lg:items-end">
          <div>
            <p className="cq-section-eyebrow">ClassQuest Worlds</p>
            <h2 id="worlds-title">Choose a world. Build a mission.</h2>
          </div>
          <p className="cq-worlds-intro">
            ClassQuest supports multiple interactive game worlds, each turning
            assignments into missions with different stakes, systems and
            learning moments.
          </p>
        </div>

        <div className="cq-world-grid">
          <WorldCard featured world={featuredWorld} />
          <div className="cq-future-world-stack">
            {futureWorlds.map((world) => (
              <WorldCard key={world.title} world={world} />
            ))}
          </div>
        </div>

        <div className="cq-worlds-note">
          <Binary aria-hidden="true" className="size-4" />
          <span>
            Rogue AI Headquarters is currently the only playable ClassQuest
            world.
          </span>
          <Sparkles aria-hidden="true" className="size-4" />
        </div>
      </div>
    </section>
  )
}

function WorldCard({
  featured = false,
  world,
}: {
  featured?: boolean
  world: World
}) {
  const Icon = world.icon
  const isAvailable = world.status === 'AVAILABLE'

  return (
    <article
      aria-label={`${world.title}, ${world.status}`}
      className={`cq-world-card cq-world-card-${world.variant} ${
        isAvailable ? 'is-available' : 'is-coming-soon'
      } ${featured ? 'cq-world-card-featured' : ''}`}
      onPointerLeave={handleWorldPointerLeave}
      onPointerMove={handleWorldPointerMove}
      tabIndex={0}
    >
      <div className="cq-world-ambient" aria-hidden="true" />
      <div className="cq-world-visual" aria-hidden="true">
        <div className="cq-world-emblem">
          <Icon className="size-8" />
          <span />
        </div>
        <div className="cq-world-lines">
          <span />
          <span />
          <span />
        </div>
      </div>

      <div className="cq-world-card-content">
        <div className="cq-world-meta">
          <span>{world.id}</span>
          <strong>{world.status}</strong>
        </div>

        <div className="cq-world-copy">
          <h3>{world.title}</h3>
          <p className="cq-world-theme">{world.theme}</p>
          <p className="cq-world-description">{world.description}</p>
        </div>

        <div className="cq-world-footer">
          {isAvailable ? (
            <>
              <ShieldCheck aria-hidden="true" className="size-4" />
              <span>Explore world</span>
              <ArrowRight aria-hidden="true" className="size-4" />
            </>
          ) : (
            <>
              <RadioTower aria-hidden="true" className="size-4" />
              <span>Product vision only</span>
            </>
          )}
        </div>
      </div>
    </article>
  )
}
