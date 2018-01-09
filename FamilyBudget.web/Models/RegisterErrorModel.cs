using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FamilyBudget.data;

namespace FamilyBudget.web.Models
{
    public class RegisterErrorModel
    {
        public User user { get; set; }

        public string ErrorMessage { get; set; }
    }

}