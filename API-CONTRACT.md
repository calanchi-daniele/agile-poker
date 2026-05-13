# Agile Poker: Real-Time API Contract

This document outlines the WebSocket/SignalR contract for the Agile Poker real-time estimation engine. 

* **Protocol:** SignalR (WebSockets w/ Long Polling fallback)
* **Endpoint:** `http://localhost:5251/AgilePoker`

---

## 📦 Data Models (DTOs)

### `RoomDTO`
Represents the current state of a poker room and its participants.
{
  roomId: string;
  areCardsRevealed: boolean;
  players: PlayerDTO[];
}

### `PlayerDTO`
Represents an individual participant in the room.
Note: vote is strictly null unless areCardsRevealed is true for the room.
{
  id: string;          // Guid
  name: string;
  hasVoted: boolean;
  vote?: string | null; 
}

---

### 📥 Client -> Server (Invocations)

* `JoinRoom(string roomId, string playerName)`: Connects the user to the specified room.
* `LeaveRoom(string roomId)`: Removes the user from the specified room.
* `SubmitVote(string roomId, string vote)`: Casts an estimate. Valid votes: 0, 1, 2, 3, 5, 8, 13, 20, 40, 100.
* `ResetTable(string roomId)`: Clears all votes and starts a new estimation round.
* `AddBot(string roomId)`: Spawns a background worker that will auto-vote after a random delay. Returns boolean.

---

### 📤 Server -> Client (Listeners)

**Room & Roster Events**
* `PlayerJoined(RoomDTO room)`: Triggered globally when a new user enters the room. Updates the full room state.
* `PlayerLeft(PlayerDTO player)`: Triggered globally when a user disconnects or explicitly leaves.
* `JoinRoomFailed()`: Triggered only to the caller if room initialization fails..

**Game State Events**
* `VoteSubmitted(PlayerDTO player)`: Triggered globally when any user casts a vote. The vote value remains hidden; hasVoted transitions to true.
* `CardsRevealed(RoomDTO room)`: Triggered globally automatically exactly 1 second after the final player casts their vote. Contains the unmasked estimates.
* `TableReset(RoomDTO room)`: Triggered globally when a user resets the table.
* `SubmitVoteFailed(Error error)`: Triggered only to the caller if an invalid vote is submitted or the round is already revealed.
