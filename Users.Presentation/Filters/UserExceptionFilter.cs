using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Users.Application.Exceptions;

namespace Users.Presentation.Filters;

public class UserExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is UserNotFoundException userNotFoundException)
        {
            context.Result = new NotFoundObjectResult(new
            {
                error = userNotFoundException.Message,
                userId = userNotFoundException.UserId
            });
            context.ExceptionHandled = true;
        }
    }
}