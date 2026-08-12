import type {
  Challenge,
  ClassGroup,
  ConceptMastery,
  LearningInsight,
  MapConfig,
  Mission,
  MissionResult,
  Student,
  StudentDiagnostic,
  Teacher,
  World,
} from './models'

export const teacher: Teacher = {
  id: 'teacher-1',
  name: 'Ananya Rao',
  subject: 'Computer Science',
}

export const classes: ClassGroup[] = [
  { id: 'class-xi-a', name: 'XI-A', grade: 'XI', studentCount: 32 },
  { id: 'class-xi-b', name: 'XI-B', grade: 'XI', studentCount: 29 },
]

export const worlds: World[] = [
  {
    id: 'rogue-ai-headquarters',
    name: 'Rogue AI Headquarters',
    genre: 'AI Lockdown - Sci-Fi Escape',
    availability: 'available',
    description:
      'A rogue AI has taken control of a futuristic research facility. Students restore systems, unlock secure areas and retrieve shutdown hardware by solving educational challenges.',
    maps: [
      {
        id: 'power-sector',
        name: 'Power Sector',
        mapConfigId: 'power-sector-config',
      },
    ],
  },
  {
    id: 'orbital-rescue',
    name: 'Orbital Rescue',
    genre: 'Space Survival',
    availability: 'coming-soon',
    description: 'A future ClassQuest world concept for survival systems and space missions.',
    maps: [],
  },
  {
    id: 'digital-detective',
    name: 'Digital Detective',
    genre: 'Mystery Investigation',
    availability: 'coming-soon',
    description: 'A future ClassQuest world concept for evidence, logic and deduction.',
    maps: [],
  },
  {
    id: 'lost-temple',
    name: 'Lost Temple',
    genre: 'Adventure Puzzle',
    availability: 'coming-soon',
    description: 'A future ClassQuest world concept for exploration and puzzle chains.',
    maps: [],
  },
]

export const mapConfigs: MapConfig[] = [
  {
    id: 'power-sector-config',
    worldId: 'rogue-ai-headquarters',
    name: 'Power Sector',
    maxChallenges: 4,
    challengeSlots: [
      {
        id: 'generator-terminal',
        displayName: 'Generator Control',
        gameAction: 'Restore sector power',
        order: 1,
      },
      {
        id: 'security-terminal',
        displayName: 'Security Override',
        gameAction: 'Unlock secured area',
        order: 2,
      },
      {
        id: 'power-module-terminal',
        displayName: 'Power Module Access',
        gameAction: 'Retrieve shutdown hardware',
        order: 3,
      },
      {
        id: 'exit-terminal',
        displayName: 'Exit Authorization',
        gameAction: 'Complete Power Sector',
        order: 4,
      },
    ],
  },
]

export const baseChallenges: Challenge[] = [
  {
    challengeId: 'challenge-loop-range',
    concept: 'Python Loops',
    type: 'Predict Output',
    question: 'What is the output?',
    codeSnippet: 'for i in range(1, 4):\n    print(i)',
    expectedAnswer: '1 2 3',
  },
  {
    challengeId: 'challenge-if-power',
    concept: 'Conditions',
    type: 'Multiple Choice',
    question: 'Which condition keeps the power module online?',
    options: ['power > 0', 'power = 0', 'power < 0', 'power == offline'],
    expectedAnswer: 'power > 0',
  },
  {
    challengeId: 'challenge-nested-loop',
    concept: 'Nested Loops',
    type: 'Predict Output',
    question: 'How many times will the alert print?',
    codeSnippet: 'for row in range(2):\n    for col in range(3):\n        print("alert")',
    expectedAnswer: '6',
  },
]

const baseMappings = baseChallenges.map((challenge, index) => ({
  challengeId: challenge.challengeId,
  slotId: mapConfigs[0].challengeSlots[index].id,
  order: index + 1,
}))

