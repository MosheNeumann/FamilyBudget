using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyBudget.data
{
 public   class UserRepository
    {
        readonly string _connectionString;

       

        public UserRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void AddUser(User user)
        {
            using (ContextDBDataContext context = new ContextDBDataContext(_connectionString))
            {
                context.Users.InsertOnSubmit(user);
                context.SubmitChanges();

            }

        }

        // returns all user objects in DB. 
        public List<User> GetAllUsers()
        {

            using (ContextDBDataContext context = new ContextDBDataContext(_connectionString))
            {
                return context.Users.ToList();

            }
         
        }
        // gets user by email
        public User GetUserByEmail(string Email)
        {
            using (ContextDBDataContext context = new ContextDBDataContext(_connectionString))
            {
               

            return context.Users.FirstOrDefault(User => User.Email == Email);
            }

        }

        

        //returns a context, so that a new context does not have to be written in each function.
        private ContextDBDataContext ReturnContext()
        {
            using(ContextDBDataContext context = new ContextDBDataContext(_connectionString))
            {
                return context;
            }
        }
    }
}
