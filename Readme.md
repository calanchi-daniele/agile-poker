# AgilePoker 🃏

A real-time, event-driven Agile Poker estimation tool designed for distributed Scrum teams. 

This project demonstrates a modern full-stack architecture, focusing on highly concurrent backend state management, real-time WebSocket communication, and a reactive frontend.

## 🏗️ Architecture & Tech Stack

### Backend (.NET 9 API)
* **Core:** C#, ASP.NET Core 9, minimal APIs.
* **Real-Time Engine:** SignalR (WebSockets) for low-latency, bi-directional communication.
* **State Management:** Thread-safe, in-memory domain management using `ConcurrentDictionary` and atomic state transitions (via Tuples). 
* **Background Workers:** Event-driven `SimulationService` orchestrating isolated async bot interactions without blocking the domain.
* **Testing:** xUnit, FluentAssertions, and NSubstitute ensuring 100% coverage of domain business rules. Leverages .NET `TimeProvider` and `FakeTimeProvider` for highly-performant, deterministic async unit testing.

### Frontend (React SPA)
* **Core:** React 19, TypeScript, Vite 8.
* **Styling:** TailwindCSS v4 for rapid, utility-first UI.
* **State Management:** React Context API and custom Hooks (`useSignalR`) for volatile WebSocket state projection, completely decoupling transport logic from the UI.
* **E2E Testing:** Playwright. Utilizes multi-browser context isolation to verify real-time, multi-player WebSocket synchronization and race-condition handling.

## 🚀 Key Features
* **Strict Domain Rules & Auto-Reveal:** Votes are securely masked during the estimation phase. Once the final player votes, the application layer orchestrates a synchronized reveal, preventing cognitive anchoring.
* **Persistent Bot Simulation:** Standalone background services simulate network clients to populate the room with automated voters. Bots securely persist across rounds and respect domain rules.
* **Concurrent-Safe:** Engineered to handle massive, simultaneous connections and race conditions without state corruption, deadlocks, or memory leaks.

## 🛠️ How to Run Locally

### 1. Backend (.NET 9)
1. Navigate to the `agile-poker-api/AgilePoker.Api` directory.
2. Start the server: `dotnet run --launch-profile http` (Listens on port 5251).
3. To run the backend test suite: `dotnet test` from the `agile-poker-api` root.

### 2. Frontend (React + Vite)
1. Open a new terminal and navigate to the `agile-poker-web` directory.
2. Install dependencies: `npm install`
3. Start the dev server: `npm run dev` (Listens on port 5173).
4. **To run the E2E Browser Tests:** `npm run test:e2e` (Requires both backend and frontend to be running).