using AgilePoker.Api.DTOs;
using AgilePoker.Api.Models;

namespace AgilePoker.Api.Services.Interfaces;

public interface IRoomManager
{
    RoomDTO? GetRoomFromConnection(string connectionId);
    List<Player> GetBotPlayers(string roomId);
    RoomDTO? JoinRoom(string roomId, string connectionId, string playerName);
    RoomDTO? JoinRoom(string roomId, Player player);
    PlayerDTO? LeaveRoom(string roomId, string connectionId);

    RoomDTO?  CheckRevealCards(string roomId);
    (PlayerDTO? player, bool allVoted) SubmitVote(string roomId, string connectionId, string vote);
    RoomDTO? ResetTable(string roomId);
}