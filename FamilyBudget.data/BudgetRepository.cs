using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyBudget.data
{
 public class BudgetRepository
    {
        readonly string _connectionString;

        public BudgetRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public User UserWithHistory(int Id)
        {
            using (ContextDBDataContext context = new ContextDBDataContext(_connectionString))
            {
                var LoadOptions = new DataLoadOptions();
                LoadOptions.LoadWith<User>(U => U.Month_Years);
                LoadOptions.LoadWith<User>(U => U.Debits);
                LoadOptions.LoadWith<User>(U => U.Credits);
                context.LoadOptions = LoadOptions;

                return context.Users.First(U => U.Id == Id);
            }
        }
        // returns a list of months for user it correct date order
        public List<Month_Year> MonthYearForUser(int UserId)
        {
            using (ContextDBDataContext context = new ContextDBDataContext(_connectionString))
            {
                 List<Month_Year> MonthsForUser = context.Month_Years.Where(MY => MY.UserId == UserId).OrderBy(My => My.Year).ToList();
                List<int> DistinctYears = MonthsForUser.Select(MY => MY.Year).Distinct().ToList();
                List<Month_Year> OrderedList = new List<Month_Year>();

                foreach (int year in DistinctYears)
                {
                    List<Month_Year> MonthsForDistincrYear = MonthsForUser.Where(MY => MY.Year == year).OrderBy(MY => MY.Month).ToList();

                    foreach (Month_Year MY in MonthsForDistincrYear)
                    {
                        OrderedList.Add(MY);
                    }

                   
                }
                return OrderedList;
            }

        }

        // adds new month and returns false if month already exists
        public bool AddMonth (Month_Year NewMonth)
        {
            using (ContextDBDataContext context = new ContextDBDataContext(_connectionString))
            {
                if (DoesMonthExistForUser(context.Month_Years.Where(M => M.UserId == NewMonth.UserId).ToList(), NewMonth))
                {
                    return true;
                }
                
                context.Month_Years.InsertOnSubmit(NewMonth);
                context.SubmitChanges();

                return false;
            }

        }

        // returns true if month already exists
        private bool DoesMonthExistForUser(List<Month_Year> MonthsForUser,Month_Year NewMonth)
        {
            if(MonthsForUser.Any(M => M.Month == NewMonth.Month && M.Year == NewMonth.Year && M.UserId == NewMonth.UserId))
            {
                return true;
            }

            return false;

        }

        public bool DoesMonthIdExistForUser(int UserId, int MonthId)
        {
            using (ContextDBDataContext context = new ContextDBDataContext(_connectionString))
            {
                if (context.Month_Years.Any(M => M.UserId == UserId && M.Id == MonthId))
                  { 
                    return true;
                }
                return false;
            }
        }
        //get credits for month
        public List<Credit> GetCreditsByMonthId(int UserId, int MonthId)
        {
            using (ContextDBDataContext context = new ContextDBDataContext(_connectionString))
            {
                                 
             return context.Credits.Where(C => C.UserId == UserId && C.MonthId == MonthId).OrderBy(C => C.Date).ToList();
               
            }

        }

        public List<Debit> GetDebitsByMonthId(int UserId, int MonthId)
        {
            using (ContextDBDataContext context = new ContextDBDataContext(_connectionString))
            {
                var LoadOptions = new DataLoadOptions();
                LoadOptions.LoadWith<Debit>(D => D.Category);
                context.LoadOptions = LoadOptions;
                return context.Debits.Where(D => D.UserId == UserId && D.MonthId == MonthId).OrderBy(D => D.Date).ToList();
            }

        }

       public Month_Year GetMonthInfo(int MonthId)
        {
            using (ContextDBDataContext context = new ContextDBDataContext(_connectionString))
            {
                return context.Month_Years.First(M => M.Id == MonthId);
            }

        }

        public List<Category> GetAllCategories()
        {
            using (ContextDBDataContext context = new ContextDBDataContext(_connectionString))
            {
                return context.Categories.ToList();
            }

        }

        public void AddCredit(Credit credit)
        {
            using (ContextDBDataContext context = new ContextDBDataContext(_connectionString))
            {
                context.Credits.InsertOnSubmit(credit);
                context.SubmitChanges();
            }

        }

        public List<Debit> AddDebit(Debit debit)
        {
            using (ContextDBDataContext context = new ContextDBDataContext(_connectionString))
            {
                context.Debits.InsertOnSubmit(debit);
                context.SubmitChanges();

                return GetDebitsByMonthId(debit.UserId, debit.MonthId);
            }

        }

        public List<Debit> CategoryListWithAmounts(int MonthId)
        {
            using (ContextDBDataContext context = new ContextDBDataContext(_connectionString))
            {
                var LoadOptions = new DataLoadOptions();
                LoadOptions.LoadWith<Debit>(D => D.Category);
                context.LoadOptions = LoadOptions;
                return context.Debits.Where(D => D.MonthId == MonthId).ToList();
            }

        }
    }
}
