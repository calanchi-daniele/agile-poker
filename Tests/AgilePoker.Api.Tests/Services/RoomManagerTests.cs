using AgilePoker.Api.Exceptions;
using AgilePoker.Api.Models;
using AgilePoker.Api.Services;
using FluentAssertions;

namespace AgilePoker.Api.Tests;

public class RoomManagerTests
{
    private readonly RoomManager _sut;

    private const string RoomId = "room-1";
    private const string ConnectionId = "conn-1";
    private const string PlayerName = "Alice";

    public RoomManagerTests()
    {
        _sut = new RoomManager();
    }

    [Fact]
    [Trait("Method", nameof(RoomManager.GetRoom))]
    public void GetRoom_WhenRoomDoesNotExist_ReturnsNull()
    {
        var result = _sut.GetRoom(RoomId);

        result.Should().BeNull();
    }

    [Fact]
    [Trait("Method", nameof(RoomManager.GetRoom))]
    public void GetRoom_WhenRoomExists_ReturnsRoomDto()
    {
        _sut.JoinRoom(RoomId, ConnectionId, PlayerName);

        var result = _sut.GetRoom(RoomId);

        result.Should().NotBeNull();
        result!.RoomId.Should().Be(RoomId);
    }

    [Fact]
    [Trait("Method", nameof(RoomManager.GetRoom))]
    public void GetRoom_WhenRoomExists_ReturnsDtoWithCorrectPlayers()
    {
        _sut.JoinRoom(RoomId, ConnectionId, PlayerName);
        _sut.JoinRoom(RoomId, "conn-2", "Bob");

        var result = _sut.GetRoom(RoomId);

        result!.Players.Should().HaveCount(2);
        result.Players.Select(p => p.Name).Should().BeEquivalentTo([PlayerName, "Bob"]);
    }

    [Fact]
    [Trait("Method", nameof(RoomManager.GetRoomFromConnection))]
    public void GetRoomFromConnection_WhenConnectionNotInAnyRoom_ReturnsNull()
    {
        var result = _sut.GetRoomFromConnection(ConnectionId);

        result.Should().BeNull();
    }

    [Fact]
    [Trait("Method", nameof(RoomManager.GetRoomFromConnection))]
    public void GetRoomFromConnection_WhenConnectionIsInRoom_ReturnsRoomDto()
    {
        _sut.JoinRoom(RoomId, ConnectionId, PlayerName);

        var result = _sut.GetRoomFromConnection(ConnectionId);

        result.Should().NotBeNull();
        result!.RoomId.Should().Be(RoomId);
    }

    [Fact]
    [Trait("Method", nameof(RoomManager.GetRoomFromConnection))]
    public void GetRoomFromConnection_WhenPlayerHasLeft_ReturnsNull()
    {
        _sut.JoinRoom(RoomId, ConnectionId, PlayerName);
        _sut.LeaveRoom(RoomId, ConnectionId);

        var result = _sut.GetRoomFromConnection(ConnectionId);

        result.Should().BeNull();
    }

    [Fact]
    [Trait("Method", nameof(RoomManager.JoinRoom))]
    public void JoinRoom_WithNameOverload_CreatesRoomAndReturnsDto()
    {
        var result = _sut.JoinRoom(RoomId, ConnectionId, PlayerName);

        result.Should().NotBeNull();
        result!.RoomId.Should().Be(RoomId);
    }

    [Fact]
    [Trait("Method", nameof(RoomManager.JoinRoom))]
    public void JoinRoom_WithNameOverload_AddsPlayerToRoom()
    {
        var result = _sut.JoinRoom(RoomId, ConnectionId, PlayerName);

        result!.Players.Should().ContainSingle(p => p.Name == PlayerName);
    }

    [Fact]
    [Trait("Method", nameof(RoomManager.JoinRoom))]
    public void JoinRoom_WhenSameConnectionJoinsRoomTwice_ReturnsNull()
    {
        _sut.JoinRoom(RoomId, ConnectionId, PlayerName);

        var result = _sut.JoinRoom(RoomId, ConnectionId, "Alice2");

        result.Should().BeNull();
    }

    [Fact]
    [Trait("Method", nameof(RoomManager.JoinRoom))]
    public void JoinRoom_WhenSameConnectionJoinsRoomTwice_DoesNotDuplicatePlayer()
    {
        _sut.JoinRoom(RoomId, ConnectionId, PlayerName);
        _sut.JoinRoom(RoomId, ConnectionId, "Alice2");

        var room = _sut.GetRoom(RoomId);

        room!.Players.Should().ContainSingle();
    }

