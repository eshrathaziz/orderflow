namespace OrderFlow.MVC.Models
{
    public enum UserRole { Admin = 1, SalesExecutive = 2, InventoryManager = 3, Customer = 4 }
    public enum CustomerStatus { Active = 1, Inactive = 2, Suspended = 3 }
    public enum ProductStatus { Active = 1, Inactive = 2, Discontinued = 3 }
    public enum InventoryTransactionType { StockIn = 1, StockOut = 2, AdjustmentIncrease = 3, AdjustmentDecrease = 4, Reservation = 5, ReservationRelease = 6 }
    public enum OrderStatus { Created = 1, Confirmed = 2, Processing = 3, Shipped = 4, Delivered = 5, Completed = 6, Cancelled = 7 }
    public enum RequestType { OrderChange = 1, Cancellation = 2, DeliveryUpdate = 3, ProductQuestion = 4, General = 5 }
    public enum RequestPriority { Low = 1, Medium = 2, High = 3, Urgent = 4 }
    public enum RequestStatus { Open = 1, InProgress = 2, AwaitingCustomer = 3, Resolved = 4, Closed = 5 }
    public enum AuditAction { Created = 1, Updated = 2, Deleted = 3, StatusChanged = 4, Login = 5, InventoryAdjusted = 6 }
}
