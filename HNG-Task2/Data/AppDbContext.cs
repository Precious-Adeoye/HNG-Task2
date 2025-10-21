using System;
using System.Collections.Generic;
using HNG_Task2.Model;
using Microsoft.EntityFrameworkCore;

namespace HNG_Task2.Data
{
    public class AppDbContext :DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        public DbSet<StringEntity> Strings { get; set; }
    }
}
