using System.ComponentModel.DataAnnotations;

namespace BankAccounts.Models
{
    public class LoginUser
    {
        [Required]
        [EmailAddress]
        [Display(Name ="Email")]
        public string LoginEmail {get; set;}

        [Required]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
        [DataType(DataType.Password)]
        [Display(Name ="Password")]
        public string LoginPassword {get; set;}
    }
}