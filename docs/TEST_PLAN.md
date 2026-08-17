# OrderFlow Test Plan

## Validation Status

The interactive React demonstration was validated with `pnpm check` (`tsc --noEmit`) after implementation and after visual refinement. The sandbox does not contain a Windows .NET Framework/MSBuild runtime, so the MVC 5 project has been delivered as source-ready and requires the prescribed Visual Studio / SQL Server validation on Windows before production use.

| Area | Scenario | Expected outcome | Status |
| --- | --- | --- | --- |
| Frontend demonstration | TypeScript static check | No TypeScript errors | Passed |
| Design review | Desktop dashboard visual review | Persistent rail, semantic status colors, data-led warehouse monitor applied | Passed |
| Authentication | Valid role account sign-in | Custom principal contains correct user ID, role, customer ID | Manual verification required on Windows |
| Authorization | Sales Executive attempts inventory adjustment | HTTP 403 response | Manual verification required on Windows |
| Order validation | Quantity exceeds available balance | Order service rejects request; no order/reservation is created | Manual verification required on Windows |
| Workflow | Attempt Created → Shipped | Service rejects invalid transition | Manual verification required on Windows |
| Shipping | Processing → Shipped | Reserved stock becomes a stock-out transaction and on-hand balance falls | Manual verification required on Windows |
| Customer request | Customer creates a request | Request number and first communication-history item are created | Manual verification required on Windows |
| Product CRUD | Create product with initial stock | Product and linked inventory balance are created | Manual verification required on Windows |
| SQL schema | Execute `database/OrderFlow.Database.sql` on clean SQL Server database | Tables, constraints, indexes and representative data are created | Manual verification required on Windows |

## Recommended Manual Test Order

Create the database using either EF6 initialization for the development seed or the SQL script for a DBA-controlled deployment; do not point both bootstrapping routes at the same non-empty database. Sign in with the development sales account, create an order for a product with sufficient stock, and verify the reservation. Advance it sequentially through the order workflow, then confirm that shipping writes a negative inventory movement. Use the inventory manager account to adjust stock, and inspect the transaction history. Finally, use the customer account to create a request and verify that an employee can update its assignment, priority, resolution, and visible communication history.
