# Tic Tac Toe — Angular + .NET

A browser-based Tic Tac Toe app built for the Round 2 assessment: Angular frontend, .NET Web API backend, REST between them, backend owns all game/session/scoreboard state.

## Project overview

Two players (or one player vs. a rule-based computer) play Tic Tac Toe in the browser. The backend is the single source of truth: it validates every move, detects wins/draws, tracks move history, runs the computer's turn, and keeps a session-level scoreboard. The Angular frontend only renders whatever the backend last returned and never applies game rules itself.

## Tech stack

- **Frontend:** Angular 17 (standalone components), TypeScript, SCSS
- **Backend:** .NET 8 Web API (controllers, singleton in-memory services)
- **API style:** REST, JSON
- **Storage:** In-memory (`ConcurrentDictionary` for games, a locked singleton for the scoreboard) — resets when the backend process restarts, as allowed by the brief
- **Tests:** xUnit (backend), Jasmine/Karma (frontend)

## Features implemented

- 3×3 board, click-to-play, cells lock once filled
- Two Player mode and Vs. Computer mode (human is always X)
- Turn indicator, invalid moves rejected without changing the turn
- Row / column / diagonal win detection, winning cells highlighted, board locks on completion
- Draw detection
- Move history (move number, player, row/column), flags which moves were the computer's
- Undo — mode-aware (see Clarification 2 below)
- Session scoreboard (X wins / O wins / draws), served by the backend, with its own reset endpoint
- Reset Game (clears board/history/status, keeps scoreboard, keeps the same session)
- Computer opponent with the required move priority: win → block → center → corner → any cell

## How to run the backend locally

