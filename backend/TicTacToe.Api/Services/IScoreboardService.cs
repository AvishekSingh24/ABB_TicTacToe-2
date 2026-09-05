using TicTacToe.Api.Models;

namespace TicTacToe.Api.Services;

public interface IScoreboardService
{
    Scoreboard GetScoreboard();
    void RecordWin(Player winner);
    void RecordDraw();
    void UndoLastRecordedResult(GameStatus previousStatus, Player? previousWinner);
    void Reset();
}
