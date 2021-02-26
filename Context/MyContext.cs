using System.Collections.Generic;
using BankAccounts.Models;
using Microsoft.EntityFrameworkCore;

namespace BankAccounts.Context
{
    public class MyContext : DbContext
    {
        public MyContext(DbContextOptions options) : base(options) { }

        public DbSet<User> Users {get; set;}
        public DbSet<Transaction> Transactions {get; set;}
    }
}