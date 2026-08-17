# OrderFlow Interview Guide

## One-Minute System Overview

OrderFlow is an enterprise-style order and inventory management system designed around accountable fulfilment. Administrators, sales executives, inventory managers, and customers each receive only the functions appropriate to their role. The key business rule is that an order is not just a commercial record: it reserves inventory at creation, protects availability while it progresses, and converts that reservation into a stock-out when the order ships.

> The system connects order flow and stock flow so that a confirmed commercial action has a traceable inventory consequence.

| Topic | Explanation to give in an interview |
| --- | --- |
| Architecture | The solution uses MVC 5 to separate presentation, controller orchestration, and domain concerns. EF6 owns persistence, LINQ powers query composition, and service classes keep order and inventory rules out of views. |
| Security | Forms Authentication creates an encrypted ticket. The ticket payload is rehydrated to a custom principal, and `AuthorizeRoleAttribute` limits controllers and actions to their allowed roles. Passwords are stored as PBKDF2 SHA-256 hashes with per-user salts. |
| Data design | The schema separates products from the one-to-one inventory balance and records every balance change in `InventoryTransactions`. Order item prices are copied at order creation to protect historical financial accuracy when a product price later changes. |
| Order workflow | The permitted forward-only workflow is Created → Confirmed → Processing → Shipped → Delivered → Completed. `OrderService` refuses illegal transitions. |
| Stock integrity | `AvailableQuantity` is on-hand stock less reservations. Order creation validates available stock and reserves it. The Shipping transition converts reservations into `StockOut` transactions. Inventory cannot become negative. |
| Auditability | Order creation and status changes create audit records; inventory operations record before/after quantities, actor, type, note, and optional order reference. Customer requests also retain a communication history. |
| UX | Search, filtering, sorting, pagination, responsive Bootstrap views, and jQuery AJAX endpoints support high-frequency operational tasks without obscuring the underlying MVC flows. |

## High-Value Technical Walkthrough

Start in `OrdersController.Create`. MVC model binding maps the posted `OrderCreateViewModel`, and declarative validation checks the request shape. The controller delegates to `OrderService.CreateAsync`, which validates the customer, deduplicates product lines, loads active products with inventory, checks availability, calculates line-level tax, saves the order, reserves inventory, and writes an audit event. The UI does not calculate the authoritative total; it can display an AJAX estimate, but the service recalculates the final persisted value.

The inventory boundary is intentionally explicit. `InventoryService` is the only place that adjusts balances, reserves stock, or commits shipment stock-outs. This keeps the logic testable and makes future transaction or concurrency enhancements localized. The `Inventory.RowVersion` column provides an EF6 optimistic concurrency anchor for a production refinement.

## Likely Follow-Up Questions

| Question | Strong response |
| --- | --- |
| Why have Inventory and InventoryTransactions? | `Inventory` is the current operational balance for fast reads. `InventoryTransactions` is the immutable movement history needed for auditability, reconciliation, and future reporting. |
| Why reserve stock at order creation rather than shipment? | Reservations prevent accepting a second order for the same physical units. The actual stock-out waits for shipment so the distinction between committed and physically dispatched stock remains clear. |
| How do you prevent invalid order status jumps? | `OrderService.IsValidTransition` contains the single workflow rule. The update action delegates to it, so neither the UI nor a direct AJAX request can bypass the workflow. |
| How would you scale the dashboard? | Start with indexed status/date columns, projection-only LINQ queries, and cached or pre-aggregated revenue summaries. The current design already avoids loading full object graphs for dashboard metrics. |
| What would you improve for production? | Add formal EF migrations, integration tests against SQL Server, distributed data protection, HTTPS-only cookies, database transactions around order/reservation, optimistic concurrency handling, structured logging, and a background low-stock notification job. |

## Demonstration Script

First, sign in as a sales executive and create an order. Show that the available quantity is checked before saving and that the total includes tax. Next, open Inventory and show the reservation. Advance the order through the workflow to Shipped and show the stock-out in inventory history. Finally, sign in as a customer to submit a change request and show how an employee sees the request, assigns it, updates its status, and leaves customer-visible communication.