Requires the [.NET 8 SDK](https://dotnet.microsoft.com/download).

```bash
cd backend
dotnet run --project TicTacToe.Api
```

The API listens on `http://localhost:5000` (see `Properties/launchSettings.json`). Swagger UI is available at `http://localhost:5000/swagger` for exploring/testing endpoints directly.

## How to run the frontend locally

Requires [Node.js 18+](https://nodejs.org/) and npm.

```bash
cd frontend
npm install
npm start
```

This serves the app at `http://localhost:4200` and proxies API calls to `http://localhost:5000/api` (see `src/environments/environment.ts`). **Start the backend first.**

## How to run tests

**Backend (xUnit):**
```bash
cd backend
dotnet test
```

**Frontend (Karma/Jasmine):**
```bash
cd frontend
npm test
```
(Requires Chrome installed locally for the headless test runner.)

## API endpoint summary

| Method | Endpoint | Purpose |
|---|---|---|
| POST | `/api/games` | Create a new game session. Body: `{ "mode": "TwoPlayer" \| "VsComputer" }` |
| GET | `/api/games/{id}` | Get current game state |
| POST | `/api/games/{id}/moves` | Submit a move. Body: `{ "player": "X"\|"O", "cellIndex": 0-8 }` (or `row`/`column` instead of `cellIndex`) |
| POST | `/api/games/{id}/undo` | Undo the last move (or move pair — see below) |
| POST | `/api/games/{id}/reset` | Reset the current game session; scoreboard untouched |
| GET | `/api/scoreboard` | Get the session scoreboard |
| POST | `/api/scoreboard/reset` | Reset the scoreboard to zero |

**Game state response shape** (`GameStateResponse`):
```json
{
  "gameId": "abc123",
  "mode": "VsComputer",
  "board": ["X", null, "O", null, "X", null, null, null, null],
  "currentPlayer": "O",
  "status": "InProgress",
  "winner": null,
  "winningCells": null,
  "moveHistory": [
    { "moveNumber": 1, "player": "X", "cellIndex": 0, "row": 0, "column": 0, "wasComputerMove": false }
  ],
  "canUndo": true,
  "scoreboard": { "xWins": 0, "oWins": 0, "draws": 0 }
}
```
`canUndo` and the embedded `scoreboard` snapshot are conveniences for the frontend — they aren't required by the brief but avoid extra round trips.

The backend rejects (HTTP 400, `{ "message": "..." }`) any move that is out of range, on an occupied cell, made after the game is complete, or made by the player who isn't currently up. A missing game ID returns HTTP 404.

## Design decisions

- **Computer moves inline, in the same request.** When Vs. Computer mode is active and it becomes O's turn, the backend plays O's move automatically before returning — the frontend never has to poll or make a second call. This keeps "computer should move automatically after the human move" simple and atomic.
- **Undo replays history rather than reversing fields.** Undo trims the trailing move(s) off the history list and replays the remainder onto a blank board. This was simpler to get correct than trying to manually un-apply a move, and it's what the `RebuildFromHistory` method in `GameService` does.
- **Reset Game reuses the same game ID.** "Start a fresh game session" is interpreted as resetting the existing session in place (board/history/status cleared, same `gameId`), rather than minting a new ID — the frontend doesn't need to juggle IDs across a reset.
- **Scoreboard updates are idempotent per game.** `GameState.ScoreboardUpdatedForThisGame` guards against ever double-counting a completed game's result, even if the state were queried or mutated more than once.
- **Frontend visual direction:** a "chalkboard scorekeeper" theme (deep board-green, chalk-white structure, warm wood frame, chalk-yellow/chalk-blue for X/O) rather than a generic SaaS card layout — chosen because a tic-tac-toe grid is literally something people scrawl on a board.

## Clarifications and assumptions

- **Clarification 2 (Scoreboard and Undo): Option A was chosen.** Undo is disabled once a game is `Won` or `Draw` (`canUndo` is `false`, and the undo endpoint returns 400 if called anyway). This keeps the scoreboard permanently final for a completed game and avoids the extra bookkeeping Option B would need. (`ScoreboardService.UndoLastRecordedResult` exists but is unused in the current API surface — kept only as evidence of how Option B could be wired in if the requirement changed.)
- **Move request format:** the API accepts either `cellIndex` (0–8, row-major) or `row`/`column` — whichever the frontend finds convenient. The shipped frontend always sends `cellIndex`.
- **Undo in Computer Mode** removes the computer's move and the human move immediately before it, as one unit — unless the computer hasn't moved yet (edge case, since it always moves inline), in which case only the single human move is removed.
- **In-memory storage** means all games and the scoreboard reset when the backend restarts. This was explicitly allowed by the brief ("in-memory storage is acceptable").
- **CORS** is opened for `http://localhost:4200` only, matching the default Angular dev server port.

## Known limitations

- State does not survive a backend restart (by design, per the brief).
- No authentication/multi-user separation — the scoreboard is shared across whoever hits the same running backend, which is fine for a local single-reviewer demo but not for a shared deployment.
- The computer opponent implements the exact priority list requested (win → block → center → corner → any cell), not a full unbeatable minimax — it can lose in positions the priority list doesn't cover optimally.
- No persistence layer (SQLite) was added since in-memory storage was explicitly acceptable; swapping `GameService`/`ScoreboardService` for a DB-backed implementation would be the natural next step.

## Future improvements

- Optional minimax/unbeatable AI difficulty setting.
- SQLite (or similar) persistence so the scoreboard survives a restart.
- Player names/accounts instead of an anonymous shared scoreboard.
- WebSocket/SignalR push instead of request/response, to support a live two-device (not same-browser) two-player mode.
- End-to-end (Cypress/Playwright) tests covering the full click-through flow, on top of the current unit tests.

## AI-assisted development notes

This solution was built with Claude (Anthropic) as a pair-programming assistant, working directly in this repository's file structure.

- **Requirement → spec:** The two source documents (JD and Round 2 problem statement) were uploaded and read in full first. The problem statement's "Suggested API Scope" table and functional requirements were used almost verbatim as the spec — endpoint names, request/response shapes, and the undo/scoreboard rules were taken directly from the brief rather than reinterpreted.
- **Prompts used (summarized):** an initial prompt shared the two documents and asked what to help with; a follow-up explicitly asked to "start building the actual solution (Angular + .NET app)"; a final prompt asked to complete the project, package it for download, and explain how to run it.
- **What the AI generated:** the full backend (models, services, controllers, `Program.cs`, xUnit tests) and the full frontend (standalone Angular components, the HTTP service, models, SCSS design system, Karma tests), plus this README.
- **What was reviewed carefully / would need re-checking before submission:**
  - The undo logic (`GameService.UndoMove` / `RebuildFromHistory`) — this is the trickiest piece of game-state logic and is worth re-reading against the "Undo Behavior by Mode" examples in the brief line by line.
  - The computer's move-priority order in `ComputerPlayerService` — confirm it matches "win → block → center → corner → any cell" exactly, including the dual-threat case (own win takes priority over blocking).
  - Whether Option A (disable undo after completion) vs. Option B is the right call for your submission — it's called out explicitly above so it's easy to defend or switch in review.
- **Assumptions made:** documented above under "Clarifications and assumptions" (reset reuses the game ID; computer moves inline in the same request; move request accepts either `cellIndex` or `row`/`column`).
- **Trade-offs:** simplicity over cleverness throughout — e.g., undo-by-replay instead of reversing fields, a lock-per-game-object instead of a more elaborate concurrency scheme, and no minimax AI since the brief specified an exact priority list rather than "unbeatable."
- Before submitting, run both test suites yourself, click through all the acceptance-criteria scenarios once by hand, and be ready to walk the panel through `GameService.cs` and `ComputerPlayerService.cs` in particular — those two files carry almost all of the actual game logic.
