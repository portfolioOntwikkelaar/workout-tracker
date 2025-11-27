using Microsoft.EntityFrameworkCore;
using WorkoutApi.Data;
using WorkoutApi.Models;
using WorkoutApi.Services;
using WorkoutApi.Validators;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

// CORS
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
.AllowAnyMethod();
});
});

// EF Core in-memory
builder.Services.AddDbContext<AppDbContext>(options =>
options.UseInMemoryDatabase("WorkoutDb"));

// Services
builder.Services.AddScoped<WorkoutService>();

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<CreateWorkoutValidator>();

var app = builder.Build();
app.UseCors("AllowReact");

if (app.Environment.IsDevelopment())
{
app.UseSwagger();
app.UseSwaggerUI();
}

// ==== ENDPOINTS ====
// (zelfde endpoints als voorheen, gewoon normaal gebruiken)
app.MapGet("/api/workouts", async (AppDbContext db) =>
{
var workouts = await db.Workouts.OrderByDescending(w => w.Date).ToListAsync();
return Results.Ok(workouts);
});

// POST voorbeeld
app.MapPost("/api/workouts", async (CreateWorkoutDto dto, WorkoutService service, IValidator<CreateWorkoutDto> validator) =>
{
var validationResult = await validator.ValidateAsync(dto);
if (!validationResult.IsValid)
return Results.BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));

var created = await service.AddWorkoutAsync(dto);
return Results.Ok(created);
});

app.Run();