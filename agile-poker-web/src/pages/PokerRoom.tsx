import { useState } from 'react';
import { useParams, useNavigate, useSearchParams } from 'react-router-dom';
import { useSignalR } from '../context/SignalRContext';
import { Toaster, toast } from 'react-hot-toast';

export function PokerRoom() {
    const { roomId } = useParams<{ roomId: string }>();
    const [searchParams] = useSearchParams();
    const roomNameParam = searchParams.get('name') || '';
    const navigate = useNavigate();
    const { room, joinRoom, leaveRoom, submitVote, resetTable, addBot } = useSignalR();
    const [playerNameInput, setPlayerNameInput] = useState('');

    const voteOptions = ['0', '1', '2', '3', '5', '8', '13', '20', '40', '100'];

    if (!room) {
        return (
            <div className="flex items-center justify-center min-h-screen bg-linear-to-br from-slate-900 via-purple-900 to-slate-900">
                <Toaster position="top-center" />
                <form
                    onSubmit={(e) => {
                        e.preventDefault();
                        if(playerNameInput.trim())
                            joinRoom(roomId!, playerNameInput, roomNameParam)
                        else
                            toast.error("Enter your name");
                    }}
                    className="w-full max-w-md p-10 mx-4 border border-white/10 shadow-2xl bg-white/10 backdrop-blur-lg rounded-2xl"
                >
                    <h2 className="text-2xl font-bold text-center text-white mb-6">Joining Room <span className="text-purple-300">{roomId}</span></h2>
                    <label className="block mb-2 text-sm font-medium text-purple-200">Your Name</label>
                    <input
                        className="w-full px-4 py-3 mb-6 text-white transition-colors bg-white/5 border border-white/20 rounded-xl focus:outline-none focus:ring-2 focus:ring-purple-500 placeholder-white/30"
                        placeholder="e.g. Alice"
                        value={playerNameInput}
                        onChange={(e) => setPlayerNameInput(e.target.value)}
                    />
                    <button type="submit" className="w-full py-3.5 font-bold text-white transition-all transform bg-purple-600 shadow-lg rounded-xl hover:bg-purple-500 hover:scale-[1.02]">
                        Join Table
                    </button>
                </form>
            </div>
        );
    }

    return (
        <div className="min-h-screen text-slate-800 bg-slate-50">
            <Toaster position="top-center" />
            <nav className="px-8 py-4 bg-white border-b shadow-sm border-slate-200">
                <div className="flex items-center justify-between max-w-6xl mx-auto">
                    <div className="flex items-center space-x-3">
                        <div className="flex items-center justify-center w-10 h-10 text-white bg-purple-600 rounded-lg">🃏</div>
                        <h1 className="text-2xl font-bold truncate max-w-50 sm:max-w-md">
                            {room.roomName}
                            <span className="ml-2 text-sm font-normal text-slate-400 font-mono">({room.roomId})</span>
                        </h1>
                    </div>
                    <div className="flex space-x-3">
                        <button onClick={() => addBot(room.roomId)} className="px-5 py-2.5 text-sm font-semibold text-slate-700 transition-colors bg-slate-100 border border-slate-300 rounded-lg hover:bg-slate-200">
                            🤖 Add Bot
                        </button>
                        <button onClick={() => resetTable(room.roomId)} className="px-5 py-2.5 text-sm font-semibold text-white transition-colors bg-red-500 rounded-lg shadow-sm hover:bg-red-600">
                            🔄 Reset Table
                        </button>
                        {/* NEW EXIT BUTTON */}
                        <button
                            onClick={() => { leaveRoom(room.roomId); navigate('/'); }}
                            className="px-5 py-2.5 text-sm font-semibold text-slate-700 transition-colors bg-white border border-slate-300 rounded-lg shadow-sm hover:bg-slate-50"
                        >
                            🚪 Exit
                        </button>
                    </div>
                </div>
            </nav>

            <main className="max-w-6xl px-8 py-12 mx-auto">
                {/* Players Grid Area */}
                <div className="mb-16">
                    <h2 className="mb-6 text-xl font-semibold text-slate-600">Team ({room.players.length})</h2>
                    <div className="grid grid-cols-2 gap-6 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5">
                        {room.players.map((player) => (
                            <div key={player.id} className="flex flex-col items-center p-6 transition-all bg-white border border-slate-100 rounded-2xl shadow-sm hover:shadow-md">
                                <div className={`relative w-24 h-36 mb-4 rounded-xl flex items-center justify-center text-3xl font-bold transition-all duration-500 ${
                                    room.areCardsRevealed
                                        ? 'bg-purple-100 border-2 border-purple-500 text-purple-700 shadow-inner'
                                        : player.hasVoted
                                            ? 'bg-linear-to-br from-purple-500 to-indigo-600 text-white shadow-lg transform -translate-y-2'
                                            : 'bg-slate-100 border-2 border-dashed border-slate-300 text-slate-400'
                                }`}>
                                    {room.areCardsRevealed ? (player.vote || '—') : (player.hasVoted ? '👍' : '...')}
                                </div>
                                <span className="font-medium truncate max-w-full text-slate-700">{player.name}</span>
                            </div>
                        ))}
                    </div>
                </div>

                {/* Action Area: Voting Deck */}
                {!room.areCardsRevealed && (
                    <div className="p-8 text-center bg-white border shadow-sm border-slate-200 rounded-3xl">
                        <h3 className="mb-6 text-lg font-medium text-slate-500">Select your estimate</h3>
                        <div className="flex flex-wrap justify-center gap-4">
                            {voteOptions.map((vote) => (
                                <button
                                    key={vote}
                                    onClick={() => submitVote(room.roomId, vote)}
                                    className="w-20 h-32 text-2xl font-bold text-purple-700 transition-all transform bg-white border-2 border-purple-200 rounded-xl shadow-sm hover:border-purple-500 hover:bg-purple-50 hover:-translate-y-2 hover:shadow-lg focus:outline-none focus:ring-4 focus:ring-purple-200"
                                >
                                    {vote}
                                </button>
                            ))}
                        </div>
                    </div>
                )}
            </main>
        </div>
    );
}