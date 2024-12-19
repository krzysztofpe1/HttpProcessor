namespace TestSolution.Common.DataModels;

public class TokenResponse
{
    public required string AccessToken { get; set; }
    public required string TokenType { get; set; }
    public required string ExpirationLocalTime { get; set; }
}