    [Fact]
    [Trait("Method", nameof(RoomManager.JoinRoom))]
    public void JoinRoom_WhenMultiplePlayersJoin_AllAreAddedToRoom()
    {
        _sut.JoinRoom(RoomId, ConnectionId, PlayerName);
        _sut.JoinRoom(RoomId, "conn-2", "Bob");
        _sut.JoinRoom(RoomId, "conn-3", "Charlie");

        var room = _sut.GetRoom(RoomId);

        room!.Players.Should().HaveCount(3);
    }

    [Fact]
    [Trait("Method", nameof(RoomManager.JoinRoom))]
    public void JoinRoom_WhenRoomDoesNotExist_CreatesIt()
    {
        _sut.JoinRoom(RoomId, ConnectionId, PlayerName);

        var room = _sut.GetRoom(RoomId);

        room.Should().NotBeNull();
    }

    [Fact]
    [Trait("Method", nameof(RoomManager.JoinRoom))]
    public void JoinRoom_WithPlayerOverload_ReturnsRoomDto()
    {
        var player = new Player(ConnectionId, PlayerName);

        var result = _sut.JoinRoom(RoomId, player);

        result.Should().NotBeNull();
        result!.RoomId.Should().Be(RoomId);
    }

    [Fact]
    [Trait("Method", nameof(RoomManager.JoinRoom))]
    public void JoinRoom_WithPlayerOverload_AddsPlayerToRoom()
    {
        var player = new Player(ConnectionId, PlayerName);

        var result = _sut.JoinRoom(RoomId, player);

        result!.Players.Should().ContainSingle(p => p.Name == PlayerName);
    }

    [Fact]
    [Trait("Method", nameof(RoomManager.JoinRoom))]
    public void JoinRoom_WithPlayerOverload_WhenConnectionAlreadyInRoom_ReturnsNull()
    {
        _sut.JoinRoom(RoomId, new Player(ConnectionId, PlayerName));

        var result = _sut.JoinRoom(RoomId, new Player(ConnectionId, "Alice2"));

        result.Should().BeNull();
    }

    [Fact]
    [Trait("Method", nameof(RoomManager.JoinRoom))]
    public void JoinRoom_WithBotPlayer_AddsIsBot()
    {
        var bot = new Player(ConnectionId, "Bot-1", isBot: true);

        var result = _sut.JoinRoom(RoomId, bot);

        result.Should().NotBeNull();
        _sut.GetBotPlayers(RoomId).Should().ContainSingle(p => p.Name == "Bot-1");
    }

    [Fact]
    [Trait("Method", nameof(RoomManager.LeaveRoom))]
    public void LeaveRoom_WhenRoomDoesNotExist_ReturnsNull()
    {
        var result = _sut.LeaveRoom(RoomId, ConnectionId);

        result.Should().BeNull();
    }

    [Fact]
    [Trait("Method", nameof(RoomManager.LeaveRoom))]
    public void LeaveRoom_WhenPlayerNotInRoom_ReturnsNull()
    {
        _sut.JoinRoom(RoomId, "conn-2", "Bob");

        var result = _sut.LeaveRoom(RoomId, ConnectionId);

        result.Should().BeNull();
    }

    [Fact]
    [Trait("Method", nameof(RoomManager.LeaveRoom))]
    public void LeaveRoom_WhenPlayerInRoom_ReturnsPlayerDto()
    {
        _sut.JoinRoom(RoomId, ConnectionId, PlayerName);

        var result = _sut.LeaveRoom(RoomId, ConnectionId);

        result.Should().NotBeNull();
        result!.Name.Should().Be(PlayerName);
    }

    [Fact]
    [Trait("Method", nameof(RoomManager.LeaveRoom))]
    public void LeaveRoom_WhenPlayerLeaves_IsRemovedFromRoom()
    {
        _sut.JoinRoom(RoomId, ConnectionId, PlayerName);
        _sut.JoinRoom(RoomId, "conn-2", "Bob");

        _sut.LeaveRoom(RoomId, ConnectionId);

        var room = _sut.GetRoom(RoomId);
        room!.Players.Should().ContainSingle(p => p.Name == "Bob");
    }

