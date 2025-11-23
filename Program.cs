using Microsoft.EntityFrameworkCore;
using WorkoutApi.Data;
using WorkoutApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=workouts.db"));

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
    var workouts = await db.Workouts.ToListAsync();
    return Results.Ok(workouts);
});

// GET workout by ID
app.MapGet("/api/workouts/{id}", async (int id, AppDbContext db) =>
{
    var workout = await db.Workouts.FindAsync(id);
    return workout is not null ? Results.Ok(workout) : Results.NotFound();
});

// POST nieuwe workout
app.MapPost("/api/workouts", async (Workout workout, AppDbContext db) =>
{
    workout.Date = DateTime.Now;
    db.Workouts.Add(workout);
    await db.SaveChangesAsync();
    return Results.Created($"/api/workouts/{workout.Id}", workout);
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

// GET Personal Records per exercise
app.MapGet("/api/workouts/prs", async (AppDbContext db) =>
{
    var prs = await db.Workouts
        .Where(w => w.IsPersonalRecord)
        .OrderBy(w => w.Name)
        .ToListAsync();
    return Results.Ok(prs);
});

app.Run();