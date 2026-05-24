using System.Collections.Concurrent;
using AgilePoker.Api.Constants;
using AgilePoker.Api.Exceptions;
using AgilePoker.Api.Hubs;
using AgilePoker.Api.Models;
using AgilePoker.Api.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace AgilePoker.Api.Services;

public class SimulationService : ISimulationService
{
    private readonly IHubContext<PokerHub> _hub;
    private readonly IRoomManager _roomManager;
    private readonly TimeProvider _timeProvider;
    private readonly Random _random = new (DateTime.Now.Millisecond);
    private readonly ConcurrentDictionary<string, Task> _botTimers = new();
    private readonly ConcurrentDictionary<string, CancellationTokenSource> _ctsDictionary = new();
    
    public SimulationService(IHubContext<PokerHub> hub, IRoomManager roomManager, TimeProvider timeProvider)
    {
        _hub  = hub;
        _roomManager  = roomManager;
        _timeProvider = timeProvider;
    }
    
    /// <summary>
    /// Adds a bot player to the room with a randomly selected name.
    /// The bot will asynchronously submit a vote after a random delay of 3–10 seconds.
    /// Returns false if the bot could not join the room.
    /// </summary>
    public async Task<bool> AddBot(string roomId)
    {
        var botName = AppConstants.BotNames[_random.Next(AppConstants.BotNames.Count)];
        var bot = new Player(Guid.NewGuid().ToString(), botName, true);
        var room = _roomManager.JoinRoom(roomId, bot);
        if (room is null) return false;
        
        var cts = _ctsDictionary.GetOrAdd(room.RoomId, new CancellationTokenSource());
        await _hub.Clients.Group(roomId).SendAsync("PlayerJoined", room, cts.Token);
        _botTimers.TryAdd(bot.ConnectionId, SubmitVote(roomId, bot, cts.Token));
        return true;
    }

    /// <summary>
    /// Cancels any in-flight bot vote tasks for the room, then schedules a fresh
    /// async vote for each bot so they participate in the new round.
    /// </summary>
    public void ResetTable(string roomId, List<Player> bots)
    {
        if(_ctsDictionary.Remove(roomId, out var cts))
            cts.Cancel();
            
        cts = _ctsDictionary.GetOrAdd(roomId, new CancellationTokenSource());
        foreach (var bot in bots)
            _botTimers.TryAdd(bot.ConnectionId, SubmitVote(roomId, bot, cts.Token));
    }
    
    private async Task SubmitVote(string roomId, Player bot, CancellationToken ctsToken)
    {
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(_random.Next(3, 8)), _timeProvider, ctsToken);
            var vote = AppConstants.Votes[_random.Next(AppConstants.Votes.Count)];
            
            var (botDto, allVoted) = _roomManager.SubmitVote(roomId, bot.ConnectionId, vote);
            
            await _hub.Clients.Group(roomId).SendAsync("VoteSubmitted", botDto);
            
            if (allVoted)
            {
                await Task.Delay(TimeSpan.FromSeconds(1), _timeProvider, ctsToken);
                var room = _roomManager.CheckRevealCards(roomId);
                if (room is not null)
                    await _hub.Clients.Group(roomId).SendAsync("CardsRevealed", room);
            }
        }
        catch (Exception e)
        {
            //TODO: Add logging
            Console.WriteLine(e);
        }
        finally
        {
            _botTimers.TryRemove(bot.ConnectionId, out _);
        }
    }
}