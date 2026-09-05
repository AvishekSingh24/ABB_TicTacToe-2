using TicTacToe.Api.Models;
using TicTacToe.Api.Services;
using Xunit;

namespace TicTacToe.Api.Tests;

public class GameServiceTests
{
    private static GameService NewService() =>
        new GameService(new ComputerPlayerService(), new ScoreboardService());

    private static GameState PlayMoves(GameService service, string gameId, params (Player player, int cell)[] moves)
    {
        GameState state = service.GetGame(gameId);
        foreach (var (player, cell) in moves)
        {
            state = service.MakeMove(gameId, new MoveRequest { Player = player, CellIndex = cell });
        }
        return state;
    }

    [Fact]
    public void ValidMove_IsApplied_AndAppearsOnBoard()
    {
        var service = NewService();
        var game = service.CreateGame(GameMode.TwoPlayer);

        var result = service.MakeMove(game.GameId, new MoveRequest { Player = Player.X, CellIndex = 0 });

        Assert.Equal(Player.X, result.Board[0]);
        Assert.Single(result.MoveHistory);
    }

    [Fact]
    public void InvalidMove_OnOccupiedCell_IsRejected()
    {
        var service = NewService();
        var game = service.CreateGame(GameMode.TwoPlayer);
        service.MakeMove(game.GameId, new MoveRequest { Player = Player.X, CellIndex = 0 });

        Assert.Throws<InvalidMoveException>(() =>
            service.MakeMove(game.GameId, new MoveRequest { Player = Player.O, CellIndex = 0 }));
    }

    [Fact]
    public void InvalidMove_OutOfRange_IsRejected()
    {
        var service = NewService();
        var game = service.CreateGame(GameMode.TwoPlayer);

        Assert.Throws<InvalidMoveException>(() =>
            service.MakeMove(game.GameId, new MoveRequest { Player = Player.X, CellIndex = 9 }));
    }

    [Fact]
    public void InvalidMove_ByWrongPlayer_IsRejected_AndTurnDoesNotChange()
    {
        var service = NewService();
        var game = service.CreateGame(GameMode.TwoPlayer);

        Assert.Throws<InvalidMoveException>(() =>
            service.MakeMove(game.GameId, new MoveRequest { Player = Player.O, CellIndex = 0 }));

        var current = service.GetGame(game.GameId);
        Assert.Equal(Player.X, current.CurrentPlayer);
        Assert.Empty(current.MoveHistory);
    }

    [Fact]
    public void TurnSwitches_AfterEachValidMove()
    {
        var service = NewService();
        var game = service.CreateGame(GameMode.TwoPlayer);

        var afterX = service.MakeMove(game.GameId, new MoveRequest { Player = Player.X, CellIndex = 0 });
        Assert.Equal(Player.O, afterX.CurrentPlayer);

        var afterO = service.MakeMove(game.GameId, new MoveRequest { Player = Player.O, CellIndex = 1 });
        Assert.Equal(Player.X, afterO.CurrentPlayer);
    }

    [Fact]
    public void RowWin_IsDetected()
    {
        var service = NewService();
        var game = service.CreateGame(GameMode.TwoPlayer);

        // X: 0,1,2 (top row) | O: 3,4
        var result = PlayMoves(service, game.GameId,
            (Player.X, 0), (Player.O, 3),
            (Player.X, 1), (Player.O, 4),
            (Player.X, 2));

        Assert.Equal(GameStatus.Won, result.Status);
        Assert.Equal(Player.X, result.Winner);
        Assert.Equal(new List<int> { 0, 1, 2 }, result.WinningCells);
    }

    [Fact]
    public void ColumnWin_IsDetected()
    {
        var service = NewService();
        var game = service.CreateGame(GameMode.TwoPlayer);

        // O: 1,4,7 (middle column) | X: 0,2,8
        var result = PlayMoves(service, game.GameId,
            (Player.X, 0), (Player.O, 1),
            (Player.X, 2), (Player.O, 4),
            (Player.X, 8), (Player.O, 7));

        Assert.Equal(GameStatus.Won, result.Status);
        Assert.Equal(Player.O, result.Winner);
        Assert.Equal(new List<int> { 1, 4, 7 }, result.WinningCells);
    }

    [Fact]
    public void DiagonalWin_IsDetected()
    {
        var service = NewService();
        var game = service.CreateGame(GameMode.TwoPlayer);

        // X: 0,4,8 (main diagonal) | O: 1,2
        var result = PlayMoves(service, game.GameId,
            (Player.X, 0), (Player.O, 1),
            (Player.X, 4), (Player.O, 2),
            (Player.X, 8));

        Assert.Equal(GameStatus.Won, result.Status);
        Assert.Equal(Player.X, result.Winner);
        Assert.Equal(new List<int> { 0, 4, 8 }, result.WinningCells);
    }

