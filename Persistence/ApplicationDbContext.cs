using Application.Common.Interfaces;
using Domain.Common;
using Domain.Constants;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Persistence;

public sealed class ApplicationDbContext: DbContext, IApplicationDbContext
{
    private readonly IDateTime _dateTime;
    
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public ApplicationDbContext(
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        DbContextOptions<ApplicationDbContext> options,
        IDateTime dateTime
        ) : base(options)
    {
        _dateTime = dateTime;
        
        Database.SetCommandTimeout(TimeSpan.FromMinutes(1));
    }
    
    public async Task<int> SaveChangesAsync()
    {
        FillAuditableEntity();
        
        var result = await base.SaveChangesAsync();
        
        return result;
    }

    #region DbSets
    
    public DbSet<CodeSigning> CodeSignings { get; set; }
    public DbSet<AuthTokens> AuthTokens { get; set; }
    public DbSet<IPsWhitelist> IPsWhitelist { get; set; }
    
    #endregion

    #region Private
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuthTokens>().HasData(
            new AuthTokens
            {
                Id = Generic.System.TokenId,
                Token = Generic.System.TokenName,
                Description = Generic.System.TokenDescription,
                IsRevoked = false,
                CreatedOn = DateTime.UtcNow,
                UpdatedOn = DateTime.UtcNow
            });

        modelBuilder.Entity<IPsWhitelist>(x =>
        {
            x.HasKey(table => new { table.Id, table.IP });
            x.Property(e => e.Id).ValueGeneratedOnAdd();
            x.HasIndex(table => table.IP).IsUnique();
        });
    }

    private void FillAuditableEntity()
    {
        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedOn = _dateTime.Now;
                    entry.Entity.UpdatedOn = _dateTime.Now;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedOn = _dateTime.Now;
                    break;
            }
        }
    }

    #endregion
}