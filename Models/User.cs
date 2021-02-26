using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BankAccounts.Models
{
    public class User
    {
        [Key]
        public int UserID {get; set;}

        [Required]
        [MinLength(2, ErrorMessage = "First Name must be atleast Two characters in length.")]
        [Display(Name ="First Name")]
        public string FirstName {get; set;}

        [Required]
        [MinLength(2, ErrorMessage = "Last Name must be atleast Two characters in length.")]
        [Display(Name ="Last Name")]
        public string LastName {get; set;}

        [Required]
        [EmailAddress]
        public string Email {get; set;}

        [Required]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
        [DataType(DataType.Password)]
        public string Password {get; set;}

        [MinLength(8, ErrorMessage = "Password Confirmation must match the password.")]
        [Compare("Password", ErrorMessage = "Password must match.")]
        [DataType(DataType.Password)]
        [Display(Name ="Confirm Password")]
        [NotMapped]
        public string ConfirmPassword {get; set;}

        public string FullName()
        {
            return $"{FirstName} {LastName}";
        }
        public DateTime CreatedAt {get; set;} = DateTime.Now;
        public DateTime UpdatedAt {get; set;} = DateTime.Now;
        public List<Transaction> Transactions {get; set;}
    }
}