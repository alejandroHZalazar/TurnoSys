using MediatR;
using Microsoft.AspNetCore.Mvc;
using TurnoSys.Application.Features.Auth.Commands.Login;

namespace TurnoSys.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController(ISender sender) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginCommand command, CancellationToken ct)
    {
        var result = await sender.Send(command, ct);
        if (result.Failure)
            return Unauthorized(new { success = false, error = result.Error });

        return Ok(new { success = true, data = result.Value });
    }
}
