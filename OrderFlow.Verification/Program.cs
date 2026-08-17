using System;
using OrderFlow.MVC.Services;

namespace OrderFlow.Verification
{
    internal static class Program
    {
        private static int Main()
        {
            try
            {
                Expect(ReservationReconciler.Reconcile(10, 5, 2, 6) == 9, "Increasing an order reservation should preserve other reservations.");
                Expect(ReservationReconciler.Reconcile(10, 5, 2, 1) == 4, "Reducing an order reservation should release only the order delta.");
                ExpectThrows(() => ReservationReconciler.Reconcile(10, 8, 3, 6), "An over-allocation must be rejected.");
                ExpectThrows(() => ReservationReconciler.Reconcile(10, 2, 3, 1), "An inconsistent existing reservation must be rejected.");
                Console.WriteLine("OrderFlow reservation reconciliation verification passed.");
                return 0;
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine(exception.Message);
                return 1;
            }
        }

        private static void Expect(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
        private static void ExpectThrows(Action action, string message) { try { action(); } catch (InvalidOperationException) { return; } throw new InvalidOperationException(message); }
    }
}
