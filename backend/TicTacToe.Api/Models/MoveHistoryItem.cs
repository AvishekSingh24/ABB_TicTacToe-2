namespace TicTacToe.Api.Models;

/// <summary>
/// A single recorded move. Row/Column are derived from CellIndex (0-8, row-major)
/// so the frontend does not have to recompute them.
/// </summary>
public class MoveHistoryItem
{
    public int MoveNumber { get; set; }
    public Player Player { get; set; }
    public int CellIndex { get; set; }
    public int Row => CellIndex / 3;
    public int Column => CellIndex % 3;
    public bool WasComputerMove { get; set; }
}
