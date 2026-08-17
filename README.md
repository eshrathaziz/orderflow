# OrderFlow

OrderFlow is an **ASP.NET MVC 5 order and inventory management application**. The primary implementation is written in **C#**, targets **.NET Framework 4.8**, uses **Entity Framework 6** for data access, and persists business data in **Microsoft SQL Server**. Its browser experience is built with Razor views, Bootstrap, jQuery, and targeted AJAX endpoints.

> The repository’s main application is `OrderFlow.MVC/`. It is a genuine MVC 5 project with controllers, models, ViewModels, services, Entity Framework context, Razor views, SQL Server schema, migrations configuration, and a Visual Studio solution. The previous TypeScript/Vite interface has been retained only as an archived prototype under `archive/`; it is not part of the primary application.

## Primary Project Structure

```text
OrderFlow/
├── OrderFlow.sln
├── OrderFlow.MVC/
│   ├── App_Start/                 # Routing, filters, bundles
│   ├── Controllers/               # MVC controllers
│   ├── Data/                      # OrderFlowDbContext and seed initializer
│   ├── Filters/                   # Role authorization filter
│   ├── Migrations/                # Entity Framework 6 migration configuration
│   ├── Models/                    # SQL Server entity model and enums
│   ├── Security/                  # Forms-authentication principal and password hashing
│   ├── Services/                  # Order, inventory, audit, dashboard business logic
│   ├── ViewModels/                # MVC input and dashboard projections
│   ├── Views/                     # Razor views for all modules
│   ├── Content/ and Scripts/      # Bootstrap-oriented styles and jQuery behaviour
│   ├── Web.config                 # SQL Server connection and MVC configuration
│   └── OrderFlow.MVC.csproj
├── OrderFlow.Verification/          # C# reservation-reconciliation verification harness
├── database/OrderFlow.Database.sql # SQL Server schema, constraints, indexes, and seed data
├── docs/                           # ER diagram, architecture diagram, interview guide, test plan
└── archive/                        # Non-primary historical UI prototype archive
```

## Implemented Business Modules

| Module | C# implementation |
| --- | --- |
| Authentication and authorization | Forms Authentication, PBKDF2 password hashing, custom principal, and controller/action role gates for Admin, Sales Executive, Inventory Manager, and Customer. |
| Customers | Searchable, paginated customer directory with create, detail, edit, and controlled deactivation actions. |
| Products | Category-backed product create/edit workflows, SKU uniqueness, pricing, product status, reorder levels, and linked inventory initialization. |
| Inventory | Stock availability calculation, auditable adjustments, reservations, shipment stock-out, history, and low-stock reporting. |
| Orders | Customer selection, multiple products, server-side pricing and tax calculation, stock validation, persisted created-order editing, reserved inventory reconciliation, and forward-only fulfilment workflow. |
| Customer requests | Create and update requests with type, priority, status, assignment, resolution, and request-history entries. |
| Dashboard | LINQ-based totals for customers, products, orders, pending/completed work, low stock, monthly revenue, recent orders, and revenue trend. |

## Order Workflow

```text
Created → Confirmed → Processing → Shipped → Delivered → Completed
```

`OrderService` validates every transition in C#. Order creation persists the order and items, computes totals, and reserves available inventory. The MVC edit workflow permits updates while an order remains in **Created** status; it recalculates monetary totals, reconciles reservations, records inventory movements, and writes an audit log. The **Shipped** transition commits the associated stock-out movement.

## SQL Server and Entity Framework 6

The database model is represented in `OrderFlow.MVC/Models/Entities.cs` and exposed through `OrderFlowDbContext`. The context has `DbSet` properties and configured relationships for the following persisted SQL Server tables:

| Table | Purpose |
| --- | --- |
| `Users`, `Roles` | Application identity and role assignment. |
| `Customers`, `Categories`, `Products` | Customer and product master data. |
| `Inventory`, `InventoryTransactions` | Current balances and movement history. |
| `Orders`, `OrderItems` | Sales orders, monetary totals, and product lines. |
| `CustomerRequests`, `RequestHistory` | Customer service work and communication history. |
| `AuditLogs` | User-attributed application audit trail. |

