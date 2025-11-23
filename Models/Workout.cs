namespace WorkoutApi.Models;

public class Workout
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Reps { get; set; }
    public double Weight { get; set; }
    public DateTime Date { get; set; }
    
    // Voor PR tracking later
    public bool IsPersonalRecord { get; set; }
}
