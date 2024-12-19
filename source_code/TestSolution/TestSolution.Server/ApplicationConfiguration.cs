namespace TestSolution.Server;

public class ApplicationConfiguration(IConfiguration configuration)
{
    
    public string JwtIssuer => configuration.GetValue<string>("Jwt:Issuer")!;
    
    public string JwtAudience => configuration.GetValue<string>("Jwt:Audience")!;
    
    public string JwtKey => configuration.GetValue<string>("Jwt:Key")!;

    public int JwtExpiryInMinutes => configuration.GetValue<int>("Jwt:ExpiryInMinutes");

}