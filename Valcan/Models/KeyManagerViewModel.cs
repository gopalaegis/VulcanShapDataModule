using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Valcan.Models
{
    public class KeyManagerViewModel
    {
        public int ID { get; set; }
        [Remote("KeyManagerAlreadyExists", "KeyManagerMaster", AdditionalFields = "ID,KeyManager", ErrorMessage = "This Key Manager already exists")]
        //[Required(ErrorMessage = "This Field is Required")]
        public string KeyManager { get; set; }
        [Required(ErrorMessage = "This Field is Required")]
        public string KeyManager_Name { get; set; }
        public string msg { get; set; }
        [Display(Name = "Is Active")]
        public Nullable<bool> IsActive { get; set; }
    }
}