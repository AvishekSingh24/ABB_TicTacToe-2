using TicTacToe.Api.Models;

namespace TicTacToe.Api.Services;

public static class WinChecker
{
    // All 8 winning lines: 3 rows, 3 columns, 2 diagonals.
    public static readonly int[][] Lines =
    {
        new[] { 0, 1, 2 }, new[] { 3, 4, 5 }, new[] { 6, 7, 8 }, // rows
        new[] { 0, 3, 6 }, new[] { 1, 4, 7 }, new[] { 2, 5, 8 }, // columns
        new[] { 0, 4, 8 }, new[] { 2, 4, 6 }                     // diagonals
    };

    /// <summary>Returns the winning line (3 cell indices) for the given player, or null if none.</summary>
    public static List<int>? FindWinningLine(Player?[] board, Player player)
    {
        foreach (var line in Lines)
        {
            if (line.All(i => board[i] == player))
            {
                return line.ToList();
            }
        }
        return null;
    }

    public static bool IsBoardFull(Player?[] board) => board.All(c => c.HasValue);
}
