using System.Security.Claims;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace NDB.Platform.Api.Controllers;

/// <summary>
/// Abstract base controller for all NDB Platform API controllers.
/// Does not inject ILogger — logging belongs in handlers, not controllers.
/// Exposes a lazy IMediator and current-user properties from JWT claims.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public abstract class BaseController : ControllerBase
{
    private IMediator? _mediator;

    /// <summary>Mediator for CQRS dispatch. Lazy-resolved from the DI container on first use.</summary>
    protected IMediator Mediator =>
        _mediator ??= HttpContext.RequestServices.GetRequiredService<IMediator>();

    /// <summary>
    /// ID of the currently authenticated user, from the <c>NameIdentifier</c> claim (<c>sub</c>).
    /// <c>null</c> if the request is not authenticated.
    /// </summary>
    protected string? CurrentUserId =>
        User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    /// <summary>
    /// Display name of the currently authenticated user, from the <c>Name</c> claim.
    /// <c>null</c> if the request is not authenticated.
    /// </summary>
    protected string? CurrentUserName =>
        User.FindFirst(ClaimTypes.Name)?.Value;

    /// <summary>
    /// Role of the currently authenticated user, from the first <c>Role</c> claim.
    /// <c>null</c> if the request is not authenticated or no role claim is present.
    /// </summary>
    protected string? CurrentUserRole =>
        User.FindFirst(ClaimTypes.Role)?.Value;
}
