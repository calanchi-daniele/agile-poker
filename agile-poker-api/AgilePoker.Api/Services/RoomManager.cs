using System.Collections.Concurrent;
using AgilePoker.Api.Constants;
using AgilePoker.Api.DTOs;
using AgilePoker.Api.Exceptions;
using AgilePoker.Api.Models;
using AgilePoker.Api.Services.Interfaces;

namespace AgilePoker.Api.Services;

public class RoomManager : IRoomManager
{
    private readonly ConcurrentDictionary<string, Room> _roomsById = new();
    private readonly ConcurrentDictionary<string, Room> _roomsByConnectionId = new();
    
    private Room? GetRawRoom(string roomId, bool createIfNotExists = false)
    {
        return createIfNotExists ? _roomsById.GetOrAdd(roomId, _ => new Room(roomId))
                                 : _roomsById.GetValueOrDefault(roomId);
    }
    
    public RoomDTO? GetRoom(string roomId)
    {
        var room = GetRawRoom(roomId);
        return room?.ToDto();
    }

    public RoomDTO? GetRoomFromConnection(string connectionId)
    {
        var room = _roomsByConnectionId.GetValueOrDefault(connectionId);
        return room?.ToDto();
    }

    /// <summary>
    /// Adds a player to the room, creating it if it does not exist yet.
    /// Returns null if the connection is already registered in any room.
    /// </summary>
    public RoomDTO? JoinRoom(string roomId, string connectionId, string playerName)
    {
        return JoinRoom(roomId, new Player(connectionId, playerName));
    }

    /// <inheritdoc cref="JoinRoom(string, string, string)"/>
    public RoomDTO? JoinRoom(string roomId, Player player)
    {
        var room = GetRawRoom(roomId, true)!;

        if (!room.Players.TryAdd(player.ConnectionId, player))
            return null;
        
        if(!_roomsByConnectionId.TryAdd(player.ConnectionId, room))
        {
            room.Players.TryRemove(player.ConnectionId, out _);
            return null;
        }

        return room.ToDto();
    }

    /// <summary>
    /// Removes the player from the room.
    /// When the last player leaves, the room is automatically deleted.
    /// </summary>
    public PlayerDTO? LeaveRoom(string roomId, string connectionId)
    {
        var room = GetRawRoom(roomId);
        var player = room?.Players.GetValueOrDefault(connectionId);
        if (player is null)
            return null;
        
        room!.Players.TryRemove(connectionId, out _);
        _roomsByConnectionId.TryRemove(connectionId, out _);
        
        if(room.Players.IsEmpty)
            _roomsById.TryRemove(roomId, out _);

        return player.ToDto();
    }

    /// <summary>
    /// Submits a vote for a player.
    /// Silently returns null and rejects the vote if the room's cards are already revealed.
    /// </summary>
    /// <exception cref="InvalidVoteException">Thrown when the vote value is not in the set of allowed votes.</exception>
    public (PlayerDTO? player, bool allVoted) SubmitVote(string roomId, string connectionId, string vote)
    {
        if(!AppConstants.Votes.Contains(vote))
            throw new InvalidVoteException($"Invalid vote: {vote}");
        
        var room = GetRawRoom(roomId);
        if (room is null || room.AreCardsRevealed)
            return (null, false);
            
        var player = room.Players.GetValueOrDefault(connectionId);
        
        if (player is not null)
            player.Vote = vote;
        
        return (player?.ToDto(), room.Players.All(p => p.Value.HasVoted));
    }

    /// <summary>
    /// Reveals the cards for all players in the room.
    /// Cards are only revealed when every player in the room has submitted a vote.
    /// </summary>
    public RoomDTO? CheckRevealCards(string roomId)
    {
        var room = GetRawRoom(roomId);

        if (room is null || room.AreCardsRevealed || room.Players.Any(p => !p.Value.HasVoted))
            return null;

        room.AreCardsRevealed = true;
        return room.ToDto();
    }

    public RoomDTO? ResetTable(string roomId)
    {
        var room = GetRawRoom(roomId);
        if (room is not null)
        {
            room.AreCardsRevealed = false;

            foreach (var (_, player) in room.Players)
                player.Vote = null;
        }

        return room?.ToDto();
    }

    public List<Player> GetBotPlayers(string roomId)
    {
        var room = GetRawRoom(roomId);
        return room?.Players.Values.Where(pl => pl.IsBot).ToList() ?? [];
    }
}