import { seedService } from "../src/services/serviceContext";

async function main() {
  const result = await seedService.resetWithDemoData();
  console.log(
    JSON.stringify(
      {
        status: "reset",
        missions: result.missions.length,
        students: result.students.length,
        attempts: result.attempts.length,
        demoMissionCode: "CQ-DEMO",
      },
      null,
      2,
    ),
  );
}

main().catch((error) => {
  console.error(error);
  process.exit(1);
});