    [Fact]
    [Trait("Method", nameof(RoomManager.LeaveRoom))]
    public void LeaveRoom_WhenLastPlayerLeaves_RoomIsDeleted()
    {
        _sut.JoinRoom(RoomId, ConnectionId, PlayerName);

        _sut.LeaveRoom(RoomId, ConnectionId);

        _sut.GetRoom(RoomId).Should().BeNull();
    }

    [Fact]
    [Trait("Method", nameof(RoomManager.LeaveRoom))]
    public void LeaveRoom_WhenLastPlayerLeaves_ConnectionIsUnmapped()
    {
        _sut.JoinRoom(RoomId, ConnectionId, PlayerName);

        _sut.LeaveRoom(RoomId, ConnectionId);

        _sut.GetRoomFromConnection(ConnectionId).Should().BeNull();
    }

    [Fact]
    [Trait("Method", nameof(RoomManager.LeaveRoom))]
    public void LeaveRoom_WhenRoomStillHasPlayers_RoomIsNotDeleted()
    {
        _sut.JoinRoom(RoomId, ConnectionId, PlayerName);
        _sut.JoinRoom(RoomId, "conn-2", "Bob");

        _sut.LeaveRoom(RoomId, ConnectionId);

        _sut.GetRoom(RoomId).Should().NotBeNull();
    }

    [Fact]
    [Trait("Method", nameof(RoomManager.SubmitVote))]
    public void SubmitVote_WhenVoteIsInvalid_ThrowsInvalidVoteException()
    {
        _sut.JoinRoom(RoomId, ConnectionId, PlayerName);

        var act = () => _sut.SubmitVote(RoomId, ConnectionId, "999");

        act.Should().Throw<InvalidVoteException>();
    }

    [Fact]
    [Trait("Method", nameof(RoomManager.SubmitVote))]
    public void SubmitVote_WhenRoomDoesNotExist_ReturnsNull()
    {
        var result = _sut.SubmitVote(RoomId, ConnectionId, "5");

        result.Should().BeNull();
    }

    [Fact]
    [Trait("Method", nameof(RoomManager.SubmitVote))]
    public void SubmitVote_WhenPlayerNotInRoom_ReturnsNull()
    {
        _sut.JoinRoom(RoomId, "conn-2", "Bob");

        var result = _sut.SubmitVote(RoomId, ConnectionId, "5");

        result.Should().BeNull();
    }

    [Fact]
    [Trait("Method", nameof(RoomManager.SubmitVote))]
    public void SubmitVote_WhenCardsAreAlreadyRevealed_ReturnsNull()
    {
        _sut.JoinRoom(RoomId, ConnectionId, PlayerName);
        _sut.SubmitVote(RoomId, ConnectionId, "5");
        _sut.RevealCards(RoomId);

        var result = _sut.SubmitVote(RoomId, ConnectionId, "8");

        result.Should().BeNull();
    }

    [Fact]
    [Trait("Method", nameof(RoomManager.SubmitVote))]
    public void SubmitVote_WhenPlayerIsInRoom_ReturnsPlayerDto()
    {
        _sut.JoinRoom(RoomId, ConnectionId, PlayerName);

        var result = _sut.SubmitVote(RoomId, ConnectionId, "5");

        result.Should().NotBeNull();
        result!.Name.Should().Be(PlayerName);
    }

    [Fact]
    [Trait("Method", nameof(RoomManager.SubmitVote))]
    public void SubmitVote_WhenVoteIsSubmitted_PlayerHasVotedIsTrue()
    {
        _sut.JoinRoom(RoomId, ConnectionId, PlayerName);

        var result = _sut.SubmitVote(RoomId, ConnectionId, "5");

        result!.HasVoted.Should().BeTrue();
    }

    [Fact]
    [Trait("Method", nameof(RoomManager.SubmitVote))]
    public void SubmitVote_WhenVoteIsSubmitted_VoteIsNotExposedBeforeReveal()
    {
        _sut.JoinRoom(RoomId, ConnectionId, PlayerName);

        _sut.SubmitVote(RoomId, ConnectionId, "5");

        var room = _sut.GetRoom(RoomId);
        room!.Players.Should().ContainSingle(p => p.Vote == null && p.HasVoted);
    }

