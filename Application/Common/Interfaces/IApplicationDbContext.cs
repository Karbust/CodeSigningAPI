using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DatabaseFacade Database { get; }
    
    Task<int> SaveChangesAsync();
    
    #region DbSets
    
    public DbSet<CodeSigning> CodeSignings { get; set; }
    public DbSet<AuthTokens> AuthTokens { get; set; }
    public DbSet<IPsWhitelist> IPsWhitelist { get; set; }
    
    #endregion
}