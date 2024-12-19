using HttpProcessor;

namespace TestSolution.Client;

internal class Program
{

    private HttpProcessorSettings PROCESSOR_SETTINGS = new()
    {
        BaseUrl = "https://localhost:5001",
        Timeout = 10000,
        RetryAfterNonSuccessfulStatusCodeReceived = true,
        RetryCount = 3,
        TimeBetweenRetries = 1000
    };

    private HttpProcessor.HttpProcessor? _httpProcessor;

    static void Main(string[] args)
    {
        while(true)
        {
            Console.WriteLine("Choose request method:\n" +
                "1: GET\n" +
                "2: PUT\n" +
                "3: PATCH\n" +
                "4: POST\n" +
                "5: HEAD\n" +
                "0: All methods");
            
            var read = Console.ReadLine();

            if (!int.TryParse(read, out int intRead))
                continue;

            switch (intRead)
            {
                case 1:
                    Console.WriteLine("GET request:");

                    break;
                case 2:
                    Console.WriteLine("PUT request:");

                    break;
                case 3:
                    Console.WriteLine("PATCH request:");

                    break;
                case 4:
                    Console.WriteLine("POST request:");

                    break;
                case 5:
                    Console.WriteLine("HEAD request:");

                    break;
                case 0:
                    Console.WriteLine("All methods:");

                    break;
                default:
                    Console.WriteLine("Invalid input");
                    break;
            }


        }
    }

    private void InitializeClient()
    {
        _httpProcessor = new HttpProcessor.HttpProcessor(PROCESSOR_SETTINGS)
        {
            AuthenticateMethod = async (processor, client, logger) =>
            {

                return true;
            }
        };
        
    }

    private void PerformRequest()
    {
        
    }

}
