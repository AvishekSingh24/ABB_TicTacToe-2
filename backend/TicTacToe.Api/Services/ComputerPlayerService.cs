using TicTacToe.Api.Models;

namespace TicTacToe.Api.Services;

/// <summary>
/// Implements the required computer-move priority:
/// 1. Win if possible.
/// 2. Block X's winning move if X could win next.
/// 3. Take center.
/// 4. Take a corner.
/// 5. Take any available cell.
/// The computer always plays as O.
/// </summary>
public class ComputerPlayerService : IComputerPlayerService
{
    private static readonly int Center = 4;
    private static readonly int[] Corners = { 0, 2, 6, 8 };

    public int? ChooseMove(Player?[] board)
    {
        var empty = EmptyCells(board);
        if (empty.Count == 0) return null;

        // 1. Win if possible.
        var winningMove = FindMoveCompletingLine(board, Player.O, empty);
        if (winningMove.HasValue) return winningMove;

        // 2. Block X's winning move.
        var blockingMove = FindMoveCompletingLine(board, Player.X, empty);
        if (blockingMove.HasValue) return blockingMove;

        // 3. Take center.
        if (empty.Contains(Center)) return Center;

        // 4. Take a corner.
        var availableCorner = Corners.FirstOrDefault(c => empty.Contains(c), -1);
        if (availableCorner != -1) return availableCorner;

        // 5. Take any available cell.
        return empty[0];
    }

    private static List<int> EmptyCells(Player?[] board)
    {
        var cells = new List<int>();
        for (var i = 0; i < board.Length; i++)
        {
            if (board[i] is null) cells.Add(i);
        }
        return cells;
    }

    /// <summary>
    /// Returns an empty cell that, if filled with `player`, completes a winning line - or null.
    /// </summary>
    private static int? FindMoveCompletingLine(Player?[] board, Player player, List<int> empty)
    {
        foreach (var cell in empty)
        {
            var trial = (Player?[])board.Clone();
            trial[cell] = player;
            if (WinChecker.FindWinningLine(trial, player) is not null)
            {
                return cell;
            }
        }
        return null;
    }
}
