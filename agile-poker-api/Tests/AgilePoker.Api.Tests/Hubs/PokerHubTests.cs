using AgilePoker.Api.DTOs;
using AgilePoker.Api.Exceptions;
using AgilePoker.Api.Hubs;
using AgilePoker.Api.Models;
using AgilePoker.Api.Services.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace AgilePoker.Api.Tests;

public class PokerHubTests
{
    private readonly PokerHub _sut;
    private readonly IRoomManager _roomManager;
    private readonly ISimulationService _simulatorService;
    private readonly IHubCallerClients _hubClients;
    private readonly ISingleClientProxy _callerProxy;
    private readonly IClientProxy _groupProxy;
    private readonly IGroupManager _groups;
    private readonly FakeTimeProvider _fakeTime;

    private const string RoomId = "room-1";
    private const string ConnectionId = "conn-1";
    private const string PlayerName = "Alice";

    public PokerHubTests()
    {
        _roomManager = Substitute.For<IRoomManager>();
        _simulatorService = Substitute.For<ISimulationService>();
        _fakeTime = new FakeTimeProvider();

        var context = Substitute.For<HubCallerContext>();
        context.ConnectionId.Returns(ConnectionId);

        _callerProxy = Substitute.For<ISingleClientProxy>();
        _groupProxy = Substitute.For<IClientProxy>();

        _hubClients = Substitute.For<IHubCallerClients>();
        _hubClients.Caller.Returns(_callerProxy);
        _hubClients.Group(Arg.Any<string>()).Returns(_groupProxy);

        _groups = Substitute.For<IGroupManager>();
        _groups
            .AddToGroupAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        _groups
            .RemoveFromGroupAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        _callerProxy
            .SendCoreAsync(Arg.Any<string>(), Arg.Any<object[]>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        _groupProxy
            .SendCoreAsync(Arg.Any<string>(), Arg.Any<object[]>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        _sut = new PokerHub(_roomManager, _simulatorService, _fakeTime)
        {
            Context = context,
            Clients = _hubClients,
            Groups = _groups
        };
    }

    [Fact]
    [Trait("Method", nameof(PokerHub.JoinRoom))]
    public async Task JoinRoom_WhenJoinSucceeds_AddsCallerToGroup()
    {
        _roomManager.JoinRoom(RoomId, ConnectionId, PlayerName).Returns(new RoomDTO(RoomId));

        await _sut.JoinRoom(RoomId, PlayerName);

        await _groups.Received(1).AddToGroupAsync(ConnectionId, RoomId, Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("Method", nameof(PokerHub.JoinRoom))]
    public async Task JoinRoom_WhenJoinSucceeds_SendsPlayerJoinedToGroup()
    {
        _roomManager.JoinRoom(RoomId, ConnectionId, PlayerName).Returns(new RoomDTO(RoomId));

        await _sut.JoinRoom(RoomId, PlayerName);

        await _groupProxy.Received(1).SendCoreAsync(
            "PlayerJoined", Arg.Any<object[]>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("Method", nameof(PokerHub.JoinRoom))]
    public async Task JoinRoom_WhenJoinFails_SendsJoinRoomFailedToCaller()
    {
        _roomManager.JoinRoom(RoomId, ConnectionId, PlayerName).Returns((RoomDTO?)null);

        await _sut.JoinRoom(RoomId, PlayerName);

        await _callerProxy.Received(1).SendCoreAsync(
            "JoinRoomFailed", Arg.Any<object[]>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("Method", nameof(PokerHub.JoinRoom))]
    public async Task JoinRoom_WhenJoinFails_DoesNotAddCallerToGroup()
    {
        _roomManager.JoinRoom(RoomId, ConnectionId, PlayerName).Returns((RoomDTO?)null);

        await _sut.JoinRoom(RoomId, PlayerName);

        await _groups.DidNotReceive().AddToGroupAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("Method", nameof(PokerHub.OnDisconnectedAsync))]
    public async Task OnDisconnectedAsync_WhenConnectionHasRoom_SendsPlayerLeftToGroup()
    {
        _roomManager.GetRoomFromConnection(ConnectionId).Returns(new RoomDTO(RoomId));
        _roomManager.LeaveRoom(RoomId, ConnectionId).Returns(new PlayerDTO(Guid.NewGuid(), PlayerName));

        await _sut.OnDisconnectedAsync(null);

        await _groupProxy.Received(1).SendCoreAsync(
            "PlayerLeft", Arg.Any<object[]>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("Method", nameof(PokerHub.OnDisconnectedAsync))]
    public async Task OnDisconnectedAsync_WhenConnectionHasRoom_DoesNotRemoveCallerFromGroup()
    {
        _roomManager.GetRoomFromConnection(ConnectionId).Returns(new RoomDTO(RoomId));
        _roomManager.LeaveRoom(RoomId, ConnectionId).Returns(new PlayerDTO(Guid.NewGuid(), PlayerName));

        await _sut.OnDisconnectedAsync(null);

        await _groups.DidNotReceive().RemoveFromGroupAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("Method", nameof(PokerHub.OnDisconnectedAsync))]
    public async Task OnDisconnectedAsync_WhenConnectionHasNoRoom_DoesNotSendPlayerLeft()
    {
        _roomManager.GetRoomFromConnection(ConnectionId).Returns((RoomDTO?)null);

        await _sut.OnDisconnectedAsync(null);

        await _groupProxy.DidNotReceive().SendCoreAsync(
            "PlayerLeft", Arg.Any<object[]>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("Method", nameof(PokerHub.LeaveRoom))]
    public async Task LeaveRoom_WhenPlayerInRoom_SendsPlayerLeftToGroup()
    {
        _roomManager.LeaveRoom(RoomId, ConnectionId).Returns(new PlayerDTO(Guid.NewGuid(), PlayerName));

        await _sut.LeaveRoom(RoomId);

        await _groupProxy.Received(1).SendCoreAsync(
            "PlayerLeft", Arg.Any<object[]>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("Method", nameof(PokerHub.LeaveRoom))]
    public async Task LeaveRoom_WhenPlayerInRoom_RemovesCallerFromGroup()
    {
        _roomManager.LeaveRoom(RoomId, ConnectionId).Returns(new PlayerDTO(Guid.NewGuid(), PlayerName));

        await _sut.LeaveRoom(RoomId);

        await _groups.Received(1).RemoveFromGroupAsync(ConnectionId, RoomId, Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("Method", nameof(PokerHub.LeaveRoom))]
    public async Task LeaveRoom_WhenPlayerNotInRoom_SendsLeaveRoomFailedToCaller()
    {
        _roomManager.LeaveRoom(RoomId, ConnectionId).Returns((PlayerDTO?)null);

        await _sut.LeaveRoom(RoomId);

        await _callerProxy.Received(1).SendCoreAsync(
            "LeaveRoomFailed", Arg.Any<object[]>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("Method", nameof(PokerHub.LeaveRoom))]
    public async Task LeaveRoom_WhenPlayerNotInRoom_DoesNotRemoveCallerFromGroup()
    {
        _roomManager.LeaveRoom(RoomId, ConnectionId).Returns((PlayerDTO?)null);

        await _sut.LeaveRoom(RoomId);

        await _groups.DidNotReceive().RemoveFromGroupAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("Method", nameof(PokerHub.LeaveRoom))]
    public async Task LeaveRoom_WhenOnDisconnectedAndPlayerNotInRoom_DoesNotSendLeaveRoomFailed()
    {
        _roomManager.LeaveRoom(RoomId, ConnectionId).Returns((PlayerDTO?)null);

        await _sut.LeaveRoom(RoomId, onDisconnected: true);

        await _callerProxy.DidNotReceive().SendCoreAsync(
            "LeaveRoomFailed", Arg.Any<object[]>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("Method", nameof(PokerHub.LeaveRoom))]
    public async Task LeaveRoom_WhenOnDisconnectedAndPlayerInRoom_DoesNotRemoveCallerFromGroup()
    {
        _roomManager.LeaveRoom(RoomId, ConnectionId).Returns(new PlayerDTO(Guid.NewGuid(), PlayerName));

        await _sut.LeaveRoom(RoomId, onDisconnected: true);

        await _groups.DidNotReceive().RemoveFromGroupAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("Method", nameof(PokerHub.SubmitVote))]
    public async Task SubmitVote_WhenPlayerIsNull_SendsSubmitVoteFailedToCaller()
    {
        _roomManager.SubmitVote(RoomId, ConnectionId, "5").Returns(((PlayerDTO?)null, false));

        await _sut.SubmitVote(RoomId, "5");

        await _callerProxy.Received(1).SendCoreAsync(
            "SubmitVoteFailed", Arg.Any<object[]>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("Method", nameof(PokerHub.SubmitVote))]
    public async Task SubmitVote_WhenPlayerFound_SendsVoteSubmittedToGroup()
    {
        var playerDto = new PlayerDTO(Guid.NewGuid(), PlayerName);
        _roomManager.SubmitVote(RoomId, ConnectionId, "5").Returns((playerDto, false));

        await _sut.SubmitVote(RoomId, "5");

        await _groupProxy.Received(1).SendCoreAsync(
            "VoteSubmitted", Arg.Any<object[]>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("Method", nameof(PokerHub.SubmitVote))]
    public async Task SubmitVote_WhenNotAllVoted_DoesNotSendCardsRevealed()
    {
        var playerDto = new PlayerDTO(Guid.NewGuid(), PlayerName);
        _roomManager.SubmitVote(RoomId, ConnectionId, "5").Returns((playerDto, false));

        await _sut.SubmitVote(RoomId, "5");

        await _groupProxy.DidNotReceive().SendCoreAsync(
            "CardsRevealed", Arg.Any<object[]>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("Method", nameof(PokerHub.SubmitVote))]
    public async Task SubmitVote_WhenAllVotedAndRevealSucceeds_SendsCardsRevealedToGroup()
    {
        var playerDto = new PlayerDTO(Guid.NewGuid(), PlayerName);
        _roomManager.SubmitVote(RoomId, ConnectionId, "5").Returns((playerDto, true));
        _roomManager.CheckRevealCards(RoomId).Returns(new RoomDTO(RoomId));

        var submitTask = _sut.SubmitVote(RoomId, "5");
        _fakeTime.Advance(TimeSpan.FromSeconds(1));
        await submitTask;

        await _groupProxy.Received(1).SendCoreAsync(
            "CardsRevealed", Arg.Any<object[]>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("Method", nameof(PokerHub.SubmitVote))]
    public async Task SubmitVote_WhenAllVotedButRevealFails_DoesNotSendCardsRevealed()
    {
        var playerDto = new PlayerDTO(Guid.NewGuid(), PlayerName);
        _roomManager.SubmitVote(RoomId, ConnectionId, "5").Returns((playerDto, true));
        _roomManager.CheckRevealCards(RoomId).Returns((RoomDTO?)null);

        var submitTask = _sut.SubmitVote(RoomId, "5");
        _fakeTime.Advance(TimeSpan.FromSeconds(1));
        await submitTask;

        await _groupProxy.DidNotReceive().SendCoreAsync(
            "CardsRevealed", Arg.Any<object[]>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("Method", nameof(PokerHub.SubmitVote))]
    public async Task SubmitVote_WhenInvalidVoteException_SendsSubmitVoteFailedToCaller()
    {
        _roomManager.When(rm => rm.SubmitVote(RoomId, ConnectionId, Arg.Any<string>()))
                    .Do(_ => throw new InvalidVoteException("Invalid vote"));

        await _sut.SubmitVote(RoomId, "999");

        await _callerProxy.Received(1).SendCoreAsync(
            "SubmitVoteFailed", Arg.Any<object[]>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("Method", nameof(PokerHub.ResetTable))]
    public async Task ResetTable_WhenRoomNotFound_SendsResetTableFailedToCaller()
    {
        _roomManager.ResetTable(RoomId).Returns((RoomDTO?)null);

        await _sut.ResetTable(RoomId);

        await _callerProxy.Received(1).SendCoreAsync(
            "ResetTableFailed", Arg.Any<object[]>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("Method", nameof(PokerHub.ResetTable))]
    public async Task ResetTable_WhenRoomFound_SendsTableResetToGroup()
    {
        _roomManager.ResetTable(RoomId).Returns(new RoomDTO(RoomId));
        _roomManager.GetBotPlayers(RoomId).Returns([]);

        await _sut.ResetTable(RoomId);

        await _groupProxy.Received(1).SendCoreAsync(
            "TableReset", Arg.Any<object[]>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("Method", nameof(PokerHub.ResetTable))]
    public async Task ResetTable_WhenRoomFound_CallsSimulatorResetTable()
    {
        var bots = new List<Player> { new Player("bot-conn-1", "Bot-1", isBot: true) };
        _roomManager.ResetTable(RoomId).Returns(new RoomDTO(RoomId));
        _roomManager.GetBotPlayers(RoomId).Returns(bots);

        await _sut.ResetTable(RoomId);

        _simulatorService.Received(1).ResetTable(RoomId, bots);
    }

    [Fact]
    [Trait("Method", nameof(PokerHub.ResetTable))]
    public async Task ResetTable_WhenRoomNotFound_DoesNotCallSimulator()
    {
        _roomManager.ResetTable(RoomId).Returns((RoomDTO?)null);

        await _sut.ResetTable(RoomId);

        _simulatorService.DidNotReceive().ResetTable(Arg.Any<string>(), Arg.Any<List<Player>>());
    }

    [Fact]
    [Trait("Method", nameof(PokerHub.AddBot))]
    public async Task AddBot_DelegatesToSimulationService()
    {
        _simulatorService.AddBot(RoomId).Returns(true);

        await _sut.AddBot(RoomId);

        await _simulatorService.Received(1).AddBot(RoomId);
    }

    [Fact]
    [Trait("Method", nameof(PokerHub.AddBot))]
    public async Task AddBot_ReturnsResultFromSimulationService()
    {
        _simulatorService.AddBot(RoomId).Returns(false);

        var result = await _sut.AddBot(RoomId);

        result.Should().BeFalse();
    }
}
