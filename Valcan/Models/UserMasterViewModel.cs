using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DocumentManagement.Models
{
    public class UserMasterViewModel
    {
        public int ID { get; set; }
        [Required(ErrorMessage = "This Field is Required")]

        public string FirstName { get; set; }
        [Required(ErrorMessage = "This Field is Required")]
        [Display(Name = "Contact Number")]
        //[DataType(DataType.PhoneNumber)]
        //[RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$", ErrorMessage = "Please enter a valid phone number")]
        public string LastName { get; set; }
        public string countryflag { get; set; }
        [Remote("UserIDAlreadyExistsAsync", "Home", AdditionalFields = "EmailID", ErrorMessage = "User with this Email already exists")]
        [Required(ErrorMessage = "This Field is Required")]
        [RegularExpression(@"[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,4}", ErrorMessage = "Please enter correct email")]
        public string EmailID { get; set; }
    }
}