using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Challenge.Commands.DeleteConcept;

public record DeleteConceptCommand(Guid ConceptId) : IRequest<Result>;
