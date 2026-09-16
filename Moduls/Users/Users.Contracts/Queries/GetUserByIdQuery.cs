using MediatR;
using Users.Contracts.DTOs;

namespace Users.Contracts.Queries;

public record GetUserByIdQuery(int UserId) : IRequest<UserDto?>;