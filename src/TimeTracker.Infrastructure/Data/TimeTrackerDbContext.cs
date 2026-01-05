using Microsoft.EntityFrameworkCore;
using TimeTracker.Core.Entities;

namespace TimeTracker.Infrastructure.Data;

public class TimeTrackerDbContext : DbContext
{
    public TimeTrackerDbContext(DbContextOptions<TimeTrackerDbContext> options) : base(options)
    {
    }
    
    public DbSet<User> Users => Set<User>();
    public DbSet<TimeSheet> TimeSheets => Set<TimeSheet>();
    public DbSet<TimeEntry> TimeEntries => Set<TimeEntry>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<WorkType> WorkTypes => Set<WorkType>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Email).HasMaxLength(255).IsRequired();
            entity.Property(e => e.PasswordHash).HasMaxLength(500).IsRequired();
            entity.Property(e => e.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.LastName).HasMaxLength(100).IsRequired();
            
            entity.HasMany(e => e.TimeSheets)
                  .WithOne(e => e.User)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
        
        // TimeSheet configuration
        modelBuilder.Entity<TimeSheet>(entity =>
        {
            entity.ToTable("TimeSheets");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.UserId, e.StartDate });
            entity.Property(e => e.TotalHours).HasPrecision(18, 2);
            
            entity.HasMany(e => e.TimeEntries)
                  .WithOne(e => e.TimeSheet)
                  .HasForeignKey(e => e.TimeSheetId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
        
        // TimeEntry configuration
        modelBuilder.Entity<TimeEntry>(entity =>
        {
            entity.ToTable("TimeEntries");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.TimeSheetId);
            entity.HasIndex(e => e.ProjectCode);
            entity.HasIndex(e => e.WorkTypeCode);
            entity.HasIndex(e => e.EntryDate);
            entity.Property(e => e.ProjectCode).HasMaxLength(50).IsRequired();
            entity.Property(e => e.WorkTypeCode).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Hours).HasPrecision(18, 2);
            entity.Property(e => e.Notes).HasMaxLength(1000);
            
            entity.HasOne(e => e.Project)
                  .WithMany(p => p.TimeEntries)
                  .HasForeignKey(e => e.ProjectCode)
                  .HasPrincipalKey(p => p.Code)
                  .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(e => e.WorkType)
                  .WithMany(w => w.TimeEntries)
                  .HasForeignKey(e => e.WorkTypeCode)
                  .HasPrincipalKey(w => w.Code)
                  .OnDelete(DeleteBehavior.Restrict);
        });
        
        // Project configuration
        modelBuilder.Entity<Project>(entity =>
        {
            entity.ToTable("Projects");
            entity.HasKey(e => e.Code);
            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(1000);
        });
        
        // WorkType configuration
        modelBuilder.Entity<WorkType>(entity =>
        {
            entity.ToTable("WorkTypes");
            entity.HasKey(e => e.Code);
            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(1000);
        });
        
        // Seed data
        SeedData(modelBuilder);
    }
    
    private void SeedData(ModelBuilder modelBuilder)
    {
        // Seed Projects
        modelBuilder.Entity<Project>().HasData(
            new Project { Code = "INTERNAL", Name = "Internal Tasks", Description = "Internal company tasks", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Project { Code = "TRAINING", Name = "Training & Development", Description = "Learning and training activities", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Project { Code = "PROJECT-A", Name = "Project Alpha", Description = "Main product development", IsActive = true, CreatedAt = DateTime.UtcNow }
        );
        
        // Seed WorkTypes
        modelBuilder.Entity<WorkType>().HasData(
            new WorkType { Code = "DEV", Name = "Development", Description = "Software development work", IsActive = true, CreatedAt = DateTime.UtcNow },
            new WorkType { Code = "MEET", Name = "Meetings", Description = "Meetings and discussions", IsActive = true, CreatedAt = DateTime.UtcNow },
            new WorkType { Code = "TEST", Name = "Testing", Description = "Quality assurance and testing", IsActive = true, CreatedAt = DateTime.UtcNow },
            new WorkType { Code = "ADMIN", Name = "Administration", Description = "Administrative tasks", IsActive = true, CreatedAt = DateTime.UtcNow },
            new WorkType { Code = "TRAIN", Name = "Training", Description = "Training and learning", IsActive = true, CreatedAt = DateTime.UtcNow },
            new WorkType { Code = "SUPPORT", Name = "Support", Description = "Customer support", IsActive = true, CreatedAt = DateTime.UtcNow }
        );
    }
}