`database/OrderFlow.Database.sql` is the DBA-friendly SQL Server deployment script. It creates primary keys, foreign keys, check constraints, unique business identifiers, query indexes, and representative master data. `OrderFlow.MVC/Migrations/Configuration.cs` provides the EF6 migrations entry point for teams using Package Manager Console migration commands.

## Run Locally on Windows

### Prerequisites

Install **Visual Studio 2022** with the **ASP.NET and web development** workload, the **.NET Framework 4.8 targeting pack**, SQL Server Express/LocalDB or SQL Server Developer Edition, and NuGet package restore.

### Database setup

Choose one supported database initialization route:

1. **Code First development setup.** Keep the `OrderFlowConnection` LocalDB connection in `OrderFlow.MVC/Web.config`. The EF6 initializer seeds roles, users, categories, products, and inventory for local development.
2. **SQL Server script setup.** Execute `database/OrderFlow.Database.sql` in SQL Server Management Studio, then replace the `OrderFlowConnection` value in `Web.config` with your SQL Server connection string. Do not run the Code First seed initializer against a database already provisioned by the script.

### Build and run

1. Open `OrderFlow.sln` in Visual Studio.
2. Restore NuGet packages.
3. Set **OrderFlow.MVC** as the startup project.
4. Build the solution and start IIS Express with `F5`.
5. Sign in with the local development accounts seeded by `Data/SeedData.cs`.

| Account | Role | Development password |
| --- | --- | --- |
| `admin@orderflow.local` | Admin | `ChangeMe!123` |
| `sales@orderflow.local` | Sales Executive | `ChangeMe!123` |
| `inventory@orderflow.local` | Inventory Manager | `ChangeMe!123` |
| `ava@northstar.example` | Customer | `ChangeMe!123` |

Change the demonstration credentials, use an HTTPS binding, and provide a production SQL Server connection string before deploying outside a local development environment.

## Development Build Verification

The MVC project is built with the solution file:

```bash
mono tools/nuget.exe restore OrderFlow.sln -PackagesDirectory packages -NonInteractive
xbuild OrderFlow.sln /p:Configuration=Debug
mono OrderFlow.Verification/bin/Debug/OrderFlow.Verification.exe
```

The project compiles to `OrderFlow.MVC/bin/OrderFlow.MVC.dll`. `OrderFlow.Verification` validates the order-edit reservation reconciliation rules for increasing, reducing, and rejecting invalid stock allocations. Windows Visual Studio/IIS Express remains the supported hosting environment for the classic .NET Framework MVC runtime.

## Managed Development Preview

The repository includes a lightweight, dependency-free JavaScript development-preview shell so the managed project workspace can remain available while the primary classic MVC application is run through Windows IIS or IIS Express. It does not replace, build, or execute the ASP.NET MVC application. The primary implementation remains `OrderFlow.MVC/`; use Visual Studio and IIS Express for MVC route, Razor, authentication, and SQL Server runtime testing.

## AJAX Endpoints

| Endpoint | Behaviour |
| --- | --- |
| `GET /Orders/ProductSearch?query=` | Finds active products by SKU or name. |
| `GET /Orders/CheckAvailability?productId=&quantity=` | Returns current available quantity and reorder level. |
| `POST /Orders/Calculate` | Calculates server-side subtotal, tax, and total for order lines. |
| `POST /Orders/UpdateStatus` | Applies only a permitted next workflow status. |
| `GET /Inventory/Availability?productId=` | Returns product availability. |
| `POST /Inventory/Adjust` | Persists an inventory adjustment and movement record. |
| `POST /CustomerRequests/Update` | Persists request status, assignment, resolution, and communication history. |

## Documentation

The repository contains an [ER diagram](docs/ER-Diagram.mmd), an [architecture diagram](docs/Architecture-Diagram.mmd), an [interview guide](docs/INTERVIEW_GUIDE.md), and a [test plan](docs/TEST_PLAN.md).

## Prototype Archive

`archive/orderflow-operations-ledger-prototype.zip` preserves the former TypeScript/Vite interface prototype for historical reference only. It is not loaded, built, or used by the ASP.NET MVC 5 solution. This separation keeps the repository’s primary source implementation accurately represented by C#, Razor, Entity Framework 6, and SQL Server assets.
