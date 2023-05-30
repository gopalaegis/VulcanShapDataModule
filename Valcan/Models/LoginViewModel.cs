using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Valcan.Models
{
    public class LoginViewModel
    {
        [Key]
        public int ID { get; set; }
        [DataType(DataType.EmailAddress, ErrorMessage = "E-mail is not valid")]
        [RegularExpression(@"[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,4}", ErrorMessage = "Please enter correct email")]

        [Required(ErrorMessage = "Please Enter Your Email Address")]
        public string EmailID { get; set; }
        [Required(ErrorMessage = "Please Enter Password")]
        public string Password { get; set; }
    }
}