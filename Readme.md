# AgilePoker 🃏

A real-time, event-driven Agile Poker estimation tool designed for distributed Scrum teams. 

This project demonstrates a modern full-stack architecture, focusing on highly concurrent backend state management, real-time WebSocket communication, and a reactive, modern frontend.

## 🏗️ Architecture & Tech Stack

### Backend (.NET 9 API)
* **Core:** C#, ASP.NET Core 9, minimal APIs.
* **Real-Time Engine:** SignalR (WebSockets) for low-latency, bi-directional communication.
* **State Management:** Thread-safe, in-memory domain management atomicity. 
* **Background Workers:** Event-driven `SimulationService` using isolated bot interactions.
* **Testing:** xUnit, FluentAssertions, and NSubstitute ensuring 100% coverage of domain business rules.

### Frontend (React) - *In Progress*
* **Core:** React 18, TypeScript, Vite.
* **Styling:** TailwindCSS for rapid, utility-first UI.
* **State:** React Context API for volatile WebSocket state projection.

## 🚀 Key Features
* **Strict Domain Rules:** Votes are securely masked during the estimation phase and only revealed when the Scrum Master/Team decides, preventing cognitive anchoring.
* **Bot Simulation:** Standalone background services simulate network clients to populate the room with automated voters for testing/demo purposes.
* **Concurrent-Safe:** Engineered to handle massive, simultaneous connections without state corruption or memory leaks.

## 🛠️ How to Run Locally

### Backend
1. Navigate to `AgilePoker.Api`
2. Run `dotnet run --launch-profile http` (Listens on port 5251)
3. To run tests: `dotnet test`