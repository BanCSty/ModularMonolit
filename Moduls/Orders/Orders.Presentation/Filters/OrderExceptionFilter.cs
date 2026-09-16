using System.Threading;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Orders.Application.Exceptions;

namespace Orders.Presentation.Filters;

public class OrderExceptionFilter : IExceptionFilter
{
    private readonly ILogger<OrderExceptionFilter> _logger;

    public OrderExceptionFilter(ILogger<OrderExceptionFilter> logger)
    {
        _logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
        switch (context.Exception)
        {
            case OrderNotFoundException orderNotFoundException:
                context.Result = new NotFoundObjectResult(new
                {
                    error = orderNotFoundException.Message,
                    orderId = orderNotFoundException.OrderId
                });
                context.ExceptionHandled = true;
                break;

            case ArgumentException argumentException:
                context.Result = new BadRequestObjectResult(new
                {
                    error = argumentException.Message
                });
                context.ExceptionHandled = true;
                break;

            default:
                _logger.LogError(context.Exception, "Unhandled exception in Orders module");
                break;
        }
    }
}