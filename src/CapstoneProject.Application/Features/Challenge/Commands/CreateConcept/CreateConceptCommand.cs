using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Challenge.Commands.CreateConcept;

public record CreateConceptCommand(string Name, string? Description = null) : IRequest<Result<Guid>>;
