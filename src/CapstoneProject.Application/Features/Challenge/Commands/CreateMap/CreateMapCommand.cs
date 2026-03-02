using CapstoneProject.Application.Commons.DTOs.Challenge;
using CapstoneProject.Application.Common.Models;
using MediatR;

namespace CapstoneProject.Application.Features.Challenge.Commands.CreateMap;

public record CreateMapCommand(CreateMapRequest Request) : IRequest<Result<Guid>>;
