using System;

namespace OrderFlow.MVC.Services
{
    /// <summary>Calculates the post-edit reservation balance while preserving reservations held by other orders.</summary>
    public static class ReservationReconciler
    {
        public static int Reconcile(int quantityOnHand, int currentlyReserved, int previousOrderQuantity, int requestedOrderQuantity)
        {
            if (quantityOnHand < 0 || currentlyReserved < 0 || previousOrderQuantity < 0 || requestedOrderQuantity < 0)
                throw new ArgumentOutOfRangeException("Reservation quantities cannot be negative.");
            if (previousOrderQuantity > currentlyReserved)
                throw new InvalidOperationException("The order reservation is inconsistent with the inventory balance.");

            var otherOrderReservations = currentlyReserved - previousOrderQuantity;
            var reconciledReservation = otherOrderReservations + requestedOrderQuantity;
            if (reconciledReservation > quantityOnHand)
                throw new InvalidOperationException("The requested quantity exceeds the available inventory after existing allocations.");
            return reconciledReservation;
        }
    }
}
