import {
  ArrowLeft,
  ArrowRight,
  CheckCircle2,
  Copy,
  Edit3,
  Plus,
  Rocket,
  Sparkles,
  Trash2,
} from 'lucide-react'
import {
  type ChangeEvent,
  type FormEvent,
  useEffect,
  useMemo,
  useRef,
  useState,
} from 'react'
import { Link, useNavigate } from 'react-router-dom'

import { getSlotForMapping, mapChallengesToMission } from '../data/missionMapper'
import { publishMission } from '../data/missionStore'
import { classes, mapConfigs, worlds } from '../data/mockData'
import type { Challenge, ChallengeType, Mission, MissionMapping } from '../data/models'

const steps = [
  'Mission Details',
  'Choose World',
  'Add Educational Questions',
  'Review & Publish',
] as const

const buildSequence = [
  'Analyzing learning objectives...',
  'Matching challenges to game systems...',
  'Balancing mission progression...',
  'Mapping challenges across Power Sector...',
] as const

const challengeTypes: ChallengeType[] = [
  'Predict Output',
  'Multiple Choice',
  'Text Answer',
  'Debug Code',
  'SQL Query',
]

const defaultChallenge: Challenge = {
  challengeId: 'challenge-loop-range',
  concept: 'Python Loops',
  type: 'Predict Output',
  question: 'What is the output?',
  codeSnippet: 'for i in range(1, 4):\n    print(i)',
  options: [],
  expectedAnswer: '1 2 3',
}

const initialDetails = {
  name: 'Python Loops Revision',
  className: 'XI-A',
  subject: 'Computer Science',
  topic: 'Python - Loops',
  estimatedDuration: '15 minutes',
}

type BuildState = 'idle' | 'building' | 'ready'

function createEmptyQuestion(): Challenge {
  return {
    challengeId: `question-${Date.now()}`,
    concept: '',
    type: 'Predict Output',
    question: '',
    codeSnippet: '',
    options: [],
    expectedAnswer: '',
  }
}

