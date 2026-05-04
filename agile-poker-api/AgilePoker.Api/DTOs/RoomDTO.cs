using System.Diagnostics.CodeAnalysis;

namespace AgilePoker.Api.DTOs;

[method: SetsRequiredMembers]
public class RoomDTO(string roomId)
{
    public required string RoomId { get; init; } = roomId;
    public bool AreCardsRevealed { get; set; }
    
    public List<PlayerDTO> Players { get; } = new();
}