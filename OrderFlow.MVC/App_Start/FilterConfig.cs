using System.Web.Mvc;
namespace OrderFlow.MVC { public class FilterConfig { public static void RegisterGlobalFilters(GlobalFilterCollection filters) { filters.Add(new HandleErrorAttribute()); } } }
