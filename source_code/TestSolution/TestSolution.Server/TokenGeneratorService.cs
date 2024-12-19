using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace TestSolution.Server;

public class TokenGeneratorService(ApplicationConfiguration applicationConfiguration)
{

    /// <summary>
    /// Generates JWT Token with claims passed through argument.
    /// </summary>
    /// <param name="claims"></param>
    /// <returns>Tuple of string and DateTime. String - token in string form. DateTime - expiration time stamp.</returns>
    public (string, DateTime) GenerateJwtToken(Claim[] claims)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(applicationConfiguration.JwtKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expirationDateTime = DateTime.Now.AddMinutes(applicationConfiguration.JwtExpiryInMinutes);

        var token = new JwtSecurityToken(
            issuer: applicationConfiguration.JwtIssuer,
            audience: applicationConfiguration.JwtAudience,
            claims: claims,
            expires: expirationDateTime,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expirationDateTime);
    }

}
