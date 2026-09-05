using Microsoft.AspNetCore.Mvc;
using TicTacToe.Api.Models;
using TicTacToe.Api.Services;

namespace TicTacToe.Api.Controllers;

[ApiController]
[Route("api/scoreboard")]
public class ScoreboardController : ControllerBase
{
    private readonly IScoreboardService _scoreboard;

    public ScoreboardController(IScoreboardService scoreboard)
    {
        _scoreboard = scoreboard;
    }

    /// <summary>Get the session-level scoreboard (X wins, O wins, draws).</summary>
    [HttpGet]
    public ActionResult<Scoreboard> GetScoreboard() => Ok(_scoreboard.GetScoreboard());

    /// <summary>Reset the scoreboard to zero. Does not affect any in-progress game.</summary>
    [HttpPost("reset")]
    public ActionResult<Scoreboard> ResetScoreboard()
    {
        _scoreboard.Reset();
        return Ok(_scoreboard.GetScoreboard());
    }
}
