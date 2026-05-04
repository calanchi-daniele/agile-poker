// src/context/SignalRContext.tsx
import React, { createContext, useContext, useEffect, useState } from 'react';
import * as signalR from '@microsoft/signalr';
import type {RoomDTO, PlayerDTO} from '../models/types';

// 1. Define the "Interface" of our Context (What components can access)
interface SignalRContextType {
    connection: signalR.HubConnection | null;
    room: RoomDTO | null;
    error: string | null;
    joinRoom: (roomId: string, playerName: string) => Promise<void>;
    submitVote: (roomId: string, vote: string) => Promise<void>;
    resetTable: (roomId: string) => Promise<void>;
    addBot: (roomId: string) => Promise<void>;
}

// 2. Create the Context (The "Singleton" container)
const SignalRContext = createContext<SignalRContextType | undefined>(undefined);

// 3. Create the Provider (The implementation)
export const SignalRProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
    const [room, setRoom] = useState<RoomDTO | null>(null);
    const [error, setError] = useState<string | null>(null);

    // 1. Lazy Initialize the connection.
    // Passing an arrow function here means React only runs this block ONCE when the app starts.
    const [connection] = useState<signalR.HubConnection>(() => {
        return new signalR.HubConnectionBuilder()
            .withUrl('http://localhost:5251/AgilePoker') // Your .NET Backend URL
            .withAutomaticReconnect()
            .configureLogging(signalR.LogLevel.Information)
            .build();
    });

// 2. Start the connection and listen to events
    useEffect(() => {
        // Clean up old listeners to prevent duplicates during React StrictMode hot-reloads
        connection.off('PlayerJoined');
        connection.off('PlayerLeft');
        connection.off('VoteSubmitted');
        connection.off('CardsRevealed');
        connection.off('TableReset');
        connection.off('MissingVotes');
        connection.off('SubmitVoteFailed');

        // Map your backend events to update our local React state
        connection.on('PlayerJoined', (updatedRoom: RoomDTO) => setRoom(updatedRoom));

        connection.on('PlayerLeft', (leftPlayer: PlayerDTO) => {
            setRoom((prev) => prev ? { ...prev, players: prev.players.filter(p => p.id !== leftPlayer.id) } : null);
        });

        connection.on('VoteSubmitted', (votingPlayer: PlayerDTO) => {
            setRoom((prev) => {
                if (!prev) return null;
                const updatedPlayers = prev.players.map(p => p.id === votingPlayer.id ? votingPlayer : p);
                return { ...prev, players: updatedPlayers };
            });
        });

        connection.on('CardsRevealed', (updatedRoom: RoomDTO) => setRoom(updatedRoom));
        connection.on('TableReset', (updatedRoom: RoomDTO) => setRoom(updatedRoom));

        connection.on('MissingVotes', () => setError('Waiting on others to vote!'));
        connection.on('SubmitVoteFailed', (err) => setError(err?.message || 'Vote failed'));

        // Start the engine ONLY if it is disconnected
        if (connection.state === signalR.HubConnectionState.Disconnected) {
            connection.start()
                .then(() => console.log('Connected to SignalR Hub!'))
                .catch(e => console.error('Connection failed: ', e));
        }

        // We removed connection.stop() from here so the socket stays alive across StrictMode renders
        return () => {};
    }, [connection]);

    // Expose the methods that call the Hub
    const joinRoom = async (roomId: string, playerName: string) => connection.invoke('JoinRoom', roomId, playerName);
    const submitVote = async (roomId: string, vote: string) => connection.invoke('SubmitVote', roomId, vote);
    const resetTable = async (roomId: string) => connection.invoke('ResetTable', roomId);
    const addBot = async (roomId: string) => connection.invoke('AddBot', roomId);

    return (
        <SignalRContext.Provider value={{ connection, room, error, joinRoom, submitVote, resetTable, addBot }}>
            {children}
        </SignalRContext.Provider>
    );
};
export default SignalRProvider

// 4. Create a Custom Hook to easily access this context from any component
export const useSignalR = () => {
    const context = useContext(SignalRContext);
    if (!context) throw new Error('useSignalR must be used within a SignalRProvider');
    return context;
};