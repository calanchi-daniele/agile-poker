// src/context/SignalRContext.tsx
import React, { createContext, useContext, useEffect, useState } from 'react';
import * as signalR from '@microsoft/signalr';
import toast from 'react-hot-toast';
import type {RoomDTO, PlayerDTO} from '../models/types';

interface SignalRContextType {
    connection: signalR.HubConnection | null;
    room: RoomDTO | null;
    joinRoom: (roomId: string, playerName: string) => Promise<void>;
    submitVote: (roomId: string, vote: string) => Promise<void>;
    resetTable: (roomId: string) => Promise<void>;
    addBot: (roomId: string) => Promise<void>;
}

const SignalRContext = createContext<SignalRContextType | undefined>(undefined);

export const SignalRProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
    const [room, setRoom] = useState<RoomDTO | null>(null);

    const [connection] = useState<signalR.HubConnection>(() => {
        const backendUrl = import.meta.env.VITE_BACKEND_URL || "http://localhost:5251";
        return new signalR.HubConnectionBuilder()
            .withUrl(backendUrl + '/AgilePoker')
            .withAutomaticReconnect()
            .configureLogging(signalR.LogLevel.Information)
            .build();
    });

    useEffect(() => {
        connection.off('PlayerJoined');
        connection.off('PlayerLeft');
        connection.off('VoteSubmitted');
        connection.off('CardsRevealed');
        connection.off('TableReset');
        connection.off('MissingVotes');
        connection.off('SubmitVoteFailed');

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

        connection.on('MissingVotes', () => toast.error('Waiting on others to vote!'));
        connection.on('SubmitVoteFailed', (err) => toast.error(err?.message || 'Vote failed'));

        // Start the engine ONLY if it is disconnected
        if (connection.state === signalR.HubConnectionState.Disconnected) {
            connection.start()
                .then(() => console.log('Connected to SignalR Hub!'))
                .catch(e => console.error('Connection failed: ', e));
        }

        return () => {};
    }, [connection]);

    // Expose the methods that call the Hub
    const joinRoom = async (roomId: string, playerName: string) => connection.invoke('JoinRoom', roomId, playerName);
    const submitVote = async (roomId: string, vote: string) => connection.invoke('SubmitVote', roomId, vote);
    const resetTable = async (roomId: string) => connection.invoke('ResetTable', roomId);
    const addBot = async (roomId: string) => connection.invoke('AddBot', roomId);

    return (
        <SignalRContext.Provider value={{ connection, room, joinRoom, submitVote, resetTable, addBot }}>
            {children}
        </SignalRContext.Provider>
    );
};
export default SignalRProvider

export const useSignalR = () => {
    const context = useContext(SignalRContext);
    if (!context) throw new Error('useSignalR must be used within a SignalRProvider');
    return context;
};