using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TestSolution.Common;
using TestSolution.Common.DataModels;

namespace TestSolution.Server.Controllers;

/// <summary>
/// Authorized endpoint controller
/// </summary>
/// <param name="tokenGenerator"></param>
[Authorize]
[Route("/api/[controller]")]
public class AuthorizedEndpointController(TokenGeneratorService tokenGenerator) : EndpointController
{

    private const string CLIENT_CLAIM_TYPE = "userId";

    /// <summary>
    /// Get client token
    /// </summary>
    /// <param name="clientDto"></param>
    /// <returns></returns>
    [HttpPost("/token")]
    [AllowAnonymous]
    public IActionResult GetToken([FromBody] TokenRequest clientDto)
    {
        try
        {
            if (clientDto.Username == AuthorizationVariables.ClientUsername && clientDto.Password == AuthorizationVariables.ClientPassword)
            {
                var tokenAndExpiration = tokenGenerator.GenerateJwtToken([new Claim(CLIENT_CLAIM_TYPE, Guid.NewGuid().ToString())]);

                var tokenResponse = new TokenResponse
                {
                    AccessToken = tokenAndExpiration.Item1,
                    ExpirationLocalTime = tokenAndExpiration.Item2.ToString("F"),
                    TokenType = "Bearer"
                };

                return Ok(tokenResponse);
            }
            else return BadRequest();
        }
        catch(Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

}
