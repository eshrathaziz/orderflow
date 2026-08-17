using System.ComponentModel.DataAnnotations;
namespace OrderFlow.MVC.ViewModels { public class LoginViewModel { [Required, EmailAddress, Display(Name = "Work email")] public string Email { get; set; } [Required, DataType(DataType.Password)] public string Password { get; set; } [Display(Name = "Keep me signed in")] public bool RememberMe { get; set; } public string ReturnUrl { get; set; } } }
