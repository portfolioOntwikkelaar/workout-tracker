using Microsoft.EntityFrameworkCore;
using WorkoutApi.Data;
using WorkoutApi.Models;
using WorkoutApi.Services;
using WorkoutApi.Validators;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);
//derde poging
// Railway port configuratie
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// Add services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database configuratie
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
if (!string.IsNullOrEmpty(databaseUrl))
{
    // Railway PostgreSQL URL parser
    var uri = new Uri(databaseUrl);
    var connectionString =
    $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};Username={uri.UserInfo.Split(':')[0]};Password={uri.UserInfo.Split(':')[1]};SSL Mode=Require;Trust Server Certificate=true";

    builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString)
    );
}
else
{
    // Lokale SQLite
    builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=workouts.db")
    );
}

// Services
builder.Services.AddScoped<WorkoutService>();

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<CreateWorkoutValidator>();

// CORS voor localhost + Vercel wildcard
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReact", policy =>
    {
        policy
    .SetIsOriginAllowed(origin =>
    origin.StartsWith("http://localhost") ||
    origin.EndsWith(".vercel.app")
    )
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials();
    });
});

var app = builder.Build();

// Middleware
app.UseCors("AllowReact");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// === ENDPOINTS ===

// GET alle workouts
app.MapGet("/api/workouts", async (AppDbContext db) =>
{
    var workouts = await db.Workouts
    .OrderByDescending(w => w.Date)
    .ToListAsync();
    return Results.Ok(workouts);
});

// GET workout by ID
app.MapGet("/api/workouts/{id}", async (int id, AppDbContext db) =>
{
    var workout = await db.Workouts.FindAsync(id);
    return workout is not null ? Results.Ok(workout) : Results.NotFound();
});

// POST nieuwe workout
app.MapPost("/api/workouts", async (CreateWorkoutDto dto, WorkoutService service, IValidator<CreateWorkoutDto> validator) =>
{
    var validationResult = await validator.ValidateAsync(dto);
    if (!validationResult.IsValid)
    {
        var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
        return Results.BadRequest(ApiResponse<Workout>.ErrorResponse("Validatie gefaald", errors));
    }

    try
    {
        var created = await service.AddWorkoutAsync(dto);
        return Results.Ok(ApiResponse<Workout>.SuccessResponse(
        created,
        created.IsPersonalRecord ? "🎉 Nieuwe PR!" : "Workout toegevoegd"
        ));
    }
    catch (Exception ex)
    {
        return Results.BadRequest(ApiResponse<Workout>.ErrorResponse(
        "Fout bij toevoegen workout",
        new List<string> { ex.Message }
        ));
    }
});

// PUT update workout
app.MapPut("/api/workouts/{id}", async (int id, UpdateWorkoutDto dto, WorkoutService service, IValidator<UpdateWorkoutDto> validator) =>
{
    var validationResult = await validator.ValidateAsync(dto);
    if (!validationResult.IsValid)
    {
        var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
        return Results.BadRequest(ApiResponse<Workout>.ErrorResponse("Validatie gefaald", errors));
    }

    try
    {
        var updated = await service.UpdateWorkoutAsync(id, dto);
        if (updated is null)
            return Results.NotFound(ApiResponse<Workout>.ErrorResponse("Workout niet gevonden"));

        return Results.Ok(ApiResponse<Workout>.SuccessResponse(updated, "Workout updated"));
    }
    catch (Exception ex)
    {
        return Results.BadRequest(ApiResponse<Workout>.ErrorResponse(
        "Fout bij updaten workout",
        new List<string> { ex.Message }
        ));
    }
});

// DELETE workout
app.MapDelete("/api/workouts/{id}", async (int id, AppDbContext db) =>
{
    var workout = await db.Workouts.FindAsync(id);
    if (workout is null) return Results.NotFound();

    db.Workouts.Remove(workout);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

// GET PRs
app.MapGet("/api/workouts/prs", async (AppDbContext db) =>
{
    var prs = await db.Workouts
    .Where(w => w.IsPersonalRecord)
    .OrderByDescending(w => w.Date)
    .ToListAsync();
    return Results.Ok(prs);
});

// GET laatste 7 dagen
app.MapGet("/api/workouts/recent", async (WorkoutService service) =>
{
    var workouts = await service.GetWorkoutsLastWeekAsync();
    return Results.Ok(workouts);
});

// GET workouts per exercise
app.MapGet("/api/workouts/exercise/{name}", async (string name, AppDbContext db) =>
{
    var workouts = await db.Workouts
    .Where(w => w.Name.ToLower() == name.ToLower())
    .OrderByDescending(w => w.Date)
    .ToListAsync();
    return Results.Ok(workouts);
});

// GET stats
app.MapGet("/api/stats/{exerciseName}", async (string exerciseName, WorkoutService service) =>
{
    var stats = await service.GetStatsForExerciseAsync(exerciseName);
    return Results.Ok(stats);
});

// GET unieke exercise namen
app.MapGet("/api/exercises", async (WorkoutService service) =>
{
    var exercises = await service.GetUniqueExerciseNamesAsync();
    return Results.Ok(exercises);
});

// Auto-migratie
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.Run();