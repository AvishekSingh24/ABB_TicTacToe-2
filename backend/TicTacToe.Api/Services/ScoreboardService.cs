using TicTacToe.Api.Models;

namespace TicTacToe.Api.Services;

/// <summary>
/// Session-level scoreboard, shared across all games. Registered as a singleton.
/// A simple lock is sufficient here: scoreboard updates are rare (once per completed game)
/// compared to reads, and correctness (never double-counting) matters more than throughput.
/// </summary>
public class ScoreboardService : IScoreboardService
{
    private readonly object _lock = new();
    private readonly Scoreboard _scoreboard = new();

    public Scoreboard GetScoreboard()
    {
        lock (_lock)
        {
            return new Scoreboard
            {
                XWins = _scoreboard.XWins,
                OWins = _scoreboard.OWins,
                Draws = _scoreboard.Draws
            };
        }
    }

    public void RecordWin(Player winner)
    {
        lock (_lock)
        {
            if (winner == Player.X) _scoreboard.XWins++;
            else _scoreboard.OWins++;
        }
    }

    public void RecordDraw()
    {
        lock (_lock)
        {
            _scoreboard.Draws++;
        }
    }

    /// <summary>
    /// Used only if a completed result needs to be reversed (kept for completeness /
    /// Option B compatibility - see README Clarification 2. Not reachable via the API
    /// as shipped, since undo is disabled once a game is complete).
    /// </summary>
    public void UndoLastRecordedResult(GameStatus previousStatus, Player? previousWinner)
    {
        lock (_lock)
        {
            if (previousStatus == GameStatus.Won && previousWinner.HasValue)
            {
                if (previousWinner == Player.X) _scoreboard.XWins = Math.Max(0, _scoreboard.XWins - 1);
                else _scoreboard.OWins = Math.Max(0, _scoreboard.OWins - 1);
            }
            else if (previousStatus == GameStatus.Draw)
            {
                _scoreboard.Draws = Math.Max(0, _scoreboard.Draws - 1);
            }
        }
    }

    public void Reset()
    {
        lock (_lock)
        {
            _scoreboard.XWins = 0;
            _scoreboard.OWins = 0;
            _scoreboard.Draws = 0;
        }
    }
}
