namespace Drivolution.Models.Constants;

// Estados do ciclo de vida de uma ManufacturingOrder / ClientOrder.

public static class OrderStatus
{
    public const string Pending = "pending";
    public const string InProgress = "in_progress";
    public const string Completed = "completed";
    public const string Cancelled = "cancelled";
}