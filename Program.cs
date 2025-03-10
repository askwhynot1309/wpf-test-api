using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var validUser = new
{
    Email = "user@gmail.com",
    Password = "Tom123123123@"
};

var games = new List<Game>
{
    new Game(1, "Math Game", "Solve math problems!", "https://example.com/mathgame", 500, "https://localhost:7000/api/downloadMathGame", ""),
    new Game(2, "English Game", "Learn English words!", "https://example.com/englishgame", 300, "https://localhost:7000/api/downloadEnglishGame", ""),
    new Game(3, "Run Game", "Run to win!", "https://example.com/rungame", 200, "https://localhost:7000/api/downloadRunGame", ""),
    new Game(4, "Balloon Pop Game", "Pop balloons for fun!", "https://example.com/balloongame", 450, "https://localhost:7000/api/downloadBalloonGame", "")
};

// Sample data for Category table
var categories = new List<Category>
{
    new Category(1, "Educational", "Games that help learning"),
    new Category(2, "Action", "Fast-paced gameplay"),
};

// Sample data for GameCategory table (Many-to-Many relation)
var gameCategories = new List<GameCategory>
{
    new GameCategory(1, 1, 1), // Math Game -> Educational
    new GameCategory(2, 2, 1), // English Game -> Educational
    new GameCategory(3, 3, 2), // Run Game -> Action
    new GameCategory(4, 4, 2)  // Balloon Pop Game -> Puzzle
};

// Sample data for GameVersion table
var gameVersions = new List<GameVersion>
{
    new GameVersion(1, "1.0", 1, "Initial release", DateTime.Now.AddMonths(-3)),
    new GameVersion(2, "1.1", 2, "Bug fixes and improvements", DateTime.Now.AddMonths(-2)),
    new GameVersion(3, "1.2", 3, "New levels added", DateTime.Now.AddMonths(-1)),
    new GameVersion(4, "2.0", 4, "Major update with new features", DateTime.Now)
};

app.MapPost("/api/login", ([FromBody] LoginRequest request) =>
{
    // Validation
    if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
    {
        return Results.BadRequest("Email and password are required");
    }

    // Credential check
    if (request.Email != validUser.Email || request.Password != validUser.Password)
    {
        return Results.Unauthorized();
    }

    // Successful login response
    var response = new LoginResponse(
        UserId: Guid.NewGuid(),
        Email: validUser.Email,
        FullName: "Test User",
        Token: "sample-jwt-token"
    );

    return Results.Ok(response);
})
.WithName("Login")
.Produces<LoginResponse>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status400BadRequest)
.Produces(StatusCodes.Status401Unauthorized);

app.MapGet("/api/downloadBalloonGame", (HttpContext context) =>
{
    var filePath = "C:\\Users\\PC\\Desktop\\export-to-exe.zip";
    if (!System.IO.File.Exists(filePath))
    {
        return Results.NotFound("File not found.");
    }

    var fileName = Path.GetFileName(filePath);
    var contentType = "application/octet-stream";

    return Results.File(System.IO.File.OpenRead(filePath), contentType, fileName);
})
.WithName("DownloadFile")
.Produces(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound);

// API to get all game info
app.MapGet("/api/get-game-info", () =>
{
    var gameInfoList = games.Select(game => new
    {
        game.Id,
        game.Title,
        game.Description,
        game.VideoUrl,
        game.PlayCount,
        game.DownloadUrl,
        game.ImageUrl,
        Categories = gameCategories
            .Where(gc => gc.GameId == game.Id)
            .Select(gc => categories.FirstOrDefault(c => c.Id == gc.CategoryId))
            .Where(c => c != null)
            .Select(c => new { c!.Id, c.Name, c.Description }),
        Versions = gameVersions
            .Where(v => v.GameId == game.Id)
            .Select(v => new { v.Version, v.Description, v.VersionDate })
    });

    return Results.Ok(gameInfoList);
})
.WithName("GetGameInfo")
.Produces(StatusCodes.Status200OK);

app.Run();

public record LoginRequest(string Email, string Password);
public record LoginResponse(Guid UserId, string Email, string FullName, string Token);
public record Game(int Id, string Title, string Description, string VideoUrl, int PlayCount, string DownloadUrl, string ImageUrl);
public record Category(int Id, string Name, string Description);
public record GameCategory(int Id, int GameId, int CategoryId);
public record GameVersion(int Id, string Version, int GameId, string Description, DateTime VersionDate);