namespace TicTacToe.Api.Services;

public class GameNotFoundException : Exception
{
    public GameNotFoundException(string gameId) : base($"Game '{gameId}' was not found.") { }
}

/// <summary>Thrown for any rejected move: out of range, occupied cell, wrong player, or completed game.</summary>
public class InvalidMoveException : Exception
{
    public InvalidMoveException(string message) : base(message) { }
}

public class InvalidUndoException : Exception
{
    public InvalidUndoException(string message) : base(message) { }
}
