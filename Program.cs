using Microsoft.EntityFrameworkCore;
using WorkoutApi.Data;
using WorkoutApi.Models;
using WorkoutApi.Services;
using WorkoutApi.Validators;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=workouts.db"));

// Add WorkoutService
builder.Services.AddScoped<WorkoutService>();

// Add FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<CreateWorkoutValidator>();

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

// POST nieuwe workout (met validatie en automatische PR detectie)
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

// PUT update workout (met validatie)
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