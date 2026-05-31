using Microsoft.AspNetCore.Http;
using NDB.Platform.Abstraction;
using NDB.Platform.Kit.Identifiers;
using System.Security.Claims;

namespace NDB.Platform.Api.Authentication;

/// <summary>
/// <see cref="IActorAccessor"/> implementation for web/API projects — reads actor identity from JWT claims in <see cref="HttpContext"/>.
/// Registered as Scoped by <see cref="AuthenticationExtensions.AddNdbJwt"/>, overriding the
/// <c>SystemActorAccessor</c> fallback registered by Core.
/// </summary>
/// <remarks>
/// Claims read:
/// <list type="bullet">
/// <item><see cref="ClaimTypes.Name"/> → <c>Actor</c> (username / display name)</item>
/// <item><see cref="ClaimTypes.NameIdentifier"/> → <c>ActorId</c> (user ID)</item>
/// <item><see cref="ClaimTypes.Role"/> → <c>Role</c> (first role claim found)</item>
/// </list>
/// Falls back to <see cref="AuditActor.System"/> when the user is not authenticated or <c>HttpContext</c> is null.
/// </remarks>
internal sealed class HttpContextActorAccessor : IActorAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextActorAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor
            ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    /// <inheritdoc />
    public AuditActor GetCurrent()
    {
        var ctx = _httpContextAccessor.HttpContext;
        var user = ctx?.User;

        if (user?.Identity?.IsAuthenticated != true)
        {
            return AuditActor.System with { CorrelationId = CorrelationId.Value };
        }

        return new AuditActor
        {
            Actor = user.FindFirst(ClaimTypes.Name)?.Value ?? user.Identity.Name,
            ActorId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value,
            Role = user.FindFirst(ClaimTypes.Role)?.Value,
            CorrelationId = CorrelationId.Value
        };
    }
}
