using GrapheneTrace.Models;
using Microsoft.EntityFrameworkCore;

namespace GrapheneTrace.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<UserAccount> UserAccounts => Set<UserAccount>();
        public DbSet<Patient> Patients => Set<Patient>();
        public DbSet<Clinician> Clinicians => Set<Clinician>();
        public DbSet<Admin> Admins => Set<Admin>();

        public DbSet<DataFile> DataFiles => Set<DataFile>();
        public DbSet<PressureFrame> PressureFrames => Set<PressureFrame>();
        public DbSet<Alert> Alerts => Set<Alert>();
        public DbSet<Feedback> Feedbacks => Set<Feedback>();
        public DbSet<Appointment> Appointments => Set<Appointment>();
        public DbSet<Prescription> Prescriptions => Set<Prescription>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

        public DbSet<Appointment> Appointment { get; set; }


        protected override void OnModelCreating(ModelBuilder model)
        {
            base.OnModelCreating(model);

            // ===== USER ↔ ROLE MAPPINGS =====
            model.Entity<Patient>()
                .HasOne(p => p.UserAccount)
                .WithOne(u => u.Patient)
                .HasForeignKey<Patient>(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            model.Entity<Clinician>()
                .HasOne(c => c.UserAccount)
                .WithOne(u => u.Clinician)
                .HasForeignKey<Clinician>(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            model.Entity<Admin>()
                .HasOne(a => a.UserAccount)
                .WithOne(u => u.Admin)
                .HasForeignKey<Admin>(a => a.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ===== PATIENT → DATA FILES =====
            model.Entity<DataFile>()
                .HasOne(df => df.Patient)
                .WithMany(p => p.DataFiles)
                .HasForeignKey(df => df.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            // ===== DATAFILE → FRAMES =====
            model.Entity<PressureFrame>()
                .HasOne(pf => pf.DataFile)
                .WithMany(df => df.Frames)
                .HasForeignKey(pf => pf.DataFileId)
                .OnDelete(DeleteBehavior.Restrict);

            // ===== ALERT RELATIONSHIPS =====
            model.Entity<Alert>()
                .HasOne(a => a.Patient)
                .WithMany(p => p.Alerts)
                .HasForeignKey(a => a.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            model.Entity<Alert>()
                .HasOne(a => a.Frame)
                .WithMany()
                .HasForeignKey(a => a.FrameId)
                .OnDelete(DeleteBehavior.Restrict);

            model.Entity<Alert>()
                .HasOne(a => a.RaisedByUser)
                .WithMany()
                .HasForeignKey(a => a.RaisedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            model.Entity<Alert>()
                .HasOne(a => a.ResolvedByUser)
                .WithMany()
                .HasForeignKey(a => a.ResolvedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ===== GLOBAL FIX: REMOVE CASCADES =====
            foreach (var fk in model.Model.GetEntityTypes().SelectMany(t => t.GetForeignKeys()))
                fk.DeleteBehavior = DeleteBehavior.Restrict;
        }
    }
}
