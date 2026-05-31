using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using NDB.Platform.Abstraction;
using System.Reflection;

namespace NDB.Platform.Api.Filters;

/// <summary>
/// Action filter that automatically converts <see cref="Result"/>, <see cref="Result{T}"/>,
/// <see cref="PagedResult{T}"/>, and <see cref="CollectionResult{T}"/> return values
/// into the standard <see cref="ApiResponse"/> JSON envelope.
/// </summary>
public sealed class ResultActionFilter : IAsyncActionFilter
{
    /// <inheritdoc />
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        var executed = await next();

        if (executed.Result is ObjectResult { Value: { } value })
        {
            var converted = ConvertResult(value);
            if (converted is not null)
            {
                executed.Result = converted;
            }
        }
    }

    private static IActionResult? ConvertResult(object value)
    {
        var type = value.GetType();

        // Most specific types are checked first
        // 1. PagedResult<T> — must be checked before CollectionResult<T> because PagedResult IS-A CollectionResult
        if (TryFindGenericBase(type, typeof(PagedResult<>), out var pagedT))
        {
            return InvokeToActionResultGeneric(value, pagedT,
                typeof(ResultExtensions).GetMethod(nameof(ResultExtensions.ToActionResult),
                    [typeof(PagedResult<>).MakeGenericType(pagedT)]));
        }

        // 2. CollectionResult<T>
        if (TryFindGenericBase(type, typeof(CollectionResult<>), out var collT))
        {
            return InvokeToActionResultGeneric(value, collT,
                typeof(ResultExtensions).GetMethod(nameof(ResultExtensions.ToActionResult),
                    [typeof(CollectionResult<>).MakeGenericType(collT)]));
        }

        // 3. Result<T> (existing)
        if (TryFindGenericBase(type, typeof(Result<>), out var resultT))
        {
            var methods = typeof(ResultExtensions)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(m => m.Name == nameof(ResultExtensions.ToActionResult)
                            && m.IsGenericMethod
                            && m.GetParameters().Length == 1
                            && m.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == typeof(Result<>))
                .FirstOrDefault();
            return (IActionResult?)methods?.MakeGenericMethod(resultT).Invoke(null, [value]);
        }

        // 4. Result non-generic (existing)
        if (value is Result r) return r.ToActionResult();

        return null;
    }

    /// <summary>
    /// Walks the type hierarchy to find the first generic base type that matches <paramref name="genericDefinition"/>.
    /// </summary>
    private static bool TryFindGenericBase(Type type, Type genericDefinition, out Type genericArg)
    {
        var current = type;
        while (current != null && current != typeof(object))
        {
            if (current.IsGenericType && current.GetGenericTypeDefinition() == genericDefinition)
            {
                genericArg = current.GetGenericArguments()[0];
                return true;
            }
            current = current.BaseType;
        }
        genericArg = typeof(object);
        return false;
    }

    private static IActionResult? InvokeToActionResultGeneric(object value, Type typeArg, MethodInfo? method)
    {
        var generic = method?.MakeGenericMethod(typeArg);
        return (IActionResult?)generic?.Invoke(null, [value]);
    }
}