    [Fact]
    [Trait("Method", nameof(RoomManager.SubmitVote))]
    public void SubmitVote_WhenPlayerVotesMultipleTimes_VoteIsUpdated()
    {
        _sut.JoinRoom(RoomId, ConnectionId, PlayerName);
        _sut.SubmitVote(RoomId, ConnectionId, "3");

        _sut.SubmitVote(RoomId, ConnectionId, "8");

        _sut.RevealCards(RoomId);
        var room = _sut.GetRoom(RoomId);
        room!.Players.Should().ContainSingle(p => p.Vote == "8");
    }

    [Fact]
    [Trait("Method", nameof(RoomManager.RevealCards))]
    public void RevealCards_WhenRoomDoesNotExist_ReturnsNull()
    {
        var result = _sut.RevealCards(RoomId);

        result.Should().BeNull();
    }

    [Fact]
    [Trait("Method", nameof(RoomManager.RevealCards))]
    public void RevealCards_WhenAllPlayersHaveVoted_ReturnsRevealedRoom()
    {
        _sut.JoinRoom(RoomId, ConnectionId, PlayerName);
        _sut.JoinRoom(RoomId, "conn-2", "Bob");
        _sut.SubmitVote(RoomId, ConnectionId, "5");
        _sut.SubmitVote(RoomId, "conn-2", "8");

        var result = _sut.RevealCards(RoomId);

        result.Should().NotBeNull();
        result!.AreCardsRevealed.Should().BeTrue();
    }

    [Fact]
    [Trait("Method", nameof(RoomManager.RevealCards))]
    public void RevealCards_WhenAllPlayersHaveVoted_ExposesVotes()
    {
        _sut.JoinRoom(RoomId, ConnectionId, PlayerName);
        _sut.JoinRoom(RoomId, "conn-2", "Bob");
        _sut.SubmitVote(RoomId, ConnectionId, "5");
        _sut.SubmitVote(RoomId, "conn-2", "8");

        _sut.RevealCards(RoomId);

        var room = _sut.GetRoom(RoomId);
        room!.Players.Should().Contain(p => p.Vote == "5");
        room.Players.Should().Contain(p => p.Vote == "8");
    }

    [Fact]
    [Trait("Method", nameof(RoomManager.RevealCards))]
    public void RevealCards_WhenNotAllPlayersHaveVoted_ThrowsInvalidVoteException()
    {
        _sut.JoinRoom(RoomId, ConnectionId, PlayerName);
        _sut.JoinRoom(RoomId, "conn-2", "Bob");
        _sut.SubmitVote(RoomId, ConnectionId, "5");

        var act = () => _sut.RevealCards(RoomId);

        act.Should().Throw<InvalidVoteException>();
    }

    [Fact]
    [Trait("Method", nameof(RoomManager.RevealCards))]
    public void RevealCards_WhenNoPlayersHaveVoted_ThrowsInvalidVoteException()
    {
        _sut.JoinRoom(RoomId, ConnectionId, PlayerName);
        _sut.JoinRoom(RoomId, "conn-2", "Bob");

        var act = () => _sut.RevealCards(RoomId);

        act.Should().Throw<InvalidVoteException>();
    }

    [Fact]
    [Trait("Method", nameof(RoomManager.RevealCards))]
    public void RevealCards_WhenRoomExists_ReturnsRoomDto()
    {
        _sut.JoinRoom(RoomId, ConnectionId, PlayerName);
        _sut.SubmitVote(RoomId, ConnectionId, "5");

        var result = _sut.RevealCards(RoomId);

        result.Should().NotBeNull();
        result!.RoomId.Should().Be(RoomId);
    }

    [Fact]
    [Trait("Method", nameof(RoomManager.ResetTable))]
    public void ResetTable_WhenRoomDoesNotExist_ReturnsNull()
    {
        var result = _sut.ResetTable(RoomId);

        result.Should().BeNull();
    }

    [Fact]
    [Trait("Method", nameof(RoomManager.ResetTable))]
    public void ResetTable_WhenRoomExists_ReturnsRoomDto()
    {
        _sut.JoinRoom(RoomId, ConnectionId, PlayerName);

        var result = _sut.ResetTable(RoomId);

        result.Should().NotBeNull();
        result!.RoomId.Should().Be(RoomId);
    }

    [Fact]
    [Trait("Method", nameof(RoomManager.ResetTable))]
    public void ResetTable_ResetsAreCardsRevealedToFalse()
    {
        _sut.JoinRoom(RoomId, ConnectionId, PlayerName);
        _sut.SubmitVote(RoomId, ConnectionId, "5");
        _sut.RevealCards(RoomId);

        var result = _sut.ResetTable(RoomId);

        result!.AreCardsRevealed.Should().BeFalse();
    }

