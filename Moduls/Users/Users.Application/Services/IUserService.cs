using Shared.Outbox;
using Users.Application.DTOs;

namespace Users.Application.Services;

public interface IUserService
{
    Task<IEnumerable<UserDto>> GetAllUsersAsync(CancellationToken cancellationToken = default);
    Task<UserDto> GetUserByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<UserDto> CreateUserAsync(CreateUserDto createUserDto, CancellationToken cancellationToken = default);
    Task<UserDto> UpdateUserAsync(int id, UpdateUserDto updateUserDto, CancellationToken cancellationToken = default);
    Task DeleteUserAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<OutboxMessage>> GetOutboxMessageAsync(CancellationToken cancellation = default);

}

public record CreateUserDto(string Name, string Email);
public record UpdateUserDto(string Name, string Email);