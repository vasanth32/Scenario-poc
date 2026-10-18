using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Policy.Api.Models;
using Policy.Api.Services;
using Policy.Api.Validation;

namespace Policy.Api.Controllers;

[ApiController]
[Route("api/policies")]
[Produces("application/json")]
public class PoliciesController(IPolicyService policyService) : ControllerBase
{
    [HttpGet(Name = "ListPolicies")]
    [ProducesResponseType<IReadOnlyList<PolicyDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<PolicyDto>>> List(
        [Range(1, 100_000)] int page = 1,
        [Range(1, 100)] int pageSize = 50,
        CancellationToken ct = default) =>
        Ok(await policyService.ListAsync(page, pageSize, ct));

    [HttpGet("{id}", Name = "GetPolicyById")]
    [ProducesResponseType<PolicyDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PolicyDto>> GetById([NotEmptyGuid] Guid id, CancellationToken ct)
    {
        var policy = await policyService.GetByIdAsync(id, ct);
        return policy is null ? PolicyNotFound(id) : policy;
    }

    [HttpPost(Name = "CreatePolicy")]
    [ProducesResponseType<PolicyDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PolicyDto>> Create(CreatePolicyRequest request, CancellationToken ct)
    {
        var result = await policyService.CreateAsync(request, ct);
        return result.Error is null
            ? CreatedAtRoute("GetPolicyById", new { id = result.Policy!.Id }, result.Policy)
            : MapError(result.Error.Value, null);
    }

    [HttpPut("{id}", Name = "UpdatePolicy")]
    [ProducesResponseType<PolicyDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PolicyDto>> Update([NotEmptyGuid] Guid id, UpdatePolicyRequest request, CancellationToken ct)
    {
        var result = await policyService.UpdateAsync(id, request, ct);
        return result.Error is null ? result.Policy! : MapError(result.Error.Value, id);
    }

    [HttpDelete("{id}", Name = "DeletePolicy")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([NotEmptyGuid] Guid id, CancellationToken ct) =>
        await policyService.DeleteAsync(id, ct) ? NoContent() : PolicyNotFound(id);

    private ObjectResult MapError(PolicyWriteError error, Guid? id) => error switch
    {
        PolicyWriteError.DuplicatePolicyNumber => Problem(statusCode: StatusCodes.Status409Conflict,
            title: "Duplicate policy number", detail: "A policy with this policy number already exists."),
        _ => PolicyNotFound(id!.Value),
    };

    private NotFoundObjectResult PolicyNotFound(Guid id) =>
        NotFound(new ProblemDetails { Title = "Policy not found", Detail = $"No policy with id {id}." });
}
