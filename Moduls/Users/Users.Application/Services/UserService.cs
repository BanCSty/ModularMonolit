using Shared.Events;
using Shared.Outbox;
using Users.Application.DTOs;
using Users.Application.Exceptions;
using Users.Domain.Entities;
using Users.Domain.Repositories;
using Users.Infrastructure.Persistence;

namespace Users.Application.Services;


public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IOutboxService<UserDbContext> _outboxService;
    private readonly UserUnitOfWork _userUnitOfWork;

    public UserService(IUserRepository userRepository, IOutboxService<UserDbContext> outboxService, UserUnitOfWork userUnitOfWork)
    {
        _userRepository = userRepository;
        _outboxService = outboxService;
        _userUnitOfWork = userUnitOfWork;
    }

    public async Task<IEnumerable<UserDto>> GetAllUsersAsync(CancellationToken cancellationToken = default)
    {
        var users = await _userRepository.GetAllAsync(cancellationToken);
        return users.Select(MapToDto);
    }

    public async Task<UserDto> GetUserByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        if (user is null)
            throw new UserNotFoundException(id);

        return MapToDto(user);
    }

    public async Task<UserDto> CreateUserAsync(CreateUserDto createUserDto, CancellationToken cancellationToken = default)
    {
        try
        {
            await _userUnitOfWork.BeginTransactionAsync(cancellationToken);
            var user = new User(createUserDto.Name, createUserDto.Email);
            var createdUser = await _userRepository.AddAsync(user, cancellationToken);

            Console.WriteLine($"UserRepository DbContext: {_userRepository.GetDbContextHashCode()}");
            Console.WriteLine($"OutboxService DbContext: {_outboxService.GetDbContextHashCode()}");
            Console.WriteLine($"UnitOfWork DbContext: {_userUnitOfWork.GetDbContextHashCode()}");

            Console.WriteLine($"[UserService] Before Outbox: {_userUnitOfWork.GetPendingChangesCount()}");


            await _outboxService.AddEventAsync(new UserCreatedEvent(
                user.Id,
                user.Name,
                user.Email,
                DateTime.UtcNow
            ));

            Console.WriteLine($"Changes before SaveChanges: {_userUnitOfWork.GetPendingChangesCount()}");

            await _userUnitOfWork.SaveChangesAsync(cancellationToken);
            await _userUnitOfWork.CommitTransactionAsync(cancellationToken);
            return MapToDto(createdUser);
        }
        catch (Exception)
        {
            await _userUnitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }        
    }

    public async Task<UserDto> UpdateUserAsync(int id, UpdateUserDto updateUserDto, CancellationToken cancellationToken = default)
    {

        try
        {
            await _userUnitOfWork.BeginTransactionAsync(cancellationToken);

            var user = await _userRepository.GetByIdAsync(id, cancellationToken);
            if (user is null)
                throw new UserNotFoundException(id);

            user.Update(updateUserDto.Name, updateUserDto.Email);
            await _userRepository.UpdateAsync(user, cancellationToken);

            await _outboxService.AddEventAsync(new UserUpdatedEvent(
                user.Id,
                user.Name,
                user.Email,
                DateTime.UtcNow
            ));

            await _userUnitOfWork.SaveChangesAsync(cancellationToken);
            await _userUnitOfWork.CommitTransactionAsync(cancellationToken);
            return MapToDto(user);
        }
        catch (Exception)
        {
            await _userUnitOfWork.RollbackTransactionAsync(cancellationToken);  
            throw;
        }      
    }

    public async Task DeleteUserAsync(int id, CancellationToken cancellationToken = default)
    {
        if (!await _userRepository.ExistsAsync(id, cancellationToken))
            throw new UserNotFoundException(id);

        await _userRepository.DeleteAsync(id, cancellationToken);
    }

    public async Task<IEnumerable<OutboxMessage>> GetOutboxMessageAsync(CancellationToken cancellation = default)
    {
        return await _userRepository.GetOutboxMessageAsync(cancellation);
    }

    private static UserDto MapToDto(User user) => new UserDto(
        user.Id,
        user.Name,
        user.Email,
        user.CreatedAt
    );
}