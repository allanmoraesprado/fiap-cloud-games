namespace FiapCloudGames.Application.Validators;

public static class PromotionValidator
{
    public static ValidationResult Validate(int discountPercentage, DateTime startDate, DateTime endDate)
    {
        var errors = new List<string>();
        if (discountPercentage < 1 || discountPercentage > 100)
            errors.Add("Discount percentage must be between 1 and 100.");
        if (endDate <= startDate)
            errors.Add("End date must be greater than start date.");
        return errors.Count == 0 ? ValidationResult.Success() : ValidationResult.Fail(errors.ToArray());
    }
}
