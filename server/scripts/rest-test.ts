const baseUrl = process.env.API_BASE_URL ?? "http://127.0.0.1:4000";

async function main() {
  const health = await request("GET", "/api/health");

  const mission = await request("POST", "/api/missions", {
    name: "HTTP Smoke Test Mission",
    className: "Grade 8",
    subject: "Computer Science",
    topic: "Python Basics",
    estimatedDuration: 15,
    worldId: "rogue-ai-headquarters",
    mapId: "power-sector",
    challenges: [
      {
        concept: "Python Loops",
        type: "predict-output",
        question: "What is printed?",
        codeSnippet: "for i in range(1, 3):\n    print(i)",
        expectedAnswer: "1 2",
      },
      {
        concept: "Variables",
        type: "short-answer",
        question: "What symbol assigns a value in Python?",
        expectedAnswer: "=",
      },
    ],
  });

  const gameMission = await request("GET", `/api/missions/code/${mission.code}`);
  const firstChallenge = gameMission.challenges[0];

  const attempt = await request("POST", "/api/attempts", {
    missionCode: gameMission.code,
    studentId: "student_http_smoke",
    studentName: "HTTP Smoke",
    challengeId: firstChallenge.id,
    slotId: firstChallenge.slotId,
    submittedAnswer: firstChallenge.expectedAnswer,
    correct: true,
    attemptNumber: 1,
    timeTakenSeconds: 12,
  });

  const report = await request("GET", `/api/missions/${mission.id}/report`);

  console.log(
    JSON.stringify(
      {
        health,
        missionCode: mission.code,
        gameMissionChallengeCount: gameMission.challenges.length,
        savedAttemptId: attempt.id,
        reportUniqueStudents: report.uniqueStudents,
        reportChallengePerformance: report.challengePerformance,
      },
      null,
      2,
    ),
  );
}

async function request(method: string, path: string, body?: unknown) {
  const response = await fetch(`${baseUrl}${path}`, {
    method,
    headers: body ? { "Content-Type": "application/json" } : undefined,
    body: body ? JSON.stringify(body) : undefined,
  });

  const json = await response.json();

  if (!response.ok) {
    throw new Error(`${method} ${path} failed with ${response.status}: ${JSON.stringify(json)}`);
  }

  return json;
}

main().catch((error) => {
  console.error(error);
  process.exit(1);
});