    [Fact]
    public void Draw_IsDetected_WhenBoardFillsWithNoWinner()
    {
        var service = NewService();
        var game = service.CreateGame(GameMode.TwoPlayer);

        // X O X
        // X O O
        // O X X
        var result = PlayMoves(service, game.GameId,
            (Player.X, 0), (Player.O, 1),
            (Player.X, 2), (Player.O, 4),
            (Player.X, 3), (Player.O, 5),
            (Player.X, 7), (Player.O, 6),
            (Player.X, 8));

        Assert.Equal(GameStatus.Draw, result.Status);
        Assert.Null(result.Winner);
    }

    [Fact]
    public void MoveAfterCompletion_IsRejected()
    {
        var service = NewService();
        var game = service.CreateGame(GameMode.TwoPlayer);
        PlayMoves(service, game.GameId,
            (Player.X, 0), (Player.O, 3),
            (Player.X, 1), (Player.O, 4),
            (Player.X, 2)); // X wins

        Assert.Throws<InvalidMoveException>(() =>
            service.MakeMove(game.GameId, new MoveRequest { Player = Player.O, CellIndex = 5 }));
    }

    [Fact]
    public void ResetGame_ClearsBoardAndHistory_ButKeepsScoreboardUntouched()
    {
        var computer = new ComputerPlayerService();
        var scoreboard = new ScoreboardService();
        var service = new GameService(computer, scoreboard);
        var game = service.CreateGame(GameMode.TwoPlayer);
        PlayMoves(service, game.GameId,
            (Player.X, 0), (Player.O, 3),
            (Player.X, 1), (Player.O, 4),
            (Player.X, 2)); // X wins -> scoreboard: XWins = 1

        var reset = service.ResetGame(game.GameId);

        Assert.All(reset.Board, cell => Assert.Null(cell));
        Assert.Empty(reset.MoveHistory);
        Assert.Equal(Player.X, reset.CurrentPlayer);
        Assert.Equal(GameStatus.InProgress, reset.Status);
        Assert.Equal(1, scoreboard.GetScoreboard().XWins); // untouched by reset
    }

    [Fact]
    public void Undo_InTwoPlayerMode_RemovesOnlyLastMove()
    {
        var service = NewService();
        var game = service.CreateGame(GameMode.TwoPlayer);
        PlayMoves(service, game.GameId, (Player.X, 0), (Player.O, 4));

        var result = service.UndoMove(game.GameId);

        Assert.Single(result.MoveHistory);
        Assert.Equal(Player.X, result.Board[0]);
        Assert.Null(result.Board[4]);
        Assert.Equal(Player.O, result.CurrentPlayer);
    }

    [Fact]
    public void Undo_InComputerMode_RemovesComputerAndPriorHumanMoveTogether()
    {
        var service = NewService();
        var game = service.CreateGame(GameMode.VsComputer);
        // Human plays X at 0; computer (O) auto-moves in the same call.
        var afterHumanMove = service.MakeMove(game.GameId, new MoveRequest { Player = Player.X, CellIndex = 0 });
        Assert.Equal(2, afterHumanMove.MoveHistory.Count); // human + computer move recorded

        var result = service.UndoMove(game.GameId);

        Assert.Empty(result.MoveHistory);
        Assert.All(result.Board, cell => Assert.Null(cell));
        Assert.Equal(Player.X, result.CurrentPlayer);
    }

    [Fact]
    public void Undo_IsRejected_WhenNoMovesExist()
    {
        var service = NewService();
        var game = service.CreateGame(GameMode.TwoPlayer);

        Assert.Throws<InvalidUndoException>(() => service.UndoMove(game.GameId));
    }

    [Fact]
    public void Undo_IsRejected_AfterGameCompletion()
    {
        var service = NewService();
        var game = service.CreateGame(GameMode.TwoPlayer);
        PlayMoves(service, game.GameId,
            (Player.X, 0), (Player.O, 3),
            (Player.X, 1), (Player.O, 4),
            (Player.X, 2)); // X wins

        Assert.Throws<InvalidUndoException>(() => service.UndoMove(game.GameId));
    }

    [Fact]
    public void Scoreboard_UpdatesOnce_WhenGameCompletes()
    {
        var scoreboard = new ScoreboardService();
        var service = new GameService(new ComputerPlayerService(), scoreboard);
        var game = service.CreateGame(GameMode.TwoPlayer);

        PlayMoves(service, game.GameId,
            (Player.X, 0), (Player.O, 3),
            (Player.X, 1), (Player.O, 4),
            (Player.X, 2)); // X wins

        Assert.Equal(1, scoreboard.GetScoreboard().XWins);
        Assert.Equal(0, scoreboard.GetScoreboard().OWins);
        Assert.Equal(0, scoreboard.GetScoreboard().Draws);
    }
}
