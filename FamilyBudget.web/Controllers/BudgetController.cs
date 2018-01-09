using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FamilyBudget.data;
using System.Web.Security;
using FamilyBudget.web.Models;

namespace FamilyBudget.web.Controllers
{
    [Authorize]
    public class BudgetController : Controller
    {
        // GET: Budget
        public ActionResult Budget()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("login", "Home");
            }

            UserRepository UserRepo = new UserRepository(Properties.Settings.Default._connectionString);
            User CurrentUser = UserRepo.GetUserByEmail(User.Identity.Name);

            BudgetRepository BudgetRepo = new BudgetRepository(Properties.Settings.Default._connectionString);

            var BudgetModel = new BudgetViewModel();
            BudgetModel.CurrentUser = BudgetRepo.UserWithHistory(CurrentUser.Id);
            BudgetModel.MonthsBudgeted = BudgetRepo.MonthYearForUser(CurrentUser.Id);
            BudgetModel.AllCategories = BudgetRepo.GetAllCategories();
            if (BudgetModel.MonthsBudgeted.Count != 0)
            {
              //gets debits for last month budgeted
                BudgetModel.DebitsForLatestMonth = BudgetRepo.GetDebitsByMonthId(CurrentUser.Id, BudgetModel.MonthsBudgeted[BudgetModel.MonthsBudgeted.Count - 1].Id);
                BudgetModel.CreditsForLastMonth = BudgetRepo.GetCreditsByMonthId(CurrentUser.Id, BudgetModel.MonthsBudgeted[BudgetModel.MonthsBudgeted.Count - 1].Id);
                BudgetModel.LatestMonth = BudgetRepo.GetMonthInfo(BudgetModel.MonthsBudgeted[BudgetModel.MonthsBudgeted.Count - 1].Id);
               
            }

            return View(BudgetModel);
        }

        [HttpPost]
        public ActionResult AddMonth(Month_Year NewMonth)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("login", "Home");
            }
            var UserRepo = new UserRepository(Properties.Settings.Default._connectionString);
            var budgetRepo = new BudgetRepository(Properties.Settings.Default._connectionString);

            //gets user id 
            NewMonth.UserId = UserRepo.GetUserByEmail(User.Identity.Name).Id;

            bool MonthExists = budgetRepo.AddMonth(NewMonth);
            
            if (MonthExists == true)
            {
                return Json(0);
            }

            return Json(NewMonth.Id);                           
        }

        [HttpPost]

        // returns list of credits and sum of credits
        public ActionResult AddCredit(Credit credit)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("login", "Home");
            }

            var UserRepo = new UserRepository(Properties.Settings.Default._connectionString);
            var budgetRepo = new BudgetRepository(Properties.Settings.Default._connectionString);

            credit.UserId = UserRepo.GetUserByEmail(User.Identity.Name).Id;
            budgetRepo.AddCredit(credit);

            List<Credit> Credits = budgetRepo.GetCreditsByMonthId(credit.UserId, credit.MonthId);

            return Json(new { credits = Credits.Select(C => new
            {
                Id = C.Id,
                Amount = C.Amount.ToString("C"),
                Date = C.Date,
                Source = C.Source
            }),
                creditTotal = Credits.Sum(C => C.Amount).ToString("C")
            });


        }

        [HttpPost]
        public ActionResult AddDebit (Debit debit)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("login", "Home");
            }

            var UserRepo = new UserRepository(Properties.Settings.Default._connectionString);
            var budgetRepo = new BudgetRepository(Properties.Settings.Default._connectionString);

            debit.UserId = UserRepo.GetUserByEmail(User.Identity.Name).Id;
            List<Debit> Debits = budgetRepo.AddDebit(debit);
           //  = budgetRepo.GetDebitsByMonthId(debit.UserId, debit.MonthId);
            //return Json(debit);

            return Json(new
            {
                
                debits = Debits.Select(D => new
                {
                    id = D.Id,
                    amount = D.Amount.ToString("C"),
                    date = D.Date,
                    category = D.Category.CategoryName,
                    detail = D.Details
                }),
                
                debitTotal = Debits.Sum( D => D.Amount).ToString("C")

            });

        }
        [HttpGet]
        public ActionResult SwitchMonth(int NewMonthId)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("login", "Home");
            }

            var UserRepo = new UserRepository(Properties.Settings.Default._connectionString);
            var budgetRepo = new BudgetRepository(Properties.Settings.Default._connectionString);

            var userId = UserRepo.GetUserByEmail(User.Identity.Name).Id;

            if (!budgetRepo.DoesMonthIdExistForUser(userId, NewMonthId))
            {
                return Json(0);
            }

           
            List<Credit> Credits = budgetRepo.GetCreditsByMonthId(userId,NewMonthId);

            decimal TotalCredit = Credits.Sum(C => C.Amount);
            List<Debit> Debits = budgetRepo.GetDebitsByMonthId(userId, NewMonthId);

            
            return Json(new { credits = Credits.Select(C => new
            {
                Id = C.Id,
                Amount = C.Amount.ToString("C"),
                Date = C.Date,
                Source = C.Source

            }),

            creditTotal = Credits.Sum(C => C.Amount).ToString("C"),

            debits = Debits.Select(D => new
            {
                id = D.Id,
                amount = D.Amount,
                date = D.Date,
                category = D.Category.CategoryName,
                detail = D.Details
            }),
            debitTotal = Debits.Sum( D => D.Amount).ToString("C")
            }, JsonRequestBehavior.AllowGet);
          

            

        }
       
        [HttpGet]
        public ActionResult getMonthsBudgeted()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("login", "Home");
            }

            var UserRepo = new UserRepository(Properties.Settings.Default._connectionString);
            var budgetRepo = new BudgetRepository(Properties.Settings.Default._connectionString);

            var CurrentUser = UserRepo.GetUserByEmail(User.Identity.Name);
            List<Month_Year> MonthYearList =        budgetRepo.MonthYearForUser(CurrentUser.Id);
        

            return Json(MonthYearList.Select(M => new
            {
                Month = M.Month.ToString(),
                Year = M.Year,
                Id = M.Id,
                MonthNum = M.Month
            }),JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetAmountForCategory(int MonthId)
        {
            var budgetRepo = new BudgetRepository(Properties.Settings.Default._connectionString);

            var List = budgetRepo.CategoryListWithAmounts(MonthId);
            var CategoryList = budgetRepo.GetAllCategories();

            List<CategoryWithAmountForMonth> CategoriesWithAMount = new List<CategoryWithAmountForMonth>();
            foreach (Category C in CategoryList)
            {
                CategoryWithAmountForMonth CategoryWithSum = new CategoryWithAmountForMonth();

                CategoryWithSum.CategoryName = C.CategoryName;
                CategoryWithSum.Sum = List.Where(D => D.Category.CategoryName == C.CategoryName).Select(D => D.Amount).Sum();

                CategoriesWithAMount.Add(CategoryWithSum);
              //  CategoryWithSum.Sum = List.Select((D => D.Amount)).Where (D => D.am)


            }
            return Json(CategoriesWithAMount.Select(C => new
            {
                categoryName = C.CategoryName,
                amount = C.Sum
            }), JsonRequestBehavior.AllowGet);
         

        }

    }
    }
