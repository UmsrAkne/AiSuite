using System;
using System.IO;
using AiSuite.Utils;
using Microsoft.EntityFrameworkCore;

namespace AiSuite.Databases
{
    public class MyDbContext : DbContext
    {
        public DbSet<LoraModel> LoraModels { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var baseDir = AppContext.BaseDirectory;
                var dbPath = Path.Combine(baseDir, "browse_data.db");
                Logger.Log($"DB Path: {dbPath}");
                optionsBuilder.UseSqlite($"Data Source={dbPath}");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }
    }
}