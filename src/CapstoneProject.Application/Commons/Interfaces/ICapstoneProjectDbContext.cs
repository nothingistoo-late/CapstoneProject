using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Application.Common.Interfaces;

public interface ICapstoneProjectDbContext
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}