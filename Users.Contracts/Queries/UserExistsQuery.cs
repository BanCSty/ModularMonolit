using MediatR;

namespace Users.Contracts.Queries;

public record UserExistsQuery(int UserId) : IRequest<bool>;