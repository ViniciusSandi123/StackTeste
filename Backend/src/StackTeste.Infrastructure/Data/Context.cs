using Microsoft.EntityFrameworkCore;
using StackTeste.Domain.Models;

namespace StackTeste.Infrastructure.Data
{
    public class Context : DbContext
    {
        public DbSet<Lead> Leads => Set<Lead>();
        public DbSet<TaskItem> TaskItens => Set<TaskItem>();

        public Context(DbContextOptions<Context> dbContextOptions) : base(dbContextOptions) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Lead>(entity =>
            {
                entity.ToTable("Leads");

                entity.HasKey(l => l.Id);

                entity.Property(l => l.Name)
                      .IsRequired()
                      .HasMaxLength(150);

                entity.Property(l => l.Email)
                      .IsRequired()
                      .HasMaxLength(200);

                entity.HasIndex(l => l.Email);

                entity.Property(l => l.Status)
                      .IsRequired();

                entity.Property(l => l.CreatedAt).IsRequired();
                entity.Property(l => l.UpdatedAt).IsRequired();

                entity.HasMany(l => l.Tasks)
                      .WithOne(t => t.Lead)
                      .HasForeignKey(t => t.LeadId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<TaskItem>(entity =>
            {
                entity.ToTable("Tasks");

                entity.HasKey(t => t.Id);

                entity.Property(t => t.Title)
                      .IsRequired()
                      .HasMaxLength(200);

                entity.Property(t => t.Status)
                      .IsRequired();

                entity.Property(t => t.DueDate);

                entity.Property(t => t.CreatedAt).IsRequired();
                entity.Property(t => t.UpdatedAt).IsRequired();

                entity.HasIndex(t => t.LeadId);
            });

            base.OnModelCreating(modelBuilder);
        }

        public override int SaveChanges()
        {
            ApplyTimestamps();
            return base.SaveChanges();
        }

        public override System.Threading.Tasks.Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ApplyTimestamps();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void ApplyTimestamps()
        {
            var now = DateTime.UtcNow;

            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.Entity is Lead lead)
                {
                    if (entry.State == EntityState.Added) 
                    {
                        lead.CreatedAt = now;
                    }
                    if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
                    {
                        lead.UpdatedAt = now;
                    }
                }
                else if (entry.Entity is TaskItem task)
                {
                    if (entry.State == EntityState.Added)
                    {
                        task.CreatedAt = now;
                    }
                    if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
                    {
                        task.UpdatedAt = now;
                    }
                }
            }
        }
    }
}
