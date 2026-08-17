using System.Data.Entity.Migrations;
using OrderFlow.MVC.Data;

namespace OrderFlow.MVC.Migrations
{
    /// <summary>
    /// Entity Framework 6 migration entry point for SQL Server deployments.
    /// Use Enable-Migrations / Add-Migration / Update-Database from the Visual Studio Package Manager Console.
    /// </summary>
    internal sealed class Configuration : DbMigrationsConfiguration<OrderFlowDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            ContextKey = "OrderFlow.MVC.Data.OrderFlowDbContext";
        }
    }
}
