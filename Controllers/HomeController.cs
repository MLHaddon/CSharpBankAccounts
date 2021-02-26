using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BankAccounts.Models;
using BankAccounts.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace BankAccounts.Controllers
{
    public class HomeController : Controller
    {
        private static MyContext _context;

        public HomeController(MyContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        //! LOGIN & REGISTRATION

        [HttpPost("register")]
        public IActionResult RegisterUser(User user)
        {
            if (ModelState.IsValid)
            {
                // Initializing a PasswordHasher object, providing our User class as its type
                PasswordHasher<User> Hasher = new PasswordHasher<User>();
                user.Password = Hasher.HashPassword(user, user.Password);
                user.CreatedAt = DateTime.Now;
                user.UpdatedAt = DateTime.Now;

                _context.Users.Add(user);
                _context.SaveChanges();
                HttpContext.Session.SetInt32("UserID", user.UserID);
                return Redirect($"/Account/{HttpContext.Session.GetInt32("UserID")}");
            } 
            else 
            {
                return View("Index");
            }
        }

        [HttpPost("auth")]
        public IActionResult Login(LoginUser user)
        {
            if (ModelState.IsValid)
            {
                User pulledUser = _context.Users.FirstOrDefault(p => p.Email.Contains(user.LoginEmail));
                if (pulledUser == null) 
                {
                    ModelState.AddModelError("LoginEmail", "Email/Password Invalid");
                    return View("Index");
                }
                // Initialize hasher object
                var hasher = new PasswordHasher<LoginUser>();
                // verify provided password against hash stored in db
                var result = hasher.VerifyHashedPassword(user, pulledUser.Password, user.LoginPassword);
                // result can be compared to 0 for failure
                if(result == 0)
                {
                    // handle failure (this should be similar to how "existing email" is handled)
                    ModelState.AddModelError("LoginPassword", "Email/password Invalid");
                    return View("Index");
                }
                else {
                    HttpContext.Session.SetInt32("UserID", pulledUser.UserID);
                    return Redirect($"/Account/{HttpContext.Session.GetInt32("UserID")}");
                }
            }
            else
            {
                return View("Index");
            }
        }

        public IActionResult NotAuthorized(LoginUser user)
        {
                return View("Index");
        }

        [HttpGet("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return View("Index");
        }

        //! MAIN WEBSITE

        [HttpGet("Account/{UserID}")]
        public IActionResult Account()
        {
            if (HttpContext.Session.GetInt32("UserID") == null)
            {
                return RedirectToAction("NotAuthorized");
            }
            int userID = (int)HttpContext.Session.GetInt32("UserID");
            ViewBag.MemberDetails = _context.Users
                .FirstOrDefault(u => u.UserID == userID);
            ViewBag.UserTransactions = _context.Transactions
                .Where(t => t.UserID == userID)
                .ToList();

            ViewBag.UserBalance = 0;
            foreach (Transaction trans in ViewBag.UserTransactions)
            {
                ViewBag.UserBalance += trans.Amount;
                ViewBag.UserBalance = Math.Round(ViewBag.UserBalance, 2);
            }
            Console.WriteLine("User Current Balance: " + ViewBag.UserBalance);
            return View();
        }

        [HttpPost("PlaceTransaction")]
        public IActionResult PlaceTransaction(Transaction transaction)
        {
            if (ModelState.IsValid)
            {
                // Pull the current user
                User PulledUser = _context.Users
                    .Include(u => u.Transactions)
                    .FirstOrDefault(u => u.UserID == (int)HttpContext.Session.GetInt32("UserID"));

                // Check if the current balance is 0 with the added amount
                float TransTotal = 0;
                foreach (Transaction trans in PulledUser.Transactions)
                {
                    TransTotal += trans.Amount;
                }
                TransTotal += transaction.Amount;
                
                // Save to the database if ModelState is Valid
                if (TransTotal < 0)
                {
                    ModelState.AddModelError("Amount", "Balance must be above 0");
                    if (HttpContext.Session.GetInt32("UserID") == null)
                    {
                        return RedirectToAction("NotAuthorized");
                    }
                    int userID = (int)HttpContext.Session.GetInt32("UserID");
                    ViewBag.MemberDetails = _context.Users
                        .FirstOrDefault(u => u.UserID == userID);
                    ViewBag.UserTransactions = _context.Transactions
                        .Where(t => t.UserID == userID)
                        .ToList();

                    ViewBag.UserBalance = 0;
                    foreach (Transaction trans in ViewBag.UserTransactions)
                    {
                        ViewBag.UserBalance += trans.Amount;
                        ViewBag.UserBalance = Math.Round(ViewBag.UserBalance, 2);
                    }
                    Console.WriteLine("User Current Balance: " + ViewBag.UserBalance);
                    return View("Account");
                }
                PulledUser.Transactions.Add(transaction);
                PulledUser.UpdatedAt = DateTime.Now;
                _context.SaveChanges();
                return Redirect("Account/{UserID}");
            }
            else
            {
                return View("Account");
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
