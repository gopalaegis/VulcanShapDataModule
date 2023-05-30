using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Valcan.Models
{
    public class CreateUserViewModel
    {
        public int ID { get; set; }
        [Required(ErrorMessage = "This Field is Required")]

        public string FirstName { get; set; }
        [Required(ErrorMessage = "This Field is Required")]
        [Display(Name = "Contact Number")]
        [DataType(DataType.PhoneNumber)]
        //[RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$", ErrorMessage = "Please enter a valid contact number")]
        public string LastName { get; set; }
        [Remote("UserAlreadyExistsAsync", "UserMasters", AdditionalFields = "EmailID,ID", ErrorMessage = "User with this Email already exists")]
        [Required(ErrorMessage = "This Field is Required")]
        [RegularExpression(@"[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,4}", ErrorMessage = "Please enter correct email")]
        public string EmailID { get; set; }
        [Required(ErrorMessage = "This Field is Required")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 8)]
        [RegularExpression("^((?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])|(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[^a-zA-Z0-9])|(?=.*?[A-Z])(?=.*?[0-9])(?=.*?[^a-zA-Z0-9])|(?=.*?[a-z])(?=.*?[0-9])(?=.*?[^a-zA-Z0-9])).{8,}$", ErrorMessage = "Passwords must be at least 8 characters and contain at 3 of 4 of the following: upper case (A-Z), lower case (a-z), number (0-9) and special character (e.g. !@#$%^&*)")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        public string countryflag { get; set; }
        public List<KMChoiceViewModel> KMChoicesVM { get; set; }
        public List<Int64> SelectedChoices { get; set; }

        public List<ChoiceViewModel> ChoicesVM { get; set; }
        public List<Int64> SelectedRoleChoices { get; set; }
    }
    public class KMChoiceViewModel
    {
        public Int64 SNo { get; set; }
        public string Text { get; set; }
        public string name { get; set; }
    }
    public class ChoiceViewModel
    {
        public Int64 SNo { get; set; }
        public string Text { get; set; }
    }
}