using Microsoft.AspNetCore.Mvc;
using TicTacToe.Api.Models;
using TicTacToe.Api.Services;

namespace TicTacToe.Api.Controllers;

[ApiController]
[Route("api/games")]
public class GamesController : ControllerBase
{
    private readonly IGameService _gameService;
    private readonly IScoreboardService _scoreboard;

    public GamesController(IGameService gameService, IScoreboardService scoreboard)
    {
        _gameService = gameService;
        _scoreboard = scoreboard;
    }

    /// <summary>Create a new game session.</summary>
    [HttpPost]
    public ActionResult<GameStateResponse> CreateGame([FromBody] CreateGameRequest request)
    {
        var game = _gameService.CreateGame(request.Mode);
        return Ok(GameStateResponse.FromGameState(game, _scoreboard.GetScoreboard()));
    }

    /// <summary>Get the current state of a game session.</summary>
    [HttpGet("{id}")]
    public ActionResult<GameStateResponse> GetGame(string id)
    {
        try
        {
            var game = _gameService.GetGame(id);
            return Ok(GameStateResponse.FromGameState(game, _scoreboard.GetScoreboard()));
        }
        catch (GameNotFoundException ex)
        {
            return NotFound(new ApiError { Message = ex.Message });
        }
    }

    /// <summary>Submit a move for the current player.</summary>
    [HttpPost("{id}/moves")]
    public ActionResult<GameStateResponse> MakeMove(string id, [FromBody] MoveRequest request)
    {
        try
        {
            var game = _gameService.MakeMove(id, request);
            return Ok(GameStateResponse.FromGameState(game, _scoreboard.GetScoreboard()));
        }
        catch (GameNotFoundException ex)
        {
            return NotFound(new ApiError { Message = ex.Message });
        }
        catch (InvalidMoveException ex)
        {
            return BadRequest(new ApiError { Message = ex.Message });
        }
    }

    /// <summary>Undo the last move (or move pair in Computer Mode).</summary>
    [HttpPost("{id}/undo")]
    public ActionResult<GameStateResponse> Undo(string id)
    {
        try
        {
            var game = _gameService.UndoMove(id);
            return Ok(GameStateResponse.FromGameState(game, _scoreboard.GetScoreboard()));
        }
        catch (GameNotFoundException ex)
        {
            return NotFound(new ApiError { Message = ex.Message });
        }
        catch (InvalidUndoException ex)
        {
            return BadRequest(new ApiError { Message = ex.Message });
        }
    }

    /// <summary>Reset the board/history/status for this session; scoreboard is left unchanged.</summary>
    [HttpPost("{id}/reset")]
    public ActionResult<GameStateResponse> ResetGame(string id)
    {
        try
        {
            var game = _gameService.ResetGame(id);
            return Ok(GameStateResponse.FromGameState(game, _scoreboard.GetScoreboard()));
        }
        catch (GameNotFoundException ex)
        {
            return NotFound(new ApiError { Message = ex.Message });
        }
    }
}
