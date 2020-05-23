using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Newtonsoft.Json;

namespace FamilyArchive.Models.DbModels
{
    public partial class FamilyArchiveContext : DbContext
    {
        public FamilyArchiveContext()
        {
        }
        public virtual DbSet<Photos> Photos { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var jsonConfig = File.ReadAllLines(".\\appsettings.json");
                var values = JsonConvert.DeserializeObject<Dictionary<string, string>>("{" + jsonConfig[6] + "}");
                optionsBuilder.UseSqlServer(values["ConnectionString"]);             
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("ProductVersion", "2.2.6-servicing-10079");

            modelBuilder.Entity<Photos>(entity =>
            {
                entity.Property(e => e.Description).HasMaxLength(300);

                entity.Property(e => e.Header).HasMaxLength(50);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.PhotoBase64).IsRequired();
            });
        }
    }
}
