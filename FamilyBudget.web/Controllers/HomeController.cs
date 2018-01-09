using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FamilyBudget.data;
using FamilyBudget.web.Models;
using System.Web.Security;
using System.Net.Mail;
using System.Net;
//using System.Windows.Forms;

namespace FamilyBudget.web.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("budget", "budget");
            }
            return View();
        }

        public ActionResult About()
        {

            return View();
        }
        public ActionResult Login()
        {
            return View();
        }

        public ActionResult Contact()
        {

            return View();
        }
        [HttpPost]
        public ActionResult Contact(string Name, string email, string comment)
        {
            var fromAddress = new MailAddress("online.familybudget@gmail.com", Name);
            var toAddress = new MailAddress("online.familybudget@gmail.com", "Moshe");
            const string fromPassword = "firstliveproject";
            const string subject = "New question/comment on familybudgeting";
            string body = "From: " + email + "comment: " + comment;
            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
            };
            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body
            })
            {
                smtp.Send(message);
            }

            TempData["CommentSent"] = "Your Comment was sent";
            return View();
        }
        [HttpPost]
        public ActionResult Login(string email, string password)
        {
            var UserRepo = new UserRepository((Properties.Settings.Default._connectionString));
            User user = UserRepo.GetUserByEmail(email);

            if (user == null || password == null || password == "" || !PasswordHelper.PasswordHash(password,user.PasswordSalt,user.PasswordHash)) 
            {
                CreateTempDataForLogin(email);
                return Redirect("/Home/Login");

            }
           
            FormsAuthentication.SetAuthCookie(user.Email, true);

            return RedirectToAction("Budget", "Budget");
        }
        public ActionResult Register()
        {

            return View();
        }
        [HttpPost]
        public ActionResult Register(User user, string Password, string PasswordConfirm)
        {
            RegisterErrorModel ErrorModel = new RegisterErrorModel();

            if (!ConfirmPasswordIsValid(Password, PasswordConfirm))
            {              
                 CreateTempData(user, "Please enter a valid Password");
                return Redirect("/Home/Register");
            }

            else if (!ConfirmUserIsValid(user))
            {
                CreateTempData(user, "Please fill out the form correctly");
                return Redirect("/Home/Register");
            }

            else if (!IsEmailAvail(user.Email))
             { 
                    user.Email = "";
                CreateTempData(user, "The email is already usedy try again or click forgot your password");
                return Redirect("/Home/Register");
            }
            
            user.PasswordSalt = PasswordHelper.GenerateSalt();
            user.PasswordHash = PasswordHelper.HashPassword(Password, user.PasswordSalt);

            // adds user to DB
            user.Email = user.Email.ToLower();
            new UserRepository(Properties.Settings.Default._connectionString).AddUser(user);

            FormsAuthentication.SetAuthCookie(user.Email, true);

            return RedirectToAction("Budget","budget");

          //  return Redirect("/Budget/Home");
        }

        //creates the tempdata and returns to register page
        public void CreateTempData(User User, string ErrorMessage)
        {
            RegisterErrorModel ErrorModel = new RegisterErrorModel();

            ErrorModel.ErrorMessage = ErrorMessage;
            ErrorModel.user = User;

            TempData["errorModel"] = (RegisterErrorModel)ErrorModel;
            //return Redirect("/Home/Register");
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return Redirect("/Home/Login");
        }

        //creates a tempdata for the login page

        public void CreateTempDataForLogin(string Email)
        {
            TempData["unrecognizableEmail"] = Email;

        }

        static bool ConfirmPasswordIsValid(string Password, string PasswordConfirm)
        {
            if ((Password != PasswordConfirm) || (Password == ""))
            {
                return false;
            }
            return true;
        }

        static bool ConfirmUserIsValid(User user)
        {
            if (user.FirstName == null|| user.LastName == null || user.Email ==null || !user.Email.Contains("@") )
            {
                return false;
            }
            return true;
        }

        // checks if email address was used already
        static bool IsEmailAvail(string Email)
        {
            var AllUsers = new UserRepository(Properties.Settings.Default._connectionString).GetAllUsers();


            if (AllUsers.Any(user => user.Email == Email))
            {
                return false;
            }

            return true;
        }
    }

  
}

   