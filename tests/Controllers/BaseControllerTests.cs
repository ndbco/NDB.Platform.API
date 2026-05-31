using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace NDB.Platform.Api.Tests.Controllers;

// Concrete testable subclass of BaseController
[Microsoft.AspNetCore.Mvc.ApiExplorerSettings(IgnoreApi = true)]
public sealed class TestableController : NDB.Platform.Api.Controllers.BaseController
{
    [Microsoft.AspNetCore.Mvc.NonAction]
    public string? GetCurrentUserId() => CurrentUserId;
    [Microsoft.AspNetCore.Mvc.NonAction]
    public string? GetCurrentUserName() => CurrentUserName;
    [Microsoft.AspNetCore.Mvc.NonAction]
    public string? GetCurrentUserRole() => CurrentUserRole;
}

public sealed class BaseControllerTests
{
    private static TestableController CreateController(ClaimsIdentity? identity = null)
    {
        var controller = new TestableController();
        var httpContext = new DefaultHttpContext();
        if (identity is not null)
            httpContext.User = new ClaimsPrincipal(identity);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
        return controller;
    }

    [Fact]
    public void CurrentUserId_WithNameIdentifierClaim_ShouldReturnId()
    {
        var identity = new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, "user-guid-123")],
            "Bearer");

        var controller = CreateController(identity);
        controller.GetCurrentUserId().Should().Be("user-guid-123");
    }

    [Fact]
    public void CurrentUserId_WithoutClaim_ShouldReturnNull()
    {
        var controller = CreateController();
        controller.GetCurrentUserId().Should().BeNull();
    }

    [Fact]
    public void CurrentUserName_WithNameClaim_ShouldReturnName()
    {
        var identity = new ClaimsIdentity(
            [new Claim(ClaimTypes.Name, "alice.johnson")],
            "Bearer");

        var controller = CreateController(identity);
        controller.GetCurrentUserName().Should().Be("alice.johnson");
    }

    [Fact]
    public void CurrentUserRole_WithRoleClaim_ShouldReturnRole()
    {
        var identity = new ClaimsIdentity(
            [new Claim(ClaimTypes.Role, "Admin")],
            "Bearer");

        var controller = CreateController(identity);
        controller.GetCurrentUserRole().Should().Be("Admin");
    }

    [Fact]
    public void CurrentUserRole_WithMultipleRoles_ShouldReturnFirst()
    {
        var identity = new ClaimsIdentity(
            [
                new Claim(ClaimTypes.Role, "Admin"),
                new Claim(ClaimTypes.Role, "Viewer")
            ],
            "Bearer");

        var controller = CreateController(identity);
        controller.GetCurrentUserRole().Should().Be("Admin");
    }

    [Fact]
    public void BaseController_ShouldNotHaveILoggerProperty()
    {
        var type = typeof(NDB.Platform.Api.Controllers.BaseController);
        var props = type.GetProperties(
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.Public |
            System.Reflection.BindingFlags.NonPublic);

        props.Should().NotContain(p =>
            p.PropertyType.IsAssignableTo(typeof(Microsoft.Extensions.Logging.ILogger)));
    }
}
