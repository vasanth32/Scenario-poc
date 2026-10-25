using System.ComponentModel.DataAnnotations;
using Claims.Api.Authentication;
using Claims.Api.Models;
using Claims.Api.Services;
using Claims.Api.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Claims.Api.Controllers;

[ApiController]
[Route("api/claims")]
[Produces("application/json")]
public class ClaimsController(IClaimService claimService) : ControllerBase
{
    [HttpGet(Name = "ListClaims")]
    [ProducesResponseType<IReadOnlyList<ClaimDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<ClaimDto>>> List(
        [Range(1, 100_000)] int page = 1,
        [Range(1, 100)] int pageSize = 50,
        CancellationToken ct = default) =>
        Ok(await claimService.ListAsync(page, pageSize, ct));

    [HttpGet("{id}", Name = "GetClaimById")]
    [ProducesResponseType<ClaimDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClaimDto>> GetById([NotEmptyGuid] Guid id, CancellationToken ct)
    {
        var claim = await claimService.GetByIdAsync(id, ct);
        return claim is null ? ClaimNotFound(id) : claim;
    }

    [HttpPost(Name = "CreateClaim")]
    [Authorize(Policy = AuthorizationPolicies.CustomerOnly)]
    [ProducesResponseType<ClaimDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<ClaimDto>> Create(CreateClaimRequest request, CancellationToken ct)
    {
        var result = await claimService.CreateAsync(request, ct);

        return result.Error switch
        {
            null => CreatedAtRoute("GetClaimById", new { id = result.Claim!.Id }, result.Claim),
            CreateClaimError.PolicyNotFound => Problem(StatusCodes.Status422UnprocessableEntity,
                "Policy not found", $"Policy {request.PolicyId} does not exist."),
            CreateClaimError.PolicyInactive => Problem(StatusCodes.Status422UnprocessableEntity,
                "Policy is not active", $"Policy {request.PolicyId} is not active."),
            _ => Problem(StatusCodes.Status503ServiceUnavailable,
                "Policy service unavailable", "Unable to verify the policy right now. Please retry later."),
        };
    }

    [HttpPut("{id}", Name = "UpdateClaim")]
    [Authorize(Policy = AuthorizationPolicies.AdjusterOnly)]
    [ProducesResponseType<ClaimDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClaimDto>> Update([NotEmptyGuid] Guid id, UpdateClaimRequest request, CancellationToken ct)
    {
        var claim = await claimService.UpdateAsync(id, request, ct);
        return claim is null ? ClaimNotFound(id) : claim;
    }

    [HttpPatch("{id}/approve", Name = "ApproveClaim")]
    [Authorize(Policy = AuthorizationPolicies.AdjusterOnly)]
    [ProducesResponseType<ClaimDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClaimDto>> Approve([NotEmptyGuid] Guid id, CancellationToken ct)
    {
        var claim = await claimService.ApproveAsync(id, ct);
        return claim is null ? ClaimNotFound(id) : claim;
    }

    [HttpDelete("{id}", Name = "DeleteClaim")]
    [Authorize(Policy = AuthorizationPolicies.AdjusterOnly)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([NotEmptyGuid] Guid id, CancellationToken ct) =>
        await claimService.DeleteAsync(id, ct) ? NoContent() : ClaimNotFound(id);

    private NotFoundObjectResult ClaimNotFound(Guid id) =>
        NotFound(new ProblemDetails { Title = "Claim not found", Detail = $"No claim with id {id}." });

    private ObjectResult Problem(int status, string title, string detail) =>
        base.Problem(statusCode: status, title: title, detail: detail);
}
