import type { Attempt, Challenge, Mission } from "../models/types";
import type { AttemptRepository } from "../repositories/attemptRepository";
import type { MissionRepository } from "../repositories/missionRepository";
import { notFound } from "../utils/httpError";

interface ChallengePerformance {
  challengeId: string;
  slotId: string;
  order: number;
  concept: string;
  studentsAttempted: number;
  correctStudents: number;
  incorrectAttempts: number;
  averageAttempts: number;
  averageTime: number;
}

interface ConceptPerformance {
  concept: string;
  studentsAttempted: number;
  correctStudents: number;
  incorrectAttempts: number;
  averageAttempts: number;
  averageTime: number;
}

export class ReportService {
  constructor(
    private readonly missions: MissionRepository,
    private readonly attempts: AttemptRepository,
  ) {}

  async getMissionReport(missionId: string) {
    const mission = await this.missions.findById(missionId);

    if (!mission) {
      throw notFound(`Mission ${missionId} was not found`);
    }

    const attempts = await this.attempts.listByMissionId(mission.id);
    const uniqueStudentIds = unique(attempts.map((attempt) => attempt.studentId));
    const completedStudents = this.countCompletedStudents(mission, attempts);

    return {
      mission: {
        id: mission.id,
        code: mission.code,
        name: mission.name,
        className: mission.className,
        subject: mission.subject,
        topic: mission.topic,
      },
      uniqueStudents: uniqueStudentIds.length,
      completedStudents,
      averageAttempts: round(averageAttemptsPerStudent(attempts)),
      averageTime: round(average(attempts.map((attempt) => attempt.timeTakenSeconds))),
      challengePerformance: this.getChallengePerformance(mission, attempts),
      conceptPerformance: this.getConceptPerformance(mission, attempts),
      studentSummaries: this.getStudentSummaries(mission, attempts),
    };
  }

  async getStudentReport(studentId: string) {
    const attempts = await this.attempts.listByStudentId(studentId);
    const missions = await this.missions.list();
    const missionById = new Map(missions.map((mission) => [mission.id, mission]));

    return {
      studentId,
      studentName: attempts.at(-1)?.studentName ?? null,
      missionsAttempted: this.getStudentMissionSummaries(attempts, missionById),
      attempts,
      conceptLevelResults: this.getStudentConceptResults(attempts, missionById),
      challengeHistory: this.getChallengeHistory(attempts, missionById),
    };
  }

  private getChallengePerformance(mission: Mission, attempts: Attempt[]): ChallengePerformance[] {
    return [...mission.mappings]
      .sort((first, second) => first.order - second.order)
      .map((mapping) => {
        const challenge = mission.challenges.find((candidate) => candidate.id === mapping.challengeId);
        const challengeAttempts = attempts.filter((attempt) => attempt.challengeId === mapping.challengeId);

        return {
          challengeId: mapping.challengeId,
          slotId: mapping.slotId,
          order: mapping.order,
          concept: challenge?.concept ?? "Unknown",
          studentsAttempted: unique(challengeAttempts.map((attempt) => attempt.studentId)).length,
          correctStudents: unique(
            challengeAttempts.filter((attempt) => attempt.correct).map((attempt) => attempt.studentId),
          ).length,
          incorrectAttempts: challengeAttempts.filter((attempt) => !attempt.correct).length,
          averageAttempts: round(averageAttemptsPerStudent(challengeAttempts)),
          averageTime: round(average(challengeAttempts.map((attempt) => attempt.timeTakenSeconds))),
        };
      });
  }

  private getConceptPerformance(mission: Mission, attempts: Attempt[]): ConceptPerformance[] {
    const attemptsByConcept = new Map<string, Attempt[]>();

    for (const attempt of attempts) {
      const challenge = findChallenge(mission, attempt.challengeId);
      const concept = challenge?.concept ?? "Unknown";
      attemptsByConcept.set(concept, [...(attemptsByConcept.get(concept) ?? []), attempt]);
    }

    return [...attemptsByConcept.entries()].map(([concept, conceptAttempts]) => ({
      concept,
      studentsAttempted: unique(conceptAttempts.map((attempt) => attempt.studentId)).length,
      correctStudents: unique(conceptAttempts.filter((attempt) => attempt.correct).map((attempt) => attempt.studentId))
        .length,
      incorrectAttempts: conceptAttempts.filter((attempt) => !attempt.correct).length,
      averageAttempts: round(averageAttemptsPerStudent(conceptAttempts)),
      averageTime: round(average(conceptAttempts.map((attempt) => attempt.timeTakenSeconds))),
    }));
  }

