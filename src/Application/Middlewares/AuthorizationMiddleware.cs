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

    public async Task Invoke(HttpContext context, IAuthorizationClient authorizationClient)
    {
        var isAuthorized = await CheckAuthorization(context, authorizationClient).ConfigureAwait(false);

        if (isAuthorized)
        {
            await _next(context);
        }
    }

    public async Task<bool> CheckAuthorization(HttpContext context, IAuthorizationClient authorizationClient)
    {
        var authHeader = context.Request.Headers["Authorization"].ToString();

        if (string.IsNullOrWhiteSpace(authHeader))
        {
            return false;
        }

        var token = authHeader.Replace("Bearer ", "");
        var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(token);
        var email = jwtToken.Claims.First(x => x.Type == ClaimTypes.Email)?.Value;
        var user = await authorizationClient.GetUserByEmailAsync(email).ConfigureAwait(false);

        return user is not null;
    }
}