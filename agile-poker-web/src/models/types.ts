// src/models/types.ts

export interface PlayerDTO {
    id: string;
    name: string;
    hasVoted: boolean;
    vote?: string | null;
}

export interface RoomDTO {
    roomId: string;
    roomName: string;
    areCardsRevealed: boolean;
    players: PlayerDTO[];
}

export interface ActiveRoomDTO {
    roomId: string;
    roomName: string;
    playerCount: number;
}