export function CreateMissionWizard() {
  const navigate = useNavigate()
  const timeoutRef = useRef<number[]>([])
  const availableWorld = worlds.find((world) => world.availability === 'available')!
  const availableMap = availableWorld.maps[0]

  const [stepIndex, setStepIndex] = useState(0)
  const [details, setDetails] = useState(initialDetails)
  const [selectedWorldId, setSelectedWorldId] = useState(availableWorld.id)
  const [selectedMapId, setSelectedMapId] = useState(availableMap.id)
  const [challenges, setChallenges] = useState<Challenge[]>([defaultChallenge])
  const [challengeDraft, setChallengeDraft] = useState<Challenge>(createEmptyQuestion())
  const [editingIndex, setEditingIndex] = useState<number | null>(null)
  const [mappings, setMappings] = useState<MissionMapping[]>([])
  const [buildState, setBuildState] = useState<BuildState>('idle')
  const [buildStage, setBuildStage] = useState(-1)
  const [publishedMission, setPublishedMission] = useState<Mission | null>(null)
  const [copied, setCopied] = useState(false)

  const selectedWorld = worlds.find((world) => world.id === selectedWorldId) ?? availableWorld
  const selectedMap =
    selectedWorld.maps.find((map) => map.id === selectedMapId) ?? availableMap
  const mapConfig =
    mapConfigs.find((config) => config.id === selectedMap.mapConfigId) ?? mapConfigs[0]
  const canAddQuestion = challenges.length < mapConfig.maxChallenges || editingIndex !== null

  const mappingPreview = mappings.map((mapping) => ({
    mapping,
    challenge: challenges.find((challenge) => challenge.challengeId === mapping.challengeId),
    slot: getSlotForMapping(mapConfig, mapping),
  }))

  const canContinue = useMemo(() => {
    if (stepIndex === 0) {
      return Object.values(details).every((value) => value.trim())
    }

    if (stepIndex === 2) {
      return challenges.length > 0 && buildState === 'ready' && mappings.length > 0
    }

    return true
  }, [buildState, challenges.length, details, mappings.length, stepIndex])

  useEffect(() => {
    return () => {
      timeoutRef.current.forEach(window.clearTimeout)
    }
  }, [])

  function resetBuild() {
    timeoutRef.current.forEach(window.clearTimeout)
    timeoutRef.current = []
    setMappings([])
    setBuildState('idle')
    setBuildStage(-1)
  }

  function updateDetails(event: ChangeEvent<HTMLInputElement | HTMLSelectElement>) {
    const { name, value } = event.target
    setDetails((current) => ({ ...current, [name]: value }))
  }

  function updateChallenge(
    event: ChangeEvent<HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement>,
  ) {
    const { name, value } = event.target
    setChallengeDraft((current) => ({
      ...current,
      [name]: name === 'options' ? value.split('\n').filter(Boolean) : value,
    }))
  }

  function handleChallengeSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()

    if (!canAddQuestion) {
      return
    }

    const normalizedChallenge: Challenge = {
      ...challengeDraft,
      challengeId:
        editingIndex === null
          ? `question-${Date.now()}`
          : challenges[editingIndex].challengeId,
    }

    if (editingIndex === null) {
      setChallenges((current) => [...current, normalizedChallenge])
    } else {
      setChallenges((current) =>
        current.map((challenge, index) =>
          index === editingIndex ? normalizedChallenge : challenge,
        ),
      )
    }

    setChallengeDraft(createEmptyQuestion())
    setEditingIndex(null)
    resetBuild()
  }

  function editChallenge(index: number) {
    setChallengeDraft(challenges[index])
    setEditingIndex(index)
  }

  function removeChallenge(index: number) {
    setChallenges((current) => current.filter((_, itemIndex) => itemIndex !== index))
    setChallengeDraft(createEmptyQuestion())
    setEditingIndex(null)
    resetBuild()
  }

  function buildMission() {
    if (challenges.length === 0 || challenges.length > mapConfig.maxChallenges) {
      return
    }

    resetBuild()
    setBuildState('building')

    timeoutRef.current = buildSequence.map((_, index) =>
      window.setTimeout(() => setBuildStage(index), 450 + index * 520),
    )

    timeoutRef.current.push(
      window.setTimeout(() => {
        setMappings(mapChallengesToMission(mapConfig, challenges))
        setBuildStage(buildSequence.length)
        setBuildState('ready')
      }, 2800),
    )
  }

  function publishCurrentMission() {
    const mission = publishMission({
      ...details,
      worldId: selectedWorld.id,
      worldName: selectedWorld.name,
      mapId: selectedMap.id,
      mapName: selectedMap.name,
      challenges,
      mappings,
    })
    setPublishedMission(mission)
  }

  async function copyCode() {
    if (!publishedMission) {
      return
    }

    await navigator.clipboard?.writeText(publishedMission.missionCode)
    setCopied(true)
  }

  if (publishedMission) {
    return (
      <section className="cq-app-card cq-app-card-pad cq-deployed">
        <p className="cq-app-eyebrow">Mission Deployed</p>
        <h1 className="cq-app-title">{publishedMission.name}</h1>
        <p className="cq-app-subtitle mx-auto">
          {publishedMission.worldName} - {publishedMission.mapName}
        </p>

        <div className="cq-mission-code">{publishedMission.missionCode}</div>

        <div className="cq-action-row justify-center">
          <button className="cq-app-button" onClick={copyCode} type="button">
            {copied ? (
              <CheckCircle2 aria-hidden="true" className="size-4" />
            ) : (
              <Copy aria-hidden="true" className="size-4" />
            )}
            {copied ? 'Copied' : 'Copy Code'}
          </button>
          <button
            className="cq-app-button cq-app-button-primary"
            onClick={() => navigate(`/teacher/missions/${publishedMission.id}`)}
            type="button"
          >
            View Mission
            <ArrowRight aria-hidden="true" className="size-4" />
          </button>
        </div>
      </section>
    )
  }

  return (
    <>
      <header className="cq-app-header">
        <div>
          <p className="cq-app-eyebrow">Create Mission</p>
          <h1 className="cq-app-title">Turn an assignment into a mission</h1>
          <p className="cq-app-subtitle">
            Define what students should learn. ClassQuest handles where those
            questions appear inside the selected game world.
          </p>
        </div>
        <Link className="cq-app-link-button" to="/teacher">
          <ArrowLeft aria-hidden="true" className="size-4" />
          Command Center
        </Link>
      </header>

      <div className="cq-wizard-progress" aria-label="Mission creation progress">
        {steps.map((step, index) => (
          <div
            className={`cq-wizard-step ${
              index === stepIndex ? 'is-active' : index < stepIndex ? 'is-complete' : ''
            }`}
            key={step}
          >
            <p className="cq-mini-label">0{index + 1}</p>
            <strong>{step}</strong>
          </div>
        ))}
      </div>

      <section className="cq-app-card cq-app-card-pad">
        {stepIndex === 0 ? (
          <div>
            <div className="cq-card-heading">
              <div>
                <p className="cq-app-eyebrow">Step 01</p>
                <h2>Mission Details</h2>
              </div>
            </div>

            <div className="cq-form-grid">
              <label className="cq-form-label">
                Mission Name
                <input name="name" onChange={updateDetails} value={details.name} />
              </label>
              <label className="cq-form-label">
                Class
                <select name="className" onChange={updateDetails} value={details.className}>
                  {classes.map((classGroup) => (
                    <option key={classGroup.id} value={classGroup.name}>
                      {classGroup.name}
                    </option>
                  ))}
                </select>
              </label>
              <label className="cq-form-label">
                Subject
                <input name="subject" onChange={updateDetails} value={details.subject} />
              </label>
              <label className="cq-form-label">
                Topic
                <input name="topic" onChange={updateDetails} value={details.topic} />
              </label>
              <label className="cq-form-label">
                Estimated Duration
                <input
                  name="estimatedDuration"
                  onChange={updateDetails}
                  value={details.estimatedDuration}
                />
              </label>
            </div>
          </div>
        ) : null}

        {stepIndex === 1 ? (
          <div>
            <div className="cq-card-heading">
              <div>
                <p className="cq-app-eyebrow">Step 02</p>
                <h2>Choose World / Map</h2>
              </div>
            </div>

            <div className="cq-world-select-grid">
              {worlds.map((world) => {
                const disabled = world.availability !== 'available'
                const selected = world.id === selectedWorldId

                return (
                  <button
                    className={`cq-select-card ${selected ? 'is-selected' : ''} ${
                      disabled ? 'is-disabled' : ''
                    }`}
                    disabled={disabled}
                    key={world.id}
                    onClick={() => {
                      setSelectedWorldId(world.id)
                      setSelectedMapId(world.maps[0]?.id ?? '')
                      resetBuild()
                    }}
                    type="button"
                  >
                    <div className="flex items-start justify-between gap-3">
                      <span className="cq-pill">
                        {disabled ? 'Coming Soon' : 'Available'}
                      </span>
                      {selected ? <CheckCircle2 className="size-5 text-cq-accent" /> : null}
                    </div>
                    <h3 className="mt-5 text-2xl font-bold text-cq-text-strong">
                      {world.name}
                    </h3>
                    <p className="mt-2 text-sm font-bold uppercase tracking-[0.14em] text-cq-text-muted">
                      {world.genre}
                    </p>
                    <p className="mt-4 text-sm leading-6 text-cq-text-muted">
                      {world.description}
                    </p>
                  </button>
                )
              })}
            </div>

            <div className="mt-5 cq-app-card cq-app-card-pad">
              <div className="flex flex-wrap items-center justify-between gap-3">
                <div>
                  <p className="cq-mini-label">Selected Map</p>
                  <strong className="mt-2 block text-xl text-cq-text-strong">
                    {mapConfig.name}
                  </strong>
                </div>
                <span className="cq-pill cq-pill-accent">
                  Supports up to {mapConfig.maxChallenges} interactive challenges
                </span>
              </div>
              <p className="mt-3 text-cq-text-muted">
                ClassQuest will automatically place your questions into this map's
                internal mission systems.
              </p>
            </div>
          </div>
        ) : null}

        {stepIndex === 2 ? (
          <div>
            <div className="cq-card-heading">
              <div>
                <p className="cq-app-eyebrow">Step 03</p>
                <h2>Add Educational Questions</h2>
              </div>
              <span className="cq-pill cq-pill-accent">
                {challenges.length} / {mapConfig.maxChallenges} questions added
              </span>
            </div>

            <div className="cq-dashboard-grid">
              <div className="cq-challenge-list">
                {challenges.map((challenge, index) => (
                  <article className="cq-challenge-item" key={challenge.challengeId}>
                    <div>
                      <p className="cq-mini-label">
                        Question {String(index + 1).padStart(2, '0')} - {challenge.type}
                      </p>
                      <h3 className="mt-2 text-xl font-bold text-cq-text-strong">
                        {challenge.concept || 'Untitled concept'}
                      </h3>
                      <p className="mt-2 text-cq-text-muted">{challenge.question}</p>
                      {challenge.codeSnippet ? (
                        <pre className="cq-code-snippet mt-3">{challenge.codeSnippet}</pre>
                      ) : null}
                      <p className="mt-3 text-sm text-cq-accent">
                        Expected: {challenge.expectedAnswer}
                      </p>
                    </div>
                    <div className="flex gap-2">
                      <button className="cq-app-button" onClick={() => editChallenge(index)} type="button">
                        <Edit3 aria-hidden="true" className="size-4" />
                      </button>
                      <button className="cq-app-button" onClick={() => removeChallenge(index)} type="button">
                        <Trash2 aria-hidden="true" className="size-4" />
                      </button>
                    </div>
                  </article>
                ))}
              </div>

              <form className="cq-app-card cq-app-card-pad" onSubmit={handleChallengeSubmit}>
                <div className="cq-card-heading">
                  <div>
                    <p className="cq-app-eyebrow">
                      {editingIndex === null ? 'Add Question' : 'Edit Question'}
                    </p>
                    <h3>Learning challenge</h3>
                  </div>
                </div>
                <div className="grid gap-3">
                  <label className="cq-form-label">
                    Concept
                    <input name="concept" onChange={updateChallenge} value={challengeDraft.concept} />
                  </label>
                  <label className="cq-form-label">
                    Question Type
                    <select name="type" onChange={updateChallenge} value={challengeDraft.type}>
                      {challengeTypes.map((type) => (
                        <option key={type} value={type}>
                          {type}
                        </option>
                      ))}
                    </select>
                  </label>
                  <label className="cq-form-label">
                    Question
                    <textarea
                      name="question"
                      onChange={updateChallenge}
                      value={challengeDraft.question}
                    />
                  </label>
                  <label className="cq-form-label">
                    Code Snippet
                    <textarea
                      name="codeSnippet"
                      onChange={updateChallenge}
                      value={challengeDraft.codeSnippet ?? ''}
                    />
                  </label>
                  <label className="cq-form-label">
                    MCQ Options
                    <textarea
                      name="options"
                      onChange={updateChallenge}
                      placeholder="One option per line"
                      value={(challengeDraft.options ?? []).join('\n')}
                    />
                  </label>
                  <label className="cq-form-label">
                    Expected Answer
                    <input
                      name="expectedAnswer"
                      onChange={updateChallenge}
                      value={challengeDraft.expectedAnswer}
                    />
                  </label>
                  <button
                    className="cq-app-button cq-app-button-primary"
                    disabled={!canAddQuestion}
                    type="submit"
                  >
                    <Plus aria-hidden="true" className="size-4" />
                    {editingIndex === null ? 'Add Question' : 'Save Question'}
                  </button>
                  {!canAddQuestion ? (
                    <p className="text-sm leading-6 text-cq-warning">
                      Power Sector supports a maximum of {mapConfig.maxChallenges}{' '}
                      questions in this prototype.
                    </p>
                  ) : null}
                </div>
              </form>
            </div>

            {buildState === 'building' ? (
              <div className="cq-app-card cq-app-card-pad cq-build-panel mt-4">
                <p className="cq-app-eyebrow">Building your mission</p>
                <h3>ClassQuest is turning your assignment into Power Sector gameplay.</h3>
                <div className="cq-build-sequence">
                  {buildSequence.map((item, index) => (
                    <div className="cq-build-step" key={item}>
                      <span className={index <= buildStage ? 'is-complete' : ''}>
                        {index <= buildStage ? (
                          <CheckCircle2 aria-hidden="true" className="size-4" />
                        ) : (
                          <Sparkles aria-hidden="true" className="size-4" />
                        )}
                      </span>
                      {item}
                    </div>
                  ))}
                </div>
              </div>
            ) : null}

            {buildState === 'ready' ? (
              <div className="cq-app-card cq-app-card-pad cq-mapping-panel mt-4">
                <div className="cq-card-heading">
                  <div>
                    <p className="cq-app-eyebrow">Automatically mapped by ClassQuest</p>
                    <h3>Mission ready</h3>
                  </div>
                  <span className="cq-pill cq-pill-accent">Mapping complete</span>
                </div>

                <div className="cq-mapping-preview">
                  {mappingPreview.map(({ mapping, challenge, slot }) => (
                    <article className="cq-mapping-row" key={mapping.challengeId}>
                      <div>
                        <p className="cq-mini-label">
                          Question {String(mapping.order).padStart(2, '0')}
                        </p>
                        <strong>{challenge?.concept}</strong>
                      </div>
                      <ArrowRight aria-hidden="true" className="size-5 text-cq-accent" />
                      <div>
                        <p className="cq-mini-label">{slot?.displayName}</p>
                        <strong>{slot?.gameAction}</strong>
                      </div>
                    </article>
                  ))}
                </div>
              </div>
            ) : null}
          </div>
        ) : null}

        {stepIndex === 3 ? (
          <div>
            <div className="cq-card-heading">
              <div>
                <p className="cq-app-eyebrow">Step 04</p>
                <h2>Review & Publish</h2>
              </div>
            </div>

            <div className="cq-report-grid">
              <div>
                <div className="cq-form-grid">
                  {[
                    ['Mission Name', details.name],
                    ['Class', details.className],
                    ['World', selectedWorld.name],
                    ['Map', selectedMap.name],
                    ['Questions', String(challenges.length)],
                    ['Estimated Duration', details.estimatedDuration],
                  ].map(([label, value]) => (
                    <div className="cq-app-card cq-app-card-pad" key={label}>
                      <p className="cq-mini-label">{label}</p>
                      <strong className="mt-2 block text-cq-text-strong">{value}</strong>
                    </div>
                  ))}
                </div>
              </div>

              <div className="cq-app-card cq-app-card-pad">
                <p className="cq-app-eyebrow">Generated Mission Flow</p>
                <div className="cq-review-flow mt-4">
                  <div className="cq-flow-node">START</div>
                  {mappingPreview.map(({ mapping, challenge, slot }) => (
                    <div key={mapping.challengeId}>
                      <div className="cq-flow-arrow-down">v</div>
                      <div className="cq-flow-node">
                        <strong className="block text-cq-text-strong">
                          {slot?.displayName}
                        </strong>
                        <span className="mt-1 block text-sm text-cq-text-muted">
                          {challenge?.concept}
                        </span>
                      </div>
                    </div>
                  ))}
                </div>
              </div>
            </div>
          </div>
        ) : null}

        <div className="cq-action-row">
          {stepIndex > 0 ? (
            <button
              className="cq-app-button"
              onClick={() => setStepIndex((current) => current - 1)}
              type="button"
            >
              <ArrowLeft aria-hidden="true" className="size-4" />
              Back
            </button>
          ) : null}
          {stepIndex === 2 && buildState !== 'ready' ? (
            <button
              className="cq-app-button cq-app-button-primary"
              disabled={challenges.length === 0 || buildState === 'building'}
              onClick={buildMission}
              type="button"
            >
              <Sparkles aria-hidden="true" className="size-4" />
              {buildState === 'building' ? 'Building Mission' : 'Build Mission'}
            </button>
          ) : stepIndex < steps.length - 1 ? (
            <button
              className="cq-app-button cq-app-button-primary"
              disabled={!canContinue}
              onClick={() => setStepIndex((current) => current + 1)}
              type="button"
            >
              Continue
              <ArrowRight aria-hidden="true" className="size-4" />
            </button>
          ) : (
            <button
              className="cq-app-button cq-app-button-primary"
              disabled={mappings.length === 0}
              onClick={publishCurrentMission}
              type="button"
            >
              <Rocket aria-hidden="true" className="size-4" />
              Publish Mission
            </button>
          )}
        </div>
      </section>
    </>
  )
}
