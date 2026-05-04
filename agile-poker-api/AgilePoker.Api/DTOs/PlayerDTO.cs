using System.Diagnostics.CodeAnalysis;

namespace AgilePoker.Api.DTOs;

[method: SetsRequiredMembers]
public class PlayerDTO(Guid id, string name)
{
    public required Guid Id { get; init; } = id;
    public required string Name { get; init; } = name;
    public bool HasVoted { get; set; }
    public string? Vote { get; set; }
}