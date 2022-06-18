using Docpro.DAL.Entity;
using Docpro.DAL.extend;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Docpro.DAL.Database
{
    public class AppDbContext:IdentityDbContext<ApplicationUser>
    {
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<ApplicationUser>(user =>
            {
                user.Property(x => x.FullName)
                .HasComputedColumnSql("[FirstName] + ' ' + [LastName]");
            });

            builder.Entity<Request>(r =>
            r.HasOne(r => r.Patient)
            .WithOne(p => p.Request)
            .HasForeignKey<Request>(r => r.PatientId)
            );

            builder.Entity<Book>().HasKey(b => new { b.PatientId, b.DoctorId });


            builder.Entity<Book>()
                .HasOne(b => b.Patient)
                .WithMany(p => p.PatientBooks)
                .HasForeignKey(b => b.PatientId);

            builder.Entity<Book>()
                .HasOne(b => b.Doctor)
                .WithMany(d => d.DoctorBooks)
                .HasForeignKey(b => b.DoctorId);

            builder.Entity<BooKRepot>()
                .HasOne(b => b.Patient)
                .WithMany(p => p.PatientReports)
                .HasForeignKey(b => b.PatientId);

            builder.Entity<BooKRepot>()
                .HasOne(b => b.Doctor)
                .WithMany(d => d.DoctorReports)
                .HasForeignKey(b => b.DoctorId);



            base.OnModelCreating(builder);
        }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public virtual DbSet<Section> Sections { get; set; }    
        public virtual DbSet<Request> Requests { get; set; }
        public virtual DbSet<Post> Posts { get; set; }
        public virtual DbSet<AvailableTimes> AvailableTimes { get; set; }
        public virtual DbSet<Book> Books { get; set; }
        public virtual DbSet<BooKRepot> Reports { get; set; }
    }
}
