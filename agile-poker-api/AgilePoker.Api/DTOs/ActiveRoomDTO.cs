using System.Diagnostics.CodeAnalysis;

namespace AgilePoker.Api.DTOs;

[method: SetsRequiredMembers]
public class ActiveRoomDTO(string roomId, int playerCount, string roomName = "")
{
    public required string RoomId { get; init; } = roomId;
    public string RoomName { get; set; } = roomName;
    public int PlayerCount { get; set; } = playerCount;
}