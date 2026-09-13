using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Shared.Outbox;
using Users.Application.Services;
using Users.Presentation.Contracts.Requests;
using Users.Presentation.Contracts.Responses;
using Users.Presentation.Filters;

namespace Users.Presentation.Controllers;

[ApiController]
[Route("api/users")]
[ServiceFilter(typeof(UserExceptionFilter))]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IValidator<CreateUserRequest> _createUserValidator;
    private readonly IValidator<UpdateUserRequest> _updateUserValidator;

    public UsersController(
        IUserService userService,
        IValidator<CreateUserRequest> createUserValidator,
        IValidator<UpdateUserRequest> updateUserValidator)
    {
        _userService = userService;
        _createUserValidator = createUserValidator;
        _updateUserValidator = updateUserValidator;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserResponse>>> GetAllUsers(CancellationToken cancellationToken)
    {
        var users = await _userService.GetAllUsersAsync(cancellationToken);
        var response = users.Select(u => new UserResponse(u.Id, u.Name, u.Email, u.CreatedAt));
        return Ok(response);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<UserResponse>> GetUserById(int id, CancellationToken cancellationToken)
    {
        var user = await _userService.GetUserByIdAsync(id, cancellationToken);
        var response = new UserResponse(user.Id, user.Name, user.Email, user.CreatedAt);
        return Ok(response);
    }

    [HttpGet("GetOutboxMessages")]
    public async Task<ActionResult<IEnumerable<OutboxMessage>>> GetOutboxMessages(CancellationToken cancellation)
    {
        var messages = await _userService.GetOutboxMessageAsync(cancellation);
        return Ok(messages);
    }

    [HttpPost]
    public async Task<ActionResult<UserResponse>> CreateUser(
        [FromBody] CreateUserRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = await _createUserValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

        var createUserDto = new CreateUserDto(request.Name, request.Email);
        var user = await _userService.CreateUserAsync(createUserDto, cancellationToken);

        var response = new UserResponse(user.Id, user.Name, user.Email, user.CreatedAt);
        return Created($"/api/users/{response.Id}", response);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<UserResponse>> UpdateUser(
        int id,
        [FromBody] UpdateUserRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = await _updateUserValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

        var updateUserDto = new UpdateUserDto(request.Name, request.Email);
        var user = await _userService.UpdateUserAsync(id, updateUserDto, cancellationToken);

        var response = new UserResponse(user.Id, user.Name, user.Email, user.CreatedAt);
        return Ok(response);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteUser(int id, CancellationToken cancellationToken)
    {
        await _userService.DeleteUserAsync(id, cancellationToken);
        return NoContent();
    }
}