using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BachelorProject.Models
{
    /** 
     * Tato třída slouží ke generování a aktualizaci databáze pomocí technologie Entity Framework
     * Aby byly vygenerovány třídy pro funkcionalitu spojenou uživateli, je nutné aby dědila ze třídy "IdentityDbContext"
     * **/
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {

        }
        // tabulka zaměstnanců
        public DbSet<Employee> Employees { get; set; }
        // tabulka typů her
        public DbSet<GameType> GameTypes { get; set; }
        // tabulka přemluvených her
        public DbSet<AdditionalGame> AdditionalGames { get; set; }

        //public DbSet<VoucherType> VoucherTypes { get; set; }
        // tabulka voucherů
        public DbSet<Voucher> Vouchers { get; set; }

        // nastavení akce při odstraňování záznamu s referencí na jinou tabulku
        // při tomto nastavení nelze smazat záznam, který je využíván v jiné tabulce (jako cizí klíč)
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            //modelBuilder.Seed();
            foreach (var foreignKey in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
            }

        }

    }
}
