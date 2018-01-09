using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FamilyBudget.web.Models
{
    public class CategoryWithAmountForMonth
    {
        public string CategoryName { get; set; }

        public decimal Sum { get; set; }
    }
}