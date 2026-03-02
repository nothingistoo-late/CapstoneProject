using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Challenge.Commands.UpdateConcept;

public record UpdateConceptCommand(Guid ConceptId, string Name, string? Description = null) : IRequest<Result>;
