using TicTacToe.Api.Models;
using TicTacToe.Api.Services;
using Xunit;

namespace TicTacToe.Api.Tests;

public class ComputerPlayerServiceTests
{
    private static Player?[] Board(params (int index, Player player)[] cells)
    {
        var board = new Player?[9];
        foreach (var (index, player) in cells) board[index] = player;
        return board;
    }

    [Fact]
    public void TakesWinningMove_WhenAvailable()
    {
        var service = new ComputerPlayerService();
        // O has 0,1 -> should complete top row at 2, ignoring anything else.
        var board = Board((0, Player.O), (1, Player.O), (3, Player.X), (4, Player.X));

        var move = service.ChooseMove(board);

        Assert.Equal(2, move);
    }

    [Fact]
    public void BlocksOpponentWin_WhenNoOwnWinAvailable()
    {
        var service = new ComputerPlayerService();
        // X has 0,1 threatening to win at 2. O has no winning move of its own.
        var board = Board((0, Player.X), (1, Player.X), (4, Player.O));

        var move = service.ChooseMove(board);

        Assert.Equal(2, move);
    }

    [Fact]
    public void PrefersOwnWin_OverBlocking()
    {
        var service = new ComputerPlayerService();
        // O can win at 2 (row 0). X threatens to win at 8 (diagonal 0,4,8 has X,X already at 0,4... 
        // set up so both a winning move and a blocking move exist; winning must be chosen.
        var board = Board((0, Player.O), (1, Player.O), (3, Player.X), (4, Player.X));
        // O wins at 2; X would win at 5 if O didn't act, but that's not yet a real threat here.
        // Use a cleaner dual-threat setup instead:
        board = Board(
            (0, Player.O), (1, Player.O),   // O wins at 2
            (3, Player.X), (5, Player.X));  // X would win at 4 next turn

        var move = service.ChooseMove(board);

        Assert.Equal(2, move); // takes the win rather than blocking at 4
    }

    [Fact]
    public void TakesCenter_WhenNoWinOrBlockNeeded_AndCenterIsFree()
    {
        var service = new ComputerPlayerService();
        var board = Board((0, Player.X));

        var move = service.ChooseMove(board);

        Assert.Equal(4, move);
    }

    [Fact]
    public void TakesCorner_WhenCenterTaken_AndNoWinOrBlockNeeded()
    {
        var service = new ComputerPlayerService();
        var board = Board((4, Player.X));

        var move = service.ChooseMove(board);

        Assert.Contains(move, new int?[] { 0, 2, 6, 8 });
    }

    [Fact]
    public void TakesAnyAvailableCell_WhenCenterAndCornersTaken()
    {
        var service = new ComputerPlayerService();
        var board = Board(
            (4, Player.X), (0, Player.O), (2, Player.X), (6, Player.O), (8, Player.X));

        var move = service.ChooseMove(board);

        Assert.Contains(move, new int?[] { 1, 3, 5, 7 });
    }

    [Fact]
    public void ReturnsNull_WhenBoardIsFull()
    {
        var service = new ComputerPlayerService();
        var board = Board(
            (0, Player.X), (1, Player.O), (2, Player.X),
            (3, Player.X), (4, Player.O), (5, Player.O),
            (6, Player.O), (7, Player.X), (8, Player.X));

        var move = service.ChooseMove(board);

        Assert.Null(move);
    }
}
