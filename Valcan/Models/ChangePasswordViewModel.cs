using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DocumentManagement.Models
{
    public class ChangePasswordViewModel
    {
        [Key]
        public int MyProperty { get; set; }
        [Display(Name = "Password")]
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "This Field is Required")]
        public string password { get; set; }
        [Display(Name ="New Password")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 8)]
        
        [RegularExpression("^((?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])|(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[^a-zA-Z0-9])|(?=.*?[A-Z])(?=.*?[0-9])(?=.*?[^a-zA-Z0-9])|(?=.*?[a-z])(?=.*?[0-9])(?=.*?[^a-zA-Z0-9])).{8,}$", ErrorMessage = "Passwords must be at least 8 characters and contain at 3 of 4 of the following: upper case (A-Z), lower case (a-z), number (0-9) and special character (e.g. !@#$%^&*)")]

        [DataType(DataType.Password)]
        [Required(ErrorMessage ="This Field is Required")]
        public string NewPassword { get; set; }
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "This Field is Required")]
        [Display(Name = "Confirm Password")]
        [Compare("NewPassword",ErrorMessage = "Password doesn't match")]
        public string ConfirmPassword { get; set; }
    }
}