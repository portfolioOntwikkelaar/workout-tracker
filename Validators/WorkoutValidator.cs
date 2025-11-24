using FluentValidation;
using WorkoutApi.Models;

namespace WorkoutApi.Validators;

public class CreateWorkoutValidator : AbstractValidator<CreateWorkoutDto>
{
    public CreateWorkoutValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Exercise naam is verplicht")
            .MinimumLength(2).WithMessage("Naam moet minimaal 2 karakters zijn")
            .MaximumLength(100).WithMessage("Naam mag maximaal 100 karakters zijn");

        RuleFor(x => x.Reps)
            .GreaterThan(0).WithMessage("Reps moet minimaal 1 zijn")
            .LessThanOrEqualTo(1000).WithMessage("Reps mag maximaal 1000 zijn");

        RuleFor(x => x.Weight)
            .GreaterThan(0).WithMessage("Weight moet groter dan 0 zijn")
            .LessThanOrEqualTo(1000).WithMessage("Weight mag maximaal 1000 kg zijn");
    }
}

public class UpdateWorkoutValidator : AbstractValidator<UpdateWorkoutDto>
{
    public UpdateWorkoutValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Exercise naam is verplicht")
            .MinimumLength(2).WithMessage("Naam moet minimaal 2 karakters zijn")
            .MaximumLength(100).WithMessage("Naam mag maximaal 100 karakters zijn");

        RuleFor(x => x.Reps)
            .GreaterThan(0).WithMessage("Reps moet minimaal 1 zijn")
            .LessThanOrEqualTo(1000).WithMessage("Reps mag maximaal 1000 zijn");

        RuleFor(x => x.Weight)
            .GreaterThan(0).WithMessage("Weight moet groter dan 0 zijn")
            .LessThanOrEqualTo(1000).WithMessage("Weight mag maximaal 1000 kg zijn");
    }
}