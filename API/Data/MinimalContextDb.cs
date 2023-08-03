using API.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class MinimalContextDb : DbContext
    {
        public MinimalContextDb(DbContextOptions<MinimalContextDb> options) : base(options) { }

        public DbSet<Conta> Contas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Conta>()
                .HasKey(p => p.Id);

            modelBuilder.Entity<Conta>()
                .Property(p => p.Name)
                .IsRequired()
                .HasColumnType("varchar(30)");

            modelBuilder.Entity<Conta>()
                .Property(p => p.Description)
                .IsRequired()
                .HasColumnType("varchar(200)");

            modelBuilder.Entity<Conta>()
                .ToTable("Contas");

            base.OnModelCreating(modelBuilder);
        }
    }
}