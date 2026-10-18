using Microsoft.AspNetCore.Mvc;
using Policy.Api.Chaos;

namespace Policy.Api.Controllers;

// Registered only in Development (see Program.cs). Never expose this in a real environment.
[ApiController]
[Route("api/chaos")]
public sealed class ChaosController(ChaosState state, IWebHostEnvironment env, ILogger<ChaosController> logger) : ControllerBase
{
    private bool Enabled => env.IsDevelopment();

    /// <summary>Shows the current fault-injection settings.</summary>
    [HttpGet]
    [ProducesResponseType<ChaosSettings>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<ChaosSettings> Get() => Enabled ? state.Current : NotFound();

    /// <summary>Sets latency (ms), random-500 rate (0..1) and/or downtime for all policy endpoints.</summary>
    [HttpPut]
    [ProducesResponseType<ChaosSettings>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<ChaosSettings> Set([FromBody] ChaosSettings settings)
    {
        if (!Enabled) return NotFound();

        state.Set(settings);
        logger.LogWarning("Chaos settings changed: {@Settings}", settings);
        return settings;
    }

    /// <summary>Turns all fault injection off.</summary>
    [HttpDelete]
    [ProducesResponseType<ChaosSettings>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<ChaosSettings> Reset()
    {
        if (!Enabled) return NotFound();

        state.Reset();
        logger.LogWarning("Chaos settings reset");
        return state.Current;
    }
}
