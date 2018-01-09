using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FamilyBudget.data;

namespace FamilyBudget.web.Models
{
    public class LayoutDataAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                var UserRepo = new UserRepository(Properties.Settings.Default._connectionString);
                var user = UserRepo.GetUserByEmail(filterContext.HttpContext.User.Identity.Name);
                filterContext.Controller.ViewBag.User = user;
            }

            base.OnActionExecuting(filterContext);
        }
    }
}