    [Fact]
    [Trait("Method", nameof(RoomManager.ResetTable))]
    public void ResetTable_ClearsAllPlayerVotes()
    {
        _sut.JoinRoom(RoomId, ConnectionId, PlayerName);
        _sut.JoinRoom(RoomId, "conn-2", "Bob");
        _sut.SubmitVote(RoomId, ConnectionId, "5");
        _sut.SubmitVote(RoomId, "conn-2", "8");
        _sut.RevealCards(RoomId);

        _sut.ResetTable(RoomId);

        var room = _sut.GetRoom(RoomId);
        room!.Players.Should().AllSatisfy(p =>
        {
            p.HasVoted.Should().BeFalse();
            p.Vote.Should().BeNull();
        });
    }

    [Fact]
    [Trait("Method", nameof(RoomManager.ResetTable))]
    public void ResetTable_PreservesPlayersInRoom()
    {
        _sut.JoinRoom(RoomId, ConnectionId, PlayerName);
        _sut.JoinRoom(RoomId, "conn-2", "Bob");
        _sut.SubmitVote(RoomId, ConnectionId, "5");
        _sut.SubmitVote(RoomId, "conn-2", "8");
        _sut.RevealCards(RoomId);

        var result = _sut.ResetTable(RoomId);

        result!.Players.Should().HaveCount(2);
    }

    [Fact]
    [Trait("Method", nameof(RoomManager.ResetTable))]
    public void ResetTable_AllowsNewVotingRound()
    {
        _sut.JoinRoom(RoomId, ConnectionId, PlayerName);
        _sut.SubmitVote(RoomId, ConnectionId, "5");
        _sut.RevealCards(RoomId);
        _sut.ResetTable(RoomId);

        var result = _sut.SubmitVote(RoomId, ConnectionId, "3");

        result.Should().NotBeNull();
        result!.HasVoted.Should().BeTrue();
    }

    [Fact]
    [Trait("Method", nameof(RoomManager.GetBotPlayers))]
    public void GetBotPlayers_WhenRoomDoesNotExist_ReturnsEmptyList()
    {
        var result = _sut.GetBotPlayers(RoomId);

        result.Should().BeEmpty();
    }

    [Fact]
    [Trait("Method", nameof(RoomManager.GetBotPlayers))]
    public void GetBotPlayers_WhenNoBotsInRoom_ReturnsEmptyList()
    {
        _sut.JoinRoom(RoomId, ConnectionId, PlayerName);

        var result = _sut.GetBotPlayers(RoomId);

        result.Should().BeEmpty();
    }

    [Fact]
    [Trait("Method", nameof(RoomManager.GetBotPlayers))]
    public void GetBotPlayers_WhenBotsAreInRoom_ReturnsBots()
    {
        var bot1 = new Player("bot-conn-1", "Bot-1", isBot: true);
        var bot2 = new Player("bot-conn-2", "Bot-2", isBot: true);
        _sut.JoinRoom(RoomId, bot1);
        _sut.JoinRoom(RoomId, bot2);

        var result = _sut.GetBotPlayers(RoomId);

        result.Should().HaveCount(2);
        result.Should().AllSatisfy(p => p.IsBot.Should().BeTrue());
    }

    [Fact]
    [Trait("Method", nameof(RoomManager.GetBotPlayers))]
    public void GetBotPlayers_WhenMixedPlayersInRoom_ReturnsOnlyBots()
    {
        _sut.JoinRoom(RoomId, ConnectionId, PlayerName);
        var bot = new Player("bot-conn-1", "Bot-1", isBot: true);
        _sut.JoinRoom(RoomId, bot);

        var result = _sut.GetBotPlayers(RoomId);

        result.Should().ContainSingle(p => p.IsBot);
        result.Should().NotContain(p => !p.IsBot);
    }

    [Fact]
    [Trait("Method", nameof(RoomManager.GetBotPlayers))]
    public void GetBotPlayers_WhenBotLeaves_IsNotReturnedAnymore()
    {
        var bot = new Player("bot-conn-1", "Bot-1", isBot: true);
        _sut.JoinRoom(RoomId, bot);

        _sut.LeaveRoom(RoomId, "bot-conn-1");

        _sut.GetBotPlayers(RoomId).Should().BeEmpty();
    }
}
