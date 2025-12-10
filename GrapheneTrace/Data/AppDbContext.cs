using GrapheneTrace.Models;
using Microsoft.EntityFrameworkCore;

namespace GrapheneTrace.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // USER ACCOUNTS
        public DbSet<UserAccount> UserAccounts => Set<UserAccount>();
        public DbSet<Patient> Patients => Set<Patient>();
        public DbSet<Clinician> Clinicians => Set<Clinician>();
        public DbSet<Admin> Admins => Set<Admin>();

        // DATA + ANALYSIS
        public DbSet<DataFile> DataFiles => Set<DataFile>();
        public DbSet<PressureFrame> PressureFrames => Set<PressureFrame>();
        public DbSet<Alert> Alerts => Set<Alert>();
        public DbSet<Feedback> Feedbacks => Set<Feedback>();

        // APPOINTMENTS
        public DbSet<Appointment> Appointments => Set<Appointment>();

        // PRESCRIPTIONS (UPDATED)
        public DbSet<Prescription> Prescriptions { get; set; }

        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

        protected override void OnModelCreating(ModelBuilder model)
        {
            base.OnModelCreating(model);

            // ------------------------------
            // PRESCRIPTION CONFIGURATION
            // ------------------------------
            model.Entity<Prescription>()
                .Property(p => p.Id)
                .HasColumnType("int")
                .ValueGeneratedOnAdd();

            // Prescription → Patient
            model.Entity<Prescription>()
                .HasOne(p => p.Patient)
                .WithMany(pt => pt.Prescriptions)
                .HasForeignKey(p => p.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            // Prescription → Clinician
            model.Entity<Prescription>()
                .HasOne(p => p.Clinician)
                .WithMany(c => c.Prescriptions)
                .HasForeignKey(p => p.ClinicianId)
                .OnDelete(DeleteBehavior.Restrict);

            // ------------------------------
            // OTHER EXISTING CONFIGURATIONS
            // ------------------------------

            // USER ↔ PATIENT
            model.Entity<Patient>()
                .HasOne(p => p.UserAccount)
                .WithOne(u => u.Patient)
                .HasForeignKey<Patient>(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // USER ↔ CLINICIAN
            model.Entity<Clinician>()
                .HasOne(c => c.UserAccount)
                .WithOne(u => u.Clinician)
                .HasForeignKey<Clinician>(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // USER ↔ ADMIN
            model.Entity<Admin>()
                .HasOne(a => a.UserAccount)
                .WithOne(u => u.Admin)
                .HasForeignKey<Admin>(a => a.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // PATIENT → DATA FILES
            model.Entity<DataFile>()
                .HasOne(df => df.Patient)
                .WithMany(p => p.DataFiles)
                .HasForeignKey(df => df.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            // DATA FILE → FRAMES
            model.Entity<PressureFrame>()
                .HasOne(pf => pf.DataFile)
                .WithMany(df => df.Frames)
                .HasForeignKey(pf => pf.DataFileId)
                .OnDelete(DeleteBehavior.Restrict);

            // ALERT RELATIONSHIPS
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
        }
    }
}
