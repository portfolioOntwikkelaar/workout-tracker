using Microsoft.EntityFrameworkCore;
using WorkoutApi.Data;
using WorkoutApi.Models;
using WorkoutApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=workouts.db"));

// Add WorkoutService
builder.Services.AddScoped<WorkoutService>();

var app = builder.Build();

// Configure middleware
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

// POST nieuwe workout (met automatische PR detectie)
app.MapPost("/api/workouts", async (Workout workout, WorkoutService service) =>
{
    var created = await service.AddWorkoutAsync(workout);
    return Results.Created($"/api/workouts/{created.Id}", created);
});

// PUT update workout
app.MapPut("/api/workouts/{id}", async (int id, Workout updatedWorkout, AppDbContext db) =>
{
    var workout = await db.Workouts.FindAsync(id);
    if (workout is null) return Results.NotFound();

    workout.Name = updatedWorkout.Name;
    workout.Reps = updatedWorkout.Reps;
    workout.Weight = updatedWorkout.Weight;
    workout.IsPersonalRecord = updatedWorkout.IsPersonalRecord;

    await db.SaveChangesAsync();
    return Results.Ok(workout);
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

// GET Personal Records
app.MapGet("/api/workouts/prs", async (AppDbContext db) =>
{
    var prs = await db.Workouts
        .Where(w => w.IsPersonalRecord)
        .OrderByDescending(w => w.Date)
        .ToListAsync();
    return Results.Ok(prs);
});

// GET workouts van laatste 7 dagen
app.MapGet("/api/workouts/recent", async (WorkoutService service) =>
{
    var workouts = await service.GetWorkoutsLastWeekAsync();
    return Results.Ok(workouts);
});

// GET workouts gefilterd op exercise naam
app.MapGet("/api/workouts/exercise/{name}", async (string name, AppDbContext db) =>
{
    var workouts = await db.Workouts
        .Where(w => w.Name.ToLower() == name.ToLower())
        .OrderByDescending(w => w.Date)
        .ToListAsync();
    return Results.Ok(workouts);
});

// GET statistieken voor een exercise
app.MapGet("/api/stats/{exerciseName}", async (string exerciseName, WorkoutService service) =>
{
    var stats = await service.GetStatsForExerciseAsync(exerciseName);
    return Results.Ok(stats);
});

// GET lijst van alle unieke exercise namen
app.MapGet("/api/exercises", async (WorkoutService service) =>
{
    var exercises = await service.GetUniqueExerciseNamesAsync();
    return Results.Ok(exercises);
});

app.Run();