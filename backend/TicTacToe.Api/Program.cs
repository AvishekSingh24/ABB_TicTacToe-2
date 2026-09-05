using TicTacToe.Api.Services;

var builder = WebApplication.CreateBuilder(args);

const string AngularDevClient = "AngularDevClient";

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy(AngularDevClient, policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Singletons: game sessions and the scoreboard both need to live for the process
// lifetime and be shared across every request.
builder.Services.AddSingleton<IScoreboardService, ScoreboardService>();
builder.Services.AddSingleton<IComputerPlayerService, ComputerPlayerService>();
builder.Services.AddSingleton<IGameService, GameService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors(AngularDevClient);
app.UseAuthorization();
app.MapControllers();

app.Run();

// Exposed for WebApplicationFactory-based integration tests.
public partial class Program { }
