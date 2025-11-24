using Microsoft.EntityFrameworkCore;
using WorkoutApi.Data;
using WorkoutApi.Models;

namespace WorkoutApi.Services;

public class WorkoutService
{
    private readonly AppDbContext _db;

    public WorkoutService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Workout> AddWorkoutAsync(Workout workout)
    {
        workout.Date = DateTime.Now;

        // Check of dit een PR is
        workout.IsPersonalRecord = await IsPersonalRecordAsync(workout);

        _db.Workouts.Add(workout);
        await _db.SaveChangesAsync();

        return workout;
    }

    public async Task<bool> IsPersonalRecordAsync(Workout workout)
    {
        // Haal alle vorige workouts op voor deze exercise
        var previousBest = await _db.Workouts
            .Where(w => w.Name.ToLower() == workout.Name.ToLower())
            .OrderByDescending(w => w.Weight)
            .FirstOrDefaultAsync();

        // Als er geen vorige workout is, is dit automatisch een PR
        if (previousBest == null) return true;

        // Check of nieuwe weight hoger is
        return workout.Weight > previousBest.Weight;
    }

    public async Task<WorkoutStats> GetStatsForExerciseAsync(string exerciseName)
    {
        var workouts = await _db.Workouts
            .Where(w => w.Name.ToLower() == exerciseName.ToLower())
            .ToListAsync();

        if (!workouts.Any())
            return new WorkoutStats { ExerciseName = exerciseName };

        var totalVolume = workouts.Sum(w => w.Weight * w.Reps);
        var averageWeight = workouts.Average(w => w.Weight);
        var maxWeight = workouts.Max(w => w.Weight);
        var totalSets = workouts.Count;

        return new WorkoutStats
        {
            ExerciseName = exerciseName,
            TotalVolume = totalVolume,
            AverageWeight = averageWeight,
            MaxWeight = maxWeight,
            TotalSets = totalSets,
            FirstWorkout = workouts.Min(w => w.Date),
            LastWorkout = workouts.Max(w => w.Date)
        };
    }

    public async Task<List<Workout>> GetWorkoutsLastWeekAsync()
    {
        var weekAgo = DateTime.Now.AddDays(-7);
        return await _db.Workouts
            .Where(w => w.Date >= weekAgo)
            .OrderByDescending(w => w.Date)
            .ToListAsync();
    }

    public async Task<List<string>> GetUniqueExerciseNamesAsync()
    {
        return await _db.Workouts
            .Select(w => w.Name)
            .Distinct()
            .OrderBy(n => n)
            .ToListAsync();
    }
}

public class WorkoutStats
{
    public string ExerciseName { get; set; } = string.Empty;
    public double TotalVolume { get; set; }
    public double AverageWeight { get; set; }
    public double MaxWeight { get; set; }
    public int TotalSets { get; set; }
    public DateTime? FirstWorkout { get; set; }
    public DateTime? LastWorkout { get; set; }
}