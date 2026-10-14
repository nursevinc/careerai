using CareerAI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;

namespace CareerAI.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Resume> Resumes => Set<Resume>();
    public DbSet<JobApplication> JobApplications => Set<JobApplication>();
    public DbSet<Skill> Skills => Set<Skill>();
    public DbSet<JobApplicationSkill> JobApplicationSkills => Set<JobApplicationSkill>();
    public DbSet<ResumeSkill> ResumeSkills => Set<ResumeSkill>();
    public DbSet<AIAnalysis> AIAnalyses => Set<AIAnalysis>();
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // Tüm DateTime alanları UTC olarak okunsun
        configurationBuilder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.Property(x => x.Name).IsRequired().HasMaxLength(100);
            e.Property(x => x.Email).IsRequired().HasMaxLength(256);
            e.Property(x => x.PasswordHash).IsRequired().HasMaxLength(512);
            e.HasIndex(x => x.Email).IsUnique(); // aynı e-posta ile iki kayıt olamaz
        });

        modelBuilder.Entity<Resume>(e =>
        {
            e.Property(x => x.FileName).IsRequired().HasMaxLength(255);
            e.Property(x => x.StoredFileName).IsRequired().HasMaxLength(100);
            e.Property(x => x.ExtractedText).IsRequired(); // nvarchar(max)
        });

        modelBuilder.Entity<JobApplication>(e =>
        {
            e.Property(x => x.JobTitle).IsRequired().HasMaxLength(200);
            e.Property(x => x.CompanyName).IsRequired().HasMaxLength(200);
            e.Property(x => x.JobDescription).IsRequired(); // nvarchar(max)
        });

        modelBuilder.Entity<Skill>(e =>
        {
            e.Property(x => x.Name).IsRequired().HasMaxLength(100);
            e.HasIndex(x => x.Name).IsUnique(); // "C#" tablosunda tek satır olsun
        });

        modelBuilder.Entity<JobApplicationSkill>(e =>
        {
            e.HasIndex(x => new { x.JobApplicationId, x.SkillId }).IsUnique();

            // Skill silinirse ilişkili kayıtlar sessizce silinmesin
            e.HasOne(x => x.Skill)
             .WithMany()
             .HasForeignKey(x => x.SkillId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ResumeSkill>(e =>
        {
            e.HasIndex(x => new { x.ResumeId, x.SkillId }).IsUnique();

            e.HasOne(x => x.Skill)
             .WithMany()
             .HasForeignKey(x => x.SkillId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AIAnalysis>(e =>
        {
            e.Property(x => x.AnalysisResult).IsRequired(); // nvarchar(max)

            // User -> JobApplication -> AIAnalysis ve User -> Resume -> AIAnalysis
            // iki cascade yolu SQL Server'da hata verir, bu yüzden birini kesiyoruz.
            e.HasOne(x => x.Resume)
             .WithMany()
             .HasForeignKey(x => x.ResumeId)
             .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
