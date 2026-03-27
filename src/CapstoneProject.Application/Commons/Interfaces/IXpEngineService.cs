using CapstoneProject.Application.Common.Models;
using CapstoneProject.Application.Commons.Models.Xp;

namespace CapstoneProject.Application.Common.Interfaces;

public interface IXpEngineService
{
    Task<Result<XpGrantResult>> GrantXpAsync(XpGrantInput input, CancellationToken cancellationToken = default);
}

