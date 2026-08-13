# ClassQuest Server

Node.js, TypeScript, Express backend for ClassQuest mission publishing, Unity mission download, attempt submission, and teacher analytics.

## Commands

```bash
npm install
npm run build
npm start
```

The API listens on `0.0.0.0:4000` by default. Set `PORT` to change the port.

## Development Helpers

```bash
npm run seed
npm run test:http
```

`npm run seed` resets `data/store.json` with demo data, including mission code `CQ-DEMO`.

`npm run test:http` expects the server to be running and exercises:

1. `GET /api/health`
2. `POST /api/missions`
3. `GET /api/missions/code/:code`
4. `POST /api/attempts`
5. `GET /api/missions/:id/report`