export const missions: Mission[] = [
  {
    id: 'mission-python-loops',
    missionCode: 'CQ-7X42',
    name: 'Python Loops Revision',
    classId: 'class-xi-a',
    className: 'XI-A',
    subject: 'Computer Science',
    topic: 'Python - Loops',
    estimatedDuration: '15 minutes',
    worldId: 'rogue-ai-headquarters',
    worldName: 'Rogue AI Headquarters',
    mapId: 'power-sector',
    mapName: 'Power Sector',
    status: 'published',
    challengeIds: baseChallenges.map((challenge) => challenge.challengeId),
    challenges: baseChallenges,
    mappings: baseMappings,
    createdAt: '2026-08-13T04:00:00.000Z',
  },
  {
    id: 'mission-sql-basics',
    missionCode: 'CQ-4Q91',
    name: 'SQL Basics',
    classId: 'class-xi-a',
    className: 'XI-A',
    subject: 'Computer Science',
    topic: 'SQL SELECT',
    estimatedDuration: '20 minutes',
    worldId: 'rogue-ai-headquarters',
    worldName: 'Rogue AI Headquarters',
    mapId: 'power-sector',
    mapName: 'Power Sector',
    status: 'published',
    challengeIds: ['challenge-sql-select'],
    challenges: [],
    mappings: [],
    createdAt: '2026-08-05T04:00:00.000Z',
  },
  {
    id: 'mission-python-conditions',
    missionCode: 'CQ-8M20',
    name: 'Python Conditions',
    classId: 'class-xi-b',
    className: 'XI-B',
    subject: 'Computer Science',
    topic: 'Python - Conditions',
    estimatedDuration: '18 minutes',
    worldId: 'rogue-ai-headquarters',
    worldName: 'Rogue AI Headquarters',
    mapId: 'power-sector',
    mapName: 'Power Sector',
    status: 'published',
    challengeIds: ['challenge-conditions'],
    challenges: [],
    mappings: [],
    createdAt: '2026-08-01T04:00:00.000Z',
  },
  {
    id: 'mission-lists-revision',
    missionCode: 'CQ-2L81',
    name: 'Lists Revision',
    classId: 'class-xi-a',
    className: 'XI-A',
    subject: 'Computer Science',
    topic: 'Python - Lists',
    estimatedDuration: '16 minutes',
    worldId: 'rogue-ai-headquarters',
    worldName: 'Rogue AI Headquarters',
    mapId: 'power-sector',
    mapName: 'Power Sector',
    status: 'published',
    challengeIds: ['challenge-lists'],
    challenges: [],
    mappings: [],
    createdAt: '2026-07-27T04:00:00.000Z',
  },
]

export const students: Student[] = [
  { id: 'student-riya-sharma', name: 'Riya Sharma', classId: 'class-xi-a' },
  { id: 'student-kabir-mehta', name: 'Kabir Mehta', classId: 'class-xi-a' },
  { id: 'student-aarav-singh', name: 'Aarav Singh', classId: 'class-xi-a' },
  { id: 'student-anika-iyer', name: 'Anika Iyer', classId: 'class-xi-a' },
  { id: 'student-dev-patel', name: 'Dev Patel', classId: 'class-xi-a' },
  { id: 'student-meera-nair', name: 'Meera Nair', classId: 'class-xi-a' },
  { id: 'student-ishaan-roy', name: 'Ishaan Roy', classId: 'class-xi-a' },
  { id: 'student-zoya-khan', name: 'Zoya Khan', classId: 'class-xi-a' },
]

export const conceptMastery: ConceptMastery[] = [
  { concept: 'Variables', mastery: 92 },
  { concept: 'Conditions', mastery: 79 },
  { concept: 'Loops', mastery: 56 },
  { concept: 'Nested Loops', mastery: 63 },
]

export const learningInsights: LearningInsight[] = [
  {
    id: 'insight-range-stop',
    title: 'Detected Learning Gap',
    evidence: '11 students included the stop value when evaluating range(1, 4).',
    misconception: 'Students believe Python range() includes its stop value.',
    recommendedAction:
      'Review range(start, stop) using a number-line or trace-table example before the next assignment.',
    severity: 'high',
    affectedStudents: 11,
  },
  {
    id: 'insight-loop-count',
    title: 'Secondary Pattern',
    evidence: '7 students counted printed lines correctly but could not explain the loop boundary.',
    misconception: 'Students may be memorizing output patterns without tracing iteration state.',
    recommendedAction:
      'Ask students to write the value of i beside each output line during the next warm-up.',
    severity: 'medium',
    affectedStudents: 7,
  },
]

