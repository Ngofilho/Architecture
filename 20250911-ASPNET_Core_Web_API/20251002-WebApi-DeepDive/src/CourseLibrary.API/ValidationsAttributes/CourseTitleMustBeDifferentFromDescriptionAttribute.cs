using CourseLibrary.API.Models;
using System.ComponentModel.DataAnnotations;

namespace CourseLibrary.API.ValidationsAttributes
{
    public class CourseTitleMustBeDifferentFromDescriptionAttribute : ValidationAttribute
    {
        public CourseTitleMustBeDifferentFromDescriptionAttribute()
        {
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (validationContext.ObjectInstance is not
                CourseForManipulationDto course)
            {
                throw new Exception($"Attribute " +
                    $"'{nameof(CourseTitleMustBeDifferentFromDescriptionAttribute)}' " +
                    $"must be applied on '{nameof(Models.CourseForCreationDto)}' or " +
                    $"'{nameof(Models.CourseForUpdateDto)}'");
            }

            if (course.Title == course.Description)
            {
                return new ValidationResult(
                    "The provided title should be different from the description.",
                    new[] { nameof(CourseForCreationDto) });
            }

            return ValidationResult.Success;
        }
    }
}
