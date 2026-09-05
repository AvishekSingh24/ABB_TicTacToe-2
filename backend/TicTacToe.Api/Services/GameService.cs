using System.Collections.Concurrent;
using TicTacToe.Api.Models;

namespace TicTacToe.Api.Services;

/// <summary>
/// Owns all game sessions in memory. Registered as a singleton so state survives
/// across requests for the lifetime of the running backend process.
/// </summary>
public class GameService : IGameService
{
    private readonly ConcurrentDictionary<string, GameState> _games = new();
    private readonly IComputerPlayerService _computerPlayer;
    private readonly IScoreboardService _scoreboard;

    public GameService(IComputerPlayerService computerPlayer, IScoreboardService scoreboard)
    {
        _computerPlayer = computerPlayer;
        _scoreboard = scoreboard;
    }

    public GameState CreateGame(GameMode mode)
    {
        var game = new GameState { Mode = mode };
        _games[game.GameId] = game;
        return game;
    }

    public GameState GetGame(string gameId)
    {
        if (!_games.TryGetValue(gameId, out var game))
        {
            throw new GameNotFoundException(gameId);
        }
        return game;
    }

    public GameState MakeMove(string gameId, MoveRequest request)
    {
        var game = GetGame(gameId);

        lock (game)
        {
            if (game.Status != GameStatus.InProgress)
            {
                throw new InvalidMoveException("This game has already finished. Reset to play again.");
            }

            var cellIndex = ResolveCellIndex(request);

            if (cellIndex < 0 || cellIndex > 8)
            {
                throw new InvalidMoveException("Move is outside the board.");
            }

            if (request.Player != game.CurrentPlayer)
            {
                throw new InvalidMoveException($"It is not {request.Player}'s turn.");
            }

            if (game.Board[cellIndex].HasValue)
            {
                throw new InvalidMoveException("That cell is already occupied.");
            }

            ApplyMove(game, cellIndex, request.Player, wasComputerMove: false);

            // In Computer Mode, once it becomes O's turn the computer moves automatically,
            // as one continuous turn from the caller's point of view.
            if (game.Mode == GameMode.VsComputer &&
                game.Status == GameStatus.InProgress &&
                game.CurrentPlayer == Player.O)
            {
                var computerCell = _computerPlayer.ChooseMove(game.Board);
                if (computerCell.HasValue)
                {
                    ApplyMove(game, computerCell.Value, Player.O, wasComputerMove: true);
                }
            }

            return game;
        }
    }

    public GameState UndoMove(string gameId)
    {
        var game = GetGame(gameId);

        lock (game)
        {
            if (game.Status != GameStatus.InProgress)
            {
                // Clarification 2 - Option A: undo is disabled once a game is complete,
                // so the scoreboard for a finished game is always final.
                throw new InvalidUndoException("Undo is disabled once a game is complete.");
            }

            if (game.MoveHistory.Count == 0)
            {
                throw new InvalidUndoException("There are no moves to undo.");
            }

            var movesToRemove = 1;
            if (game.Mode == GameMode.VsComputer)
            {
                var lastMove = game.MoveHistory[^1];
                var hasPriorHumanMove = game.MoveHistory.Count >= 2 && !game.MoveHistory[^2].WasComputerMove;
                if (lastMove.WasComputerMove && hasPriorHumanMove)
                {
                    movesToRemove = 2;
                }
            }

            var remaining = game.MoveHistory
                .Take(game.MoveHistory.Count - movesToRemove)
                .ToList();

            RebuildFromHistory(game, remaining);

            return game;
        }
    }

    public GameState ResetGame(string gameId)
    {
        var game = GetGame(gameId);

        lock (game)
        {
            // Reset Game clears the board/history/status but intentionally leaves the
            // scoreboard untouched (see Functional Requirement 5).
            Array.Clear(game.Board);
            game.MoveHistory.Clear();
            game.CurrentPlayer = Player.X;
            game.Status = GameStatus.InProgress;
            game.Winner = null;
            game.WinningCells = null;
            game.ScoreboardUpdatedForThisGame = false;

            return game;
        }
    }

    private void ApplyMove(GameState game, int cellIndex, Player player, bool wasComputerMove)
    {
        game.Board[cellIndex] = player;
        game.MoveHistory.Add(new MoveHistoryItem
        {
            MoveNumber = game.MoveHistory.Count + 1,
            Player = player,
            CellIndex = cellIndex,
            WasComputerMove = wasComputerMove
        });

        var winningLine = WinChecker.FindWinningLine(game.Board, player);
        if (winningLine is not null)
        {
            game.Status = GameStatus.Won;
            game.Winner = player;
            game.WinningCells = winningLine;
            CompleteGameOnce(game, () => _scoreboard.RecordWin(player));
            return;
        }

        if (WinChecker.IsBoardFull(game.Board))
        {
            game.Status = GameStatus.Draw;
            CompleteGameOnce(game, () => _scoreboard.RecordDraw());
            return;
        }

        game.CurrentPlayer = player == Player.X ? Player.O : Player.X;
    }

    /// <summary>Ensures a completed game can only ever update the scoreboard once.</summary>
    private static void CompleteGameOnce(GameState game, Action recordResult)
    {
        if (game.ScoreboardUpdatedForThisGame) return;
        recordResult();
        game.ScoreboardUpdatedForThisGame = true;
    }

    /// <summary>
    /// Rebuilds board/current-player/status from a (possibly trimmed) move list.
    /// Used by Undo: replaying history from scratch is simpler and less error-prone
    /// than trying to reverse individual fields in place.
    /// </summary>
    private static void RebuildFromHistory(GameState game, List<MoveHistoryItem> history)
    {
        Array.Clear(game.Board);
        foreach (var move in history)
        {
            game.Board[move.CellIndex] = move.Player;
        }

        game.MoveHistory = history;
        game.Status = GameStatus.InProgress;
        game.Winner = null;
        game.WinningCells = null;
        game.ScoreboardUpdatedForThisGame = false;
        game.CurrentPlayer = history.Count == 0
            ? Player.X
            : (history[^1].Player == Player.X ? Player.O : Player.X);
    }

    private static int ResolveCellIndex(MoveRequest request)
    {
        if (request.CellIndex.HasValue) return request.CellIndex.Value;

        if (request.Row.HasValue && request.Column.HasValue)
        {
            if (request.Row is < 0 or > 2 || request.Column is < 0 or > 2) return -1;
            return request.Row.Value * 3 + request.Column.Value;
        }

        throw new InvalidMoveException("Move must include either cellIndex or both row and column.");
    }
}
