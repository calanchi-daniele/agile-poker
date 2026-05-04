using AgilePoker.Api.Exceptions;
using AgilePoker.Api.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace AgilePoker.Api.Hubs;

public class PokerHub : Hub
{
    private readonly IRoomManager _roomManager;
    private readonly ISimulationService _simulatorService;

    public PokerHub(IRoomManager roomManager, ISimulationService simulatorService)
    {
        _roomManager = roomManager;
        _simulatorService = simulatorService;
    }
    
    public async Task JoinRoom(string roomId, string playerName)
    {
        var room = _roomManager.JoinRoom(roomId, Context.ConnectionId, playerName);
        if (room is not null)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
            await Clients.Group(roomId).SendAsync("PlayerJoined", room);
        }
        else
        {
            await Clients.Caller.SendAsync("JoinRoomFailed");
        }
    }
    
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var room = _roomManager.GetRoomFromConnection(Context.ConnectionId);
        if (room is not null)
        {
            await LeaveRoom(room.RoomId, true);
        }

        await base.OnDisconnectedAsync(exception);
    }
    
    public async Task LeaveRoom(string roomId, bool onDisconnected = false)
    {
        var player = _roomManager.LeaveRoom(roomId, Context.ConnectionId);

        if (player is null)
        {
            if(!onDisconnected)
                await Clients.Caller.SendAsync("LeaveRoomFailed");
            return;
        }

        await Clients.Group(roomId).SendAsync("PlayerLeft", player);
        
        if(!onDisconnected)
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);
    }

    public async Task SubmitVote(string roomId, string vote)
    {
        try
        {
            var (player, allVoted) = _roomManager.SubmitVote(roomId, Context.ConnectionId, vote);
            
            if (player is null) 
            {
                await Clients.Caller.SendAsync("SubmitVoteFailed");
                return;
            }
            
            await Clients.Group(roomId).SendAsync("VoteSubmitted", player);
            
            if (allVoted)
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
                var room = _roomManager.CheckRevealCards(roomId);
                if (room is not null)
                    await Clients.Group(roomId).SendAsync("CardsRevealed", room);
            }
        }
        catch (InvalidVoteException e)
        {
            await Clients.Caller.SendAsync("SubmitVoteFailed", e);
        }
    }

    public async Task ResetTable(string roomId)
    {
        var room = _roomManager.ResetTable(roomId);
        if (room is null)
            await Clients.Caller.SendAsync("ResetTableFailed");
        else
        {
            await Clients.Group(roomId).SendAsync("TableReset", room);
            var bots = _roomManager.GetBotPlayers(roomId);
            _simulatorService.ResetTable(roomId, bots);
        }
    }

    public Task<bool> AddBot(string roomId)
    {
        return _simulatorService.AddBot(roomId);
    }
}