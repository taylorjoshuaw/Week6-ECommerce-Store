using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace StricklandPropane.Models
{
    public class RegisterViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "E-mail Address")]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Password")]
        [DataType(DataType.Password)]
        [RegularExpression(@"(.*\w+.*){6,100}", ErrorMessage = "Passwords must consist of at least 6 non-whitespace characters")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Passwords must be between 6-100 characters in length.")]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "The password and the password confirmation must match.")]
        public string ConfirmPassword { get; set; }
    }
}
