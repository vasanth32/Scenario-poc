using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Claims.Api.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Claims.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IOptions<JwtOptions> options, IWebHostEnvironment environment) : ControllerBase
{
    private static readonly HashSet<string> AllowedRoles = ["Adjuster", "Customer"];

    [AllowAnonymous]
    [HttpPost("token")]
    [ProducesResponseType<TokenResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<TokenResponse> IssueToken(TokenRequest request)
    {
        if (!environment.IsDevelopment()) return NotFound();
        if (!AllowedRoles.Contains(request.Role))
        {
            ModelState.AddModelError(nameof(request.Role), "Role must be Adjuster or Customer.");
            return ValidationProblem(ModelState);
        }

        var jwt = options.Value;
        var expiresAt = DateTime.UtcNow.AddMinutes(jwt.ExpiryMinutes);
        var token = new JwtSecurityToken(
            issuer: jwt.Issuer,
            audience: jwt.Audience,
            claims:
            [
                new Claim(JwtRegisteredClaimNames.Sub, request.UserName),
                new Claim(ClaimTypes.Name, request.UserName),
                new Claim(ClaimTypes.Role, request.Role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            ],
            expires: expiresAt,
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
                SecurityAlgorithms.HmacSha256));

        return Ok(new TokenResponse(new JwtSecurityTokenHandler().WriteToken(token), expiresAt, request.Role));
    }

    [HttpGet("me")]
    [ProducesResponseType<CurrentUserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<CurrentUserResponse> GetCurrentUser()
    {
        var subject = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("The authenticated token has no subject claim.");
        var roles = User.FindAll(ClaimTypes.Role).Select(claim => claim.Value).ToArray();

        return Ok(new CurrentUserResponse(subject, User.Identity?.Name, roles));
    }
}

public sealed record TokenRequest(
    [Required, StringLength(100, MinimumLength = 1)] string UserName,
    [Required] string Role);

public sealed record TokenResponse(string AccessToken, DateTime ExpiresAtUtc, string Role);

public sealed record CurrentUserResponse(string Subject, string? Name, IReadOnlyList<string> Roles);