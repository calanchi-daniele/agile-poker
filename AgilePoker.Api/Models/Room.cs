using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using AgilePoker.Api.DTOs;

namespace AgilePoker.Api.Models;

[method: SetsRequiredMembers]
public class Room(string roomId)
{
    public required string RoomId { get; set; } = roomId;
    public bool AreCardsRevealed { get; set; }
    
    public ConcurrentDictionary<string, Player> Players { get; } = new();
    
    public RoomDTO ToDto()
    {
        var roomDto = new RoomDTO (RoomId)
        {
            AreCardsRevealed = AreCardsRevealed
        };
        
        foreach (var (_, player) in Players)
            roomDto.Players.Add(player.ToDto(AreCardsRevealed));
        
        return roomDto;
    }
}