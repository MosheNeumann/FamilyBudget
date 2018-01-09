using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FamilyBudget.data;

namespace FamilyBudget.web.Models
{
    public class BudgetViewModel
    {
        public BudgetViewModel()
        {
            LatestMonth = new Month_Year();
        }
        public User CurrentUser { get; set; }
        public List<Month_Year> MonthsBudgeted { get; set; }
        public List<Debit> DebitsForLatestMonth { get; set; }
        public List<Credit> CreditsForLastMonth { get; set; }
        public Month_Year LatestMonth { get; set; }
        public List<Category> AllCategories { get; set; }
    }
}