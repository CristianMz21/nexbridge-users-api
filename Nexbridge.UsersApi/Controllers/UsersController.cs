using Microsoft.AspNetCore.Mvc;

using Nexbridge.UsersApi.Application.Interfaces;
using Nexbridge.UsersApi.Application.Results;
using Nexbridge.UsersApi.Contracts.Users;

namespace Nexbridge.UsersApi.Controllers;

[ApiController]
[Route("users")]
public sealed class UsersController(IUserService userService) : ControllerBase
{
    [HttpGet]
    public ActionResult<IReadOnlyCollection<UserResponse>> GetAll()
    {
        return Ok(userService.GetAll());
    }

    [HttpGet("{id:guid}", Name = "GetUserById")]
    public IActionResult GetById(Guid id)
    {
        var result = userService.GetById(id);

        if (result.Status == UserResultStatus.Success)
        {
            return Ok(result.Value);
        }

        return result.Status == UserResultStatus.NotFound
            ? NotFound(ProblemResult(result, HttpContext.Request.Path))
            : ConflictOrBadRequest(result, HttpContext.Request.Path);
    }

    [HttpPost]
    public IActionResult Create(CreateUserRequest request)
    {
        var result = userService.Create(request);

        if (result.Status == UserResultStatus.Success)
        {
            return CreatedAtAction(
                nameof(GetById),
                new
                {
                    id = result.Value!.Id
                },
                result.Value
            );
        }

        if (result.Status == UserResultStatus.InvalidInput)
        {
            return BadRequest(ValidationProblemResult(result));
        }

        return ConflictOrBadRequest(result, HttpContext.Request.Path);
    }

    [HttpPut("{id:guid}")]
    public IActionResult Update(Guid id, UpdateUserRequest request)
    {
        var result = userService.Update(id, request);

        if (result.Status == UserResultStatus.Success)
        {
            return Ok(result.Value);
        }

        return result.Status switch
        {
            UserResultStatus.InvalidInput => BadRequest(ValidationProblemResult(result)),
            UserResultStatus.NotFound => NotFound(ProblemResult(result, HttpContext.Request.Path)),
            _ => ConflictOrBadRequest(result, HttpContext.Request.Path)
        };
    }

    [HttpDelete("{id:guid}")]
    public IActionResult Delete(Guid id)
    {
        var result = userService.Delete(id);

        if (result.Status == UserResultStatus.Success)
        {
            return NoContent();
        }

        return result.Status == UserResultStatus.NotFound
            ? NotFound(ProblemResult(result, HttpContext.Request.Path))
            : ConflictOrBadRequest(result, HttpContext.Request.Path);
    }

    private static ValidationProblemDetails ValidationProblemResult<T>(UserResult<T> result)
    {
        var validationErrors = result.ValidationErrors is null
            ? new Dictionary<string, string[]>()
            : new Dictionary<string, string[]>(result.ValidationErrors);

        return new ValidationProblemDetails(validationErrors)
        {
            Type = "https://api.nexbridge.local/problems/validation",
            Title = result.Title,
            Detail = result.Detail,
            Status = StatusCodes.Status400BadRequest
        };
    }

    private static ProblemDetails ProblemResult<T>(UserResult<T> result, string instance)
    {
        return new ProblemDetails
        {
            Type = result.Status switch
            {
                UserResultStatus.NotFound => "https://api.nexbridge.local/problems/not-found",
                UserResultStatus.EmailConflict => "https://api.nexbridge.local/problems/conflict",
                UserResultStatus.UpdateConflict => "https://api.nexbridge.local/problems/conflict",
                _ => "https://api.nexbridge.local/problems/api"
            },
            Title = result.Title,
            Detail = result.Detail,
            Status = result.Status switch
            {
                UserResultStatus.NotFound => StatusCodes.Status404NotFound,
                UserResultStatus.EmailConflict => StatusCodes.Status409Conflict,
                UserResultStatus.UpdateConflict => StatusCodes.Status409Conflict,
                _ => StatusCodes.Status500InternalServerError
            },
            Instance = instance
        };
    }

    private IActionResult ConflictOrBadRequest<T>(
        UserResult<T> result,
        string instance
    )
    {
        if (result.Status == UserResultStatus.InvalidInput)
        {
            return BadRequest(ValidationProblemResult(result));
        }

        if (result.Status is UserResultStatus.NotFound)
        {
            return NotFound(ProblemResult(result, instance));
        }

        if (result.Status is UserResultStatus.EmailConflict or UserResultStatus.UpdateConflict)
        {
            return Conflict(ProblemResult(result, instance));
        }

        return BadRequest(new ProblemDetails
        {
            Title = result.Title,
            Detail = result.Detail,
            Status = StatusCodes.Status400BadRequest,
            Instance = instance
        });
    }
}
