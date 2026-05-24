import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useSignalR } from '../context/SignalRContext';
import type { ActiveRoomDTO } from '../models/types';

export function SplashPage() {
    const [createRoomName, setCreateRoomName] = useState('');
    const [joinRoomId, setJoinRoomId] = useState('');
    const [activeRooms, setActiveRooms] = useState<ActiveRoomDTO[]>([]);

    const navigate = useNavigate();
    const { connection, getActiveRooms } = useSignalR();

    // Fetch rooms when connection is established, and set up a tiny polling loop for the lobby
    useEffect(() => {
        const fetchRooms = () => {
            if (connection?.state === 'Connected') {
                getActiveRooms().then(setActiveRooms).catch(console.error);
            }
        };

        fetchRooms(); // Initial fetch
        const interval = setInterval(fetchRooms, 3000); // Refresh lobby every 3 seconds
        return () => clearInterval(interval);
    }, [connection?.state, getActiveRooms]);

    const handleCreateRoom = (e: React.FormEvent) => {
        e.preventDefault();
        if (!createRoomName.trim()) return;

        // Generate the random secure code behind the scenes
        const newRoomId = Math.random().toString(36).substring(2, 8).toUpperCase();

        // Pass the chosen name safely through the URL query parameters
        navigate(`/room/${newRoomId}?name=${encodeURIComponent(createRoomName)}`);
    };

    const handleJoinRoom = (e: React.FormEvent) => {
        e.preventDefault();
        if (joinRoomId.trim()) navigate(`/room/${joinRoomId.toUpperCase()}`);
    };

    return (
        <div className="flex items-center justify-center min-h-screen p-4 bg-linear-to-br from-slate-900 via-purple-900 to-slate-900">
            {/* 2-Column Grid: Stacks to 1 column on mobile (md:grid-cols-2) */}
            <div className="grid w-full max-w-5xl gap-6 grid-cols-1 md:grid-cols-2">

                {/* LEFT COLUMN: Create & Join */}
                <div className="p-8 border shadow-2xl border-white/10 bg-white/10 backdrop-blur-lg rounded-2xl flex flex-col justify-center">
                    <div className="mb-8 text-center">
                        <h1 className="text-4xl font-extrabold tracking-tight text-white">Agile Poker</h1>
                        <p className="mt-2 text-purple-200">Real-time team estimation</p>
                    </div>

                    <form onSubmit={handleCreateRoom} className="mb-6">
                        <label className="block mb-2 text-sm font-medium text-purple-200">Start a New Session</label>
                        <div className="flex gap-2">
                            <input
                                className="flex-1 px-4 py-3 text-white transition-colors bg-white/5 border border-white/20 rounded-xl focus:outline-none focus:ring-2 focus:ring-purple-500 placeholder-white/30"
                                placeholder="e.g. Sprint 42 Planning"
                                value={createRoomName}
                                onChange={(e) => setCreateRoomName(e.target.value)}
                            />
                            <button
                                type="submit"
                                disabled={!createRoomName.trim()}
                                className="px-6 font-bold text-white transition-all bg-purple-600 rounded-xl hover:bg-purple-500 disabled:opacity-50 disabled:cursor-not-allowed"
                            >
                                Create
                            </button>
                        </div>
                    </form>

                    <div className="relative flex items-center py-2 mb-4">
                        <div className="grow border-t border-white/20"></div>
                        <span className="shrink-0 mx-4 text-sm font-medium text-purple-300">or join via code</span>
                        <div className="grow border-t border-white/20"></div>
                    </div>

                    <form onSubmit={handleJoinRoom} className="flex gap-2">
                        <input
                            className="flex-1 px-4 py-3 text-white transition-colors uppercase bg-white/5 border border-white/20 rounded-xl focus:outline-none focus:ring-2 focus:ring-purple-500 placeholder-white/30"
                            placeholder="ENTER CODE"
                            maxLength={6}
                            value={joinRoomId}
                            onChange={(e) => setJoinRoomId(e.target.value)}
                        />
                        <button
                            type="submit"
                            disabled={!joinRoomId.trim()}
                            className="px-6 font-bold text-slate-900 transition-all bg-purple-100 rounded-xl hover:bg-white disabled:opacity-50 disabled:cursor-not-allowed"
                        >
                            Join
                        </button>
                    </form>
                </div>

                {/* RIGHT COLUMN: Active Rooms Lobby */}
                <div className="p-8 border shadow-2xl border-white/10 bg-white/10 backdrop-blur-lg rounded-2xl flex flex-col max-h-150">
                    <h2 className="text-xl font-bold text-white mb-4 flex items-center justify-between">
                        Lobby
                        <span className="text-xs font-normal px-2 py-1 bg-purple-500/30 rounded-full">{activeRooms.length} Active</span>
                    </h2>

                    {activeRooms.length === 0 ? (
                        <div className="flex-1 flex flex-col items-center justify-center text-purple-300/50">
                            <span className="text-4xl mb-2">👻</span>
                            <p>No active rooms right now.</p>
                        </div>
                    ) : (
                        <div className="flex-1 overflow-y-auto pr-2 space-y-3">
                            {activeRooms.map(r => (
                                <div key={r.roomId} className="flex items-center justify-between p-4 transition-colors border rounded-xl bg-white/5 border-white/10 hover:bg-white/10">
                                    <div className="flex flex-col">
                                        {/* Primary line: Room Name + Code in parentheses */}
                                        <div className="flex items-baseline space-x-2">
                                            <h3 className="text-lg font-bold text-white truncate max-w-45" title={r.roomName}>
                                                {r.roomName}
                                            </h3>
                                            <span className="text-xs font-mono text-purple-300/60">
                                                ({r.roomId})
                                            </span>
                                        </div>
                                        {/* Secondary line: Player count */}
                                        <p className="text-sm text-purple-300">
                                            {r.playerCount} {r.playerCount === 1 ? 'person' : 'people'} sitting
                                        </p>
                                    </div>

                                    <button
                                        onClick={() => navigate(`/room/${r.roomId}`)}
                                        className="px-4 py-2 text-sm font-bold text-white transition-colors bg-purple-600 rounded-lg hover:bg-purple-500"
                                    >
                                        Enter
                                    </button>
                                </div>
                            ))}
                        </div>
                    )}
                </div>

            </div>
        </div>
    );
}