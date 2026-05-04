using FiapCloudGames.Domain.Exceptions;

namespace FiapCloudGames.Domain.Entities;

public class Promotion
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public int DiscountPercentage { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    private Promotion() { }

    public Promotion(string title, string description, int discountPercentage, DateTime startDate, DateTime endDate)
    {
        Validate(discountPercentage, startDate, endDate);
        Title = title;
        Description = description;
        DiscountPercentage = discountPercentage;
        StartDate = startDate;
        EndDate = endDate;
    }

    public void Update(string title, string description, int discountPercentage, DateTime startDate, DateTime endDate, bool isActive)
    {
        Validate(discountPercentage, startDate, endDate);
        Title = title;
        Description = description;
        DiscountPercentage = discountPercentage;
        StartDate = startDate;
        EndDate = endDate;
        IsActive = isActive;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    private static void Validate(int discount, DateTime start, DateTime end)
    {
        if (discount < 1 || discount > 100)
            throw new DomainException("Discount percentage must be between 1 and 100.");
        if (end <= start)
            throw new DomainException("End date must be greater than start date.");
    }
}
