using System.ComponentModel.DataAnnotations;

namespace Authorization.Api.Models
{
    public class LoginModel
    {
        [Required]
        [Display(Name = "Email address")]
        public string Email { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }
    }
}