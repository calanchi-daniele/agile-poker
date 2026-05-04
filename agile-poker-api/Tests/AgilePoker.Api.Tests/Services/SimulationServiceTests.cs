using AgilePoker.Api.Constants;
using AgilePoker.Api.DTOs;
using AgilePoker.Api.Hubs;
using AgilePoker.Api.Models;
using AgilePoker.Api.Services;
using AgilePoker.Api.Services.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;

namespace AgilePoker.Api.Tests;

public class SimulationServiceTests
{
    private readonly SimulationService _sut;
    private readonly IRoomManager _roomManager;
    private readonly IClientProxy _clientProxy;

    private const string RoomId = "room-1";

    public SimulationServiceTests()
    {
        _roomManager = Substitute.For<IRoomManager>();

        var hubContext = Substitute.For<IHubContext<PokerHub>>();
        var hubClients = Substitute.For<IHubClients>();
        _clientProxy = Substitute.For<IClientProxy>();

        hubContext.Clients.Returns(hubClients);
        hubClients.Group(Arg.Any<string>()).Returns(_clientProxy);
        _clientProxy
            .SendCoreAsync(Arg.Any<string>(), Arg.Any<object[]>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        _sut = new SimulationService(hubContext, _roomManager, new FakeTimeProvider());
    }

    [Fact]
    [Trait("Method", nameof(SimulationService.AddBot))]
    public async Task AddBot_WhenJoinRoomFails_ReturnsFalse()
    {
        _roomManager.JoinRoom(RoomId, Arg.Any<Player>()).Returns((RoomDTO?)null);

        var result = await _sut.AddBot(RoomId);

        result.Should().BeFalse();
    }

    [Fact]
    [Trait("Method", nameof(SimulationService.AddBot))]
    public async Task AddBot_WhenJoinRoomSucceeds_ReturnsTrue()
    {
        _roomManager.JoinRoom(RoomId, Arg.Any<Player>()).Returns(new RoomDTO(RoomId));

        var result = await _sut.AddBot(RoomId);

        result.Should().BeTrue();
    }

    [Fact]
    [Trait("Method", nameof(SimulationService.AddBot))]
    public async Task AddBot_WhenJoinRoomSucceeds_JoinsPlayerWithIsBotTrue()
    {
        _roomManager.JoinRoom(RoomId, Arg.Any<Player>()).Returns(new RoomDTO(RoomId));

        await _sut.AddBot(RoomId);

        _roomManager.Received(1).JoinRoom(RoomId, Arg.Is<Player>(p => p.IsBot));
    }

    [Fact]
    [Trait("Method", nameof(SimulationService.AddBot))]
    public async Task AddBot_WhenJoinRoomSucceeds_BotNameIsFromAppConstants()
    {
        _roomManager.JoinRoom(RoomId, Arg.Any<Player>()).Returns(new RoomDTO(RoomId));

        await _sut.AddBot(RoomId);

        _roomManager.Received(1).JoinRoom(
            RoomId,
            Arg.Is<Player>(p => AppConstants.BotNames.Contains(p.Name))
        );
    }

    [Fact]
    [Trait("Method", nameof(SimulationService.AddBot))]
    public async Task AddBot_WhenJoinRoomSucceeds_SendsPlayerJoinedToRoom()
    {
        _roomManager.JoinRoom(RoomId, Arg.Any<Player>()).Returns(new RoomDTO(RoomId));

        await _sut.AddBot(RoomId);

        await _clientProxy.Received(1).SendCoreAsync(
            "PlayerJoined",
            Arg.Any<object[]>(),
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    [Trait("Method", nameof(SimulationService.AddBot))]
    public async Task AddBot_WhenJoinRoomFails_DoesNotNotifyHub()
    {
        _roomManager.JoinRoom(RoomId, Arg.Any<Player>()).Returns((RoomDTO?)null);

        await _sut.AddBot(RoomId);

        await _clientProxy.DidNotReceive().SendCoreAsync(
            Arg.Any<string>(),
            Arg.Any<object[]>(),
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    [Trait("Method", nameof(SimulationService.ResetTable))]
    public void ResetTable_WithEmptyBotList_DoesNotThrow()
    {
        var act = () => _sut.ResetTable(RoomId, []);

        act.Should().NotThrow();
    }

    [Fact]
    [Trait("Method", nameof(SimulationService.ResetTable))]
    public void ResetTable_WithBots_DoesNotThrow()
    {
        var bot1 = new Player("bot-conn-1", "Bot-1", isBot: true);
        var bot2 = new Player("bot-conn-2", "Bot-2", isBot: true);

        var act = () => _sut.ResetTable(RoomId, [bot1, bot2]);

        act.Should().NotThrow();
    }

    [Fact]
    [Trait("Method", nameof(SimulationService.ResetTable))]
    public async Task ResetTable_WhenCalledAfterAddBot_DoesNotThrow()
    {
        _roomManager.JoinRoom(RoomId, Arg.Any<Player>()).Returns(new RoomDTO(RoomId));
        await _sut.AddBot(RoomId);
        var bot = new Player("bot-conn-1", "Bot-1", isBot: true);

        var act = () => _sut.ResetTable(RoomId, [bot]);

        act.Should().NotThrow();
    }
}
