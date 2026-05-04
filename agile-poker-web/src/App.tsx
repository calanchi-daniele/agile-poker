import React, { useState } from 'react';
import { useSignalR } from './context/SignalRContext';

export default function App() {
  // 1. Pull the data and functions from our SignalR Context
  const { room, error, joinRoom, submitVote, resetTable, addBot } = useSignalR();

  // 2. Local state just for the login form typing
  const [roomIdInput, setRoomIdInput] = useState('');
  const [playerNameInput, setPlayerNameInput] = useState('');

  // The available Agile Poker cards
  const fibonacci = ['1', '2', '3', '5', '8', '13'];

  // Handle the form submission
  const handleJoin = (e: React.FormEvent) => {
    e.preventDefault(); // Prevents the browser from reloading the page
    if (roomIdInput && playerNameInput) {
      joinRoom(roomIdInput, playerNameInput);
    }
  };

  // --- VIEW 1: LOBBY (If not in a room yet) ---
  if (!room) {
    return (
        <div className="flex items-center justify-center min-h-screen bg-gray-100">
          <form onSubmit={handleJoin} className="w-96 p-8 bg-white rounded shadow-md">
            <h1 className="mb-6 text-2xl font-bold text-center">Agile Poker</h1>
            {error && <p className="mb-4 text-sm text-red-500">{error}</p>}

            <input
                className="w-full p-2 mb-4 border rounded"
                placeholder="Room ID (e.g. team-alpha)"
                value={roomIdInput}
                onChange={(e) => setRoomIdInput(e.target.value)}
            />
            <input
                className="w-full p-2 mb-4 border rounded"
                placeholder="Your Name"
                value={playerNameInput}
                onChange={(e) => setPlayerNameInput(e.target.value)}
            />
            <button type="submit" className="w-full p-2 text-white bg-blue-600 rounded hover:bg-blue-700">
              Join Room
            </button>
          </form>
        </div>
    );
  }

  // --- VIEW 2: POKER TABLE (If in a room) ---
  return (
      <div className="min-h-screen p-8 bg-gray-50">
        <div className="max-w-4xl mx-auto">

          {/* Header Controls */}
          <div className="flex items-center justify-between mb-8">
            <h1 className="text-3xl font-bold">Room: {room.roomId}</h1>
            <div className="space-x-4">
              <button onClick={() => addBot(room.roomId)} className="px-4 py-2 text-white bg-green-600 rounded hover:bg-green-700">Add Bot</button>
              <button onClick={() => resetTable(room.roomId)} className="px-4 py-2 text-white bg-red-600 rounded hover:bg-red-700">Reset Table</button>
            </div>
          </div>

          {/* Global Error Banner */}
          {error && <p className="p-4 mb-4 font-semibold text-red-700 bg-red-100 rounded">{error}</p>}

          {/* Players Area */}
          <div className="grid grid-cols-2 gap-4 mb-12 md:grid-cols-4">
            {room.players.map((player) => (
                <div key={player.id} className="p-4 text-center bg-white rounded shadow">
                  <div className="mb-2 font-semibold text-gray-700">{player.name}</div>
                  <div className={`h-24 flex items-center justify-center text-2xl font-bold rounded ${player.hasVoted ? 'bg-blue-100 text-blue-800 border-2 border-blue-500' : 'bg-gray-100 text-gray-400 border-2 border-dashed border-gray-300'}`}>
                    {/* Visual Logic: Show vote if revealed, otherwise show a thumbs up if they voted */}
                    {room.areCardsRevealed ? (player.vote || 'Skipped') : (player.hasVoted ? '👍' : '...')}
                  </div>
                </div>
            ))}
          </div>

          {/* Voting Cards Area (Only show if cards are hidden) */}
          {!room.areCardsRevealed && (
              <div className="text-center">
                <h3 className="mb-4 text-xl font-medium text-gray-600">Cast your vote:</h3>
                <div className="flex justify-center space-x-4">
                  {fibonacci.map((vote) => (
                      <button
                          key={vote}
                          onClick={() => submitVote(room.roomId, vote)}
                          className="w-16 h-24 text-2xl font-bold text-gray-700 transition-colors bg-white border-2 border-gray-300 rounded shadow hover:border-blue-500 hover:bg-blue-50"
                      >
                        {vote}
                      </button>
                  ))}
                </div>
              </div>
          )}

        </div>
      </div>
  );
}