export const missionResults: MissionResult[] = [
  {
    missionId: 'mission-python-loops',
    studentId: 'student-riya-sharma',
    completion: 100,
    mastery: 54,
    attempts: 3,
    timeMinutes: 14,
    status: 'Needs Review',
    attemptHistory: [
      {
        id: 'attempt-riya-1',
        challengeId: 'challenge-loop-range',
        checkpoint: 'Generator Terminal',
        question: 'What is the output?',
        codeSnippet: 'for i in range(1,4):\n    print(i)',
        answer: '1 2 3 4',
        isCorrect: false,
        timeSeconds: 18,
      },
      {
        id: 'attempt-riya-2',
        challengeId: 'challenge-loop-range',
        checkpoint: 'Generator Terminal',
        question: 'What is the output?',
        codeSnippet: 'for i in range(1,4):\n    print(i)',
        answer: '1 2 3',
        isCorrect: true,
        timeSeconds: 11,
      },
    ],
  },
  { missionId: 'mission-python-loops', studentId: 'student-kabir-mehta', completion: 100, mastery: 47, attempts: 4, timeMinutes: 17, status: 'Attention', attemptHistory: [] },
  { missionId: 'mission-python-loops', studentId: 'student-aarav-singh', completion: 100, mastery: 82, attempts: 1, timeMinutes: 9, status: 'Strong', attemptHistory: [] },
  { missionId: 'mission-python-loops', studentId: 'student-anika-iyer', completion: 100, mastery: 74, attempts: 2, timeMinutes: 12, status: 'On Track', attemptHistory: [] },
  { missionId: 'mission-python-loops', studentId: 'student-dev-patel', completion: 75, mastery: 61, attempts: 2, timeMinutes: 13, status: 'Needs Review', attemptHistory: [] },
  { missionId: 'mission-python-loops', studentId: 'student-meera-nair', completion: 100, mastery: 88, attempts: 1, timeMinutes: 8, status: 'Strong', attemptHistory: [] },
  { missionId: 'mission-python-loops', studentId: 'student-ishaan-roy', completion: 75, mastery: 58, attempts: 3, timeMinutes: 15, status: 'Needs Review', attemptHistory: [] },
  { missionId: 'mission-python-loops', studentId: 'student-zoya-khan', completion: 100, mastery: 69, attempts: 2, timeMinutes: 11, status: 'On Track', attemptHistory: [] },
]

export const missionPerformance = {
  'mission-python-loops': {
    completionLabel: '24 / 32',
    completed: 24,
    total: 32,
    averageScore: 73,
    averageMastery: 71,
    averageAttempts: 1.8,
    averageTime: '12m 41s',
    completionPercent: 75,
  },
  'mission-sql-basics': {
    completionPercent: 96,
    averageMastery: 84,
  },
  'mission-python-conditions': {
    completionPercent: 88,
    averageMastery: 76,
  },
  'mission-lists-revision': {
    completionPercent: 91,
    averageMastery: 81,
  },
} as const

export const studentDiagnostics: StudentDiagnostic[] = [
  {
    studentId: 'student-riya-sharma',
    overallMastery: 68,
    assignmentsCompleted: '7 / 8',
    currentTrend: '+9%',
    skillBreakdown: [
      { concept: 'Variables', mastery: 94 },
      { concept: 'Conditions', mastery: 81 },
      { concept: 'Loops', mastery: 52 },
      { concept: 'Lists', mastery: 77 },
      { concept: 'SQL SELECT', mastery: 90 },
      { concept: 'SQL WHERE', mastery: 63 },
    ],
    pattern: 'Likely assumes that range() includes the stop boundary.',
    recommendedAction: 'Review exclusive upper bounds.',
  },
]