  private getStudentSummaries(mission: Mission, attempts: Attempt[]) {
    return unique(attempts.map((attempt) => attempt.studentId)).map((studentId) => {
      const studentAttempts = attempts.filter((attempt) => attempt.studentId === studentId);
      const correctChallengeIds = unique(
        studentAttempts.filter((attempt) => attempt.correct).map((attempt) => attempt.challengeId),
      );
      const attemptedChallengeIds = unique(studentAttempts.map((attempt) => attempt.challengeId));

      return {
        studentId,
        studentName: studentAttempts.at(-1)?.studentName ?? "",
        attempts: studentAttempts.length,
        attemptedChallenges: attemptedChallengeIds.length,
        correctChallenges: correctChallengeIds.length,
        completed: correctChallengeIds.length === mission.challenges.length,
        totalTimeSeconds: sum(studentAttempts.map((attempt) => attempt.timeTakenSeconds)),
        lastAttemptAt: studentAttempts.at(-1)?.createdAt ?? null,
      };
    });
  }

  private getStudentMissionSummaries(attempts: Attempt[], missionById: Map<string, Mission>) {
    return unique(attempts.map((attempt) => attempt.missionId)).map((missionId) => {
      const mission = missionById.get(missionId);
      const missionAttempts = attempts.filter((attempt) => attempt.missionId === missionId);
      const correctChallengeIds = unique(
        missionAttempts.filter((attempt) => attempt.correct).map((attempt) => attempt.challengeId),
      );

      return {
        missionId,
        missionCode: missionAttempts[0]?.missionCode ?? mission?.code ?? "",
        missionName: mission?.name ?? "Unknown mission",
        attempts: missionAttempts.length,
        correctChallenges: correctChallengeIds.length,
        completed: mission ? correctChallengeIds.length === mission.challenges.length : false,
        totalTimeSeconds: sum(missionAttempts.map((attempt) => attempt.timeTakenSeconds)),
        lastAttemptAt: missionAttempts.at(-1)?.createdAt ?? null,
      };
    });
  }

  private getStudentConceptResults(attempts: Attempt[], missionById: Map<string, Mission>) {
    const attemptsByConcept = new Map<string, Attempt[]>();

    for (const attempt of attempts) {
      const mission = missionById.get(attempt.missionId);
      const challenge = mission ? findChallenge(mission, attempt.challengeId) : undefined;
      const concept = challenge?.concept ?? "Unknown";
      attemptsByConcept.set(concept, [...(attemptsByConcept.get(concept) ?? []), attempt]);
    }

    return [...attemptsByConcept.entries()].map(([concept, conceptAttempts]) => ({
      concept,
      attempts: conceptAttempts.length,
      correctAttempts: conceptAttempts.filter((attempt) => attempt.correct).length,
      incorrectAttempts: conceptAttempts.filter((attempt) => !attempt.correct).length,
      averageTime: round(average(conceptAttempts.map((attempt) => attempt.timeTakenSeconds))),
      lastAttemptAt: conceptAttempts.at(-1)?.createdAt ?? null,
    }));
  }

  private getChallengeHistory(attempts: Attempt[], missionById: Map<string, Mission>) {
    return attempts.map((attempt) => {
      const mission = missionById.get(attempt.missionId);
      const challenge = mission ? findChallenge(mission, attempt.challengeId) : undefined;

      return {
        missionId: attempt.missionId,
        missionCode: attempt.missionCode,
        missionName: mission?.name ?? "Unknown mission",
        challengeId: attempt.challengeId,
        slotId: attempt.slotId,
        concept: challenge?.concept ?? "Unknown",
        question: challenge?.question ?? null,
        submittedAnswer: attempt.submittedAnswer,
        correct: attempt.correct,
        attemptNumber: attempt.attemptNumber,
        timeTakenSeconds: attempt.timeTakenSeconds,
        createdAt: attempt.createdAt,
      };
    });
  }

  private countCompletedStudents(mission: Mission, attempts: Attempt[]): number {
    return unique(attempts.map((attempt) => attempt.studentId)).filter((studentId) => {
      const correctChallengeIds = unique(
        attempts
          .filter((attempt) => attempt.studentId === studentId && attempt.correct)
          .map((attempt) => attempt.challengeId),
      );

      return correctChallengeIds.length === mission.challenges.length;
    }).length;
  }
}

function findChallenge(mission: Mission, challengeId: string): Challenge | undefined {
  return mission.challenges.find((challenge) => challenge.id === challengeId);
}

function unique<T>(values: T[]): T[] {
  return [...new Set(values)];
}

function sum(values: number[]): number {
  return values.reduce((total, value) => total + value, 0);
}

function average(values: number[]): number {
  return values.length === 0 ? 0 : sum(values) / values.length;
}

function averageAttemptsPerStudent(attempts: Attempt[]): number {
  const studentCount = unique(attempts.map((attempt) => attempt.studentId)).length;
  return studentCount === 0 ? 0 : attempts.length / studentCount;
}

function round(value: number): number {
  return Math.round(value * 100) / 100;
}
