using System.Diagnostics.CodeAnalysis;
using AgilePoker.Api.DTOs;

namespace AgilePoker.Api.Models;

[method: SetsRequiredMembers]
public class Player(string connectionId, string name, bool isBot = false)
{
    public Guid Id { get; } = Guid.NewGuid();
    public required string ConnectionId { get; init; } = connectionId;
    public required string Name { get; init; } = name;
    public string? Vote { get; set; }
    public bool IsBot { get; init; } = isBot;
    public bool HasVoted => !string.IsNullOrEmpty(Vote);
    
    public PlayerDTO ToDto(bool areCardsRevealed = false)
    {
        return new PlayerDTO(Id, Name)
        {
            Vote = areCardsRevealed ? Vote : null,
            HasVoted = !string.IsNullOrEmpty(Vote)
        };
    }
}