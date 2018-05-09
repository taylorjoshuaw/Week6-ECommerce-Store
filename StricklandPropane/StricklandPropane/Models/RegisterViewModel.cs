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
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Passwords must be between 6-100 characters in length.")]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "The password and the password confirmation must match.")]
        public string ConfirmPassword { get; set; }

        [Required]
        [Display(Name = "First Name")]
        [RegularExpression(@"(.*\w+.*){1,50}", ErrorMessage = "Names should be at least 1 and at most 50 characters in length.")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "Names should be at least 1 and at most 50 characters in length.")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        [RegularExpression(@"(.*\w+.*){1,50}", ErrorMessage = "Names should be at least 1 and at most 50 characters in length.")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "Names should be at least 1 and at most 50 characters in length.")]
        public string LastName { get; set; }

        [Required]
        [EnumDataType(typeof(State))]
        [Display(Name = "Home State")]
        public State HomeState { get; set; }

        [Required]
        [EnumDataType(typeof(GrillingPreference))]
        [Display(Name = "Grilling Preference")]
        public GrillingPreference GrillingPreference { get; set; }
    }
}
