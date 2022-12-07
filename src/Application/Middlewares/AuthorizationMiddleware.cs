using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Midas.Services;

namespace Application.Middlewares;

public class AuthorizationMiddleware
{
    private readonly RequestDelegate _next;

    public AuthorizationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IUserClient userClient)
    {
        var isAuthorized = await CheckAuthorization(context, userClient).ConfigureAwait(false);

        if (!isAuthorized) return;

        try { await _next(context).ConfigureAwait(false); }
        catch (Exception ex) { await HandleExceptionAsync(context, ex).ConfigureAwait(false); }
    }

    private async Task<bool> CheckAuthorization(HttpContext context, IUserClient userClient)
    {
        var authHeader = context.Request.Headers["Authorization"].ToString();

        if (string.IsNullOrWhiteSpace(authHeader))
        {
            return false;
        }

        var token = authHeader.Replace("Bearer ", "");
        var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(token);
        var email = jwtToken.Claims.First(x => x.Type == ClaimTypes.Email)?.Value;
        var user = await userClient.GetUserByEmailAsync(email).ConfigureAwait(false);

        return user is not null;
    }
    
    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";
        
        await context.Response.WriteAsync(ex.Message + "\n\n" + ex.StackTrace);
    }
}