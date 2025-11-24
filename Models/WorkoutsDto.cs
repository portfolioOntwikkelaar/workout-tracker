using System.ComponentModel.DataAnnotations;

namespace WorkoutApi.Models;

public class CreateWorkoutDto
{
    [Required(ErrorMessage = "Exercise naam is verplicht")]
    [MinLength(2, ErrorMessage = "Naam moet minimaal 2 karakters zijn")]
    public string Name { get; set; } = string.Empty;

    [Range(1, 1000, ErrorMessage = "Reps moet tussen 1 en 1000 zijn")]
    public int Reps { get; set; }

    [Range(0.1, 1000, ErrorMessage = "Weight moet tussen 0.1 en 1000 kg zijn")]
    public double Weight { get; set; }
}

public class UpdateWorkoutDto
{
    [Required]
    [MinLength(2)]
    public string Name { get; set; } = string.Empty;

    [Range(1, 1000)]
    public int Reps { get; set; }

    [Range(0.1, 1000)]
    public double Weight { get; set; }

    public bool IsPersonalRecord { get; set; }
}