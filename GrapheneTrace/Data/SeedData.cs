using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using GrapheneTrace.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GrapheneTrace.Data
{
    public static class SeedData
    {
        // Change this path if your CSVs live somewhere else
        private static readonly string CsvSourceFolder =

            @"C:\Users\tomas\source\repos\Group2GrapheneStudy\GrapheneTraceFinal\csv_files";
  

        /// <summary>
        /// Entry point used from Program.cs:
        /// await SeedData.InitializeAsync(app.Services);
        /// </summary>
        public static Task InitializeAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Ensure database exists. We *do not* run Migrate() here to avoid
            // PendingModelChanges warnings becoming runtime exceptions.
            context.Database.EnsureCreated();

            // 1) Seed users + roles (only if none exist)
            SeedUsers(context);

            // 2) Seed CSV sessions / DataFiles + PressureFrames (only if none exist)
            SeedCsvSessions(context);

            return Task.CompletedTask;
        }

        // -----------------------------
        // USERS / ROLES / PATIENTS
        // -----------------------------
        private static void SeedUsers(AppDbContext context)
        {
            if (context.UserAccounts.Any())
            {
                // Already seeded, don't create duplicates
                return;
            }

            string Hash(string password)
            {
                using var sha = SHA256.Create();
                var bytes = Encoding.UTF8.GetBytes(password);
                var hash = sha.ComputeHash(bytes);
                return Convert.ToHexString(hash);
            }

            const string defaultPassword = "Password123!";

            // ---------- ADMIN ----------
            var adminUser = new UserAccount
            {
                Email = "admin@test.com",
                PasswordHash = Hash(defaultPassword),
                Role = "Admin",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            context.UserAccounts.Add(adminUser);
            context.SaveChanges();

            context.Admins.Add(new Admin
            {
                UserId = adminUser.UserId
            });

            // ---------- CLINICIANS ----------
            var clinicianUsers = new List<UserAccount>
            {
                new UserAccount
                {
                    Email = "clinician1@test.com",
                    PasswordHash = Hash(defaultPassword),
                    Role = "Clinician",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new UserAccount
                {
                    Email = "clinician2@test.com",
                    PasswordHash = Hash(defaultPassword),
                    Role = "Clinician",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            };

            context.UserAccounts.AddRange(clinicianUsers);
            context.SaveChanges();

            context.Clinicians.AddRange(new[]
            {
                new Clinician
                {
                    UserId = clinicianUsers[0].UserId,
                    Specialty = "Tissue Viability",
                    IsAvailable = true
                },
                new Clinician
                {
                    UserId = clinicianUsers[1].UserId,
                    Specialty = "Rehabilitation",
                    IsAvailable = true
                }
            });

            // ---------- PATIENTS ----------
            var patientUsers = new List<UserAccount>();
            for (int i = 1; i <= 5; i++)
            {
                patientUsers.Add(new UserAccount
                {
                    Email = $"patient{i}@test.com",
                    PasswordHash = Hash(defaultPassword),
                    Role = "Patient",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                });
            }

            context.UserAccounts.AddRange(patientUsers);
            context.SaveChanges();

            var patients = new List<Patient>();
            for (int i = 0; i < patientUsers.Count; i++)
            {
                patients.Add(new Patient
                {
                    UserId = patientUsers[i].UserId,
                    ExternalId = $"P{i + 1:D3}",
                    ContactEmail = patientUsers[i].Email,
                    ContactPhone = "0000000000",
                    RiskNotes = "Seeded patient for testing."
                });
            }

            context.Patients.AddRange(patients);
            context.SaveChanges();
        }

        // -----------------------------
        // CSV - DATAFILES / FRAMES
        // -----------------------------
        private static void SeedCsvSessions(AppDbContext context)
        {
            // If we already have data files, don't seed again
            if (context.DataFiles.Any())
                return;

            // If the folder doesn't exist on this machine, just skip 
            if (!Directory.Exists(CsvSourceFolder))
                return;

            var patients = context.Patients
                .OrderBy(p => p.PatientId)
                .ToList();

            if (!patients.Any())
                return;

            // Use admin as uploader
            var adminUserId = context.Admins
                .Select(a => a.UserId)
                .FirstOrDefault();

            if (adminUserId == 0)
                return;

            // Simple analysis
            // Returns the maximum value (peak pressure)
            decimal CalcPeak(int[,] matrix)
            {
                int max = 0;
                foreach (var v in matrix)
                    if (v > max) max = v;
                return max;
            }
            // Returns the average
            decimal CalcAvg(int[,] matrix)
            {
                long total = 0; // long to avoid overflow
                int count = 0;

                // Sum all values and count how many entries we have
                foreach (var v in matrix)
                {
                    total += v;
                    count++;
                }

                // Protect against division by zero
                return count == 0 ? 0 : (decimal)total / count;
            }

            /// <summary>
            /// Calculates the percentage of cells whose value is at or above
            /// a given threshold. Returns a percentage in the range 0, 100
            /// </summary>

            decimal CalcArea(int[,] matrix, int threshold = 10)
            {
                int active = 0;
                int total = matrix.Length;

                // Count how many cells are active
                foreach (var v in matrix)
                    if (v >= threshold) active++;
                return total == 0 ? 0 : (decimal)active / total * 100m;
            }

            /// <summary>
            /// Combines peak pressure and contact area into a risk score:
            ///  Peak contributes 60% scaled from 0–255
            ///  Area contributes 40% scaled from 0–100%
            /// The result is rounded to 2 decimal places
            /// </summary>

            decimal CalcRisk(decimal peak, decimal area)
            {
                return Math.Round((peak / 255m) * 60m + (area / 100m) * 40m, 2);
            }

            // Find all CSV files in the data folder
            var csvFiles = Directory.GetFiles(CsvSourceFolder, "*.csv");
            if (csvFiles.Length == 0)
                return;

            int patientIndex = 0;
            var now = DateTime.UtcNow;

            foreach (var csvPath in csvFiles)
            {
                // Assign files round-robin across available patients
                var patient = patients[patientIndex % patients.Count];
                patientIndex++;

                var dataFile = new DataFile
                {
                    PatientId = patient.PatientId,
                    UploadedByUserId = adminUserId,
                    UploadedAt = now,
                    FilePath = csvPath // Store absolute path
                };

                context.DataFiles.Add(dataFile);
                context.SaveChanges(); // ensures DataFileId is generated

                // Parse CSV into 32x32 frames
                var lines = File.ReadAllLines(csvPath);

                // Ensure the file has a size that is a multiple of 32 lines
                if (lines.Length % 32 != 0)
                    continue; // skip weird CSVs

                int frameCount = lines.Length / 32;
                var baseTime = now; // All frames in this file are relative to this timestamp

                for (int i = 0; i < frameCount; i++)
                {
                    // Build a 32x32 matrix for the current frame
                    int[,] matrix = new int[32, 32];
                    int startRow = i * 32;

                    for (int r = 0; r < 32; r++)
                    {
                        // Split the CSV row into 32 integer values
                        var cells = lines[startRow + r]
                            .Split(',', StringSplitOptions.RemoveEmptyEntries);

                        // Map each CSV value into the matrix cell
                        for (int c = 0; c < 32; c++)
                            matrix[r, c] = int.Parse(cells[c]);
                    }

                    // metrics for this frame
                    var peak = CalcPeak(matrix);
                    var avg = CalcAvg(matrix);
                    var area = CalcArea(matrix);
                    var risk = CalcRisk(peak, area);

                    // Persist the frame with its calculated metrics
                    context.PressureFrames.Add(new PressureFrame
                    {
                        DataFileId = dataFile.DataFileId,
                        FrameIndex = i,
                        CapturedAtUtc = baseTime.AddSeconds(i),
                        PeakPressure = peak,
                        AveragePressure = avg,
                        ContactAreaPercent = area,
                        RiskScore = risk
                    });
                }

                context.SaveChanges();
            }
        }
    }
}