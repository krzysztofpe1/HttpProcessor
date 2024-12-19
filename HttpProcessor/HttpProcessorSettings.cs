namespace HttpProcessor
{

    public class HttpProcessorSettings
    {
        
        public string BaseUrl { get; set; }
        
        public int Timeout { get; set; }
        
        public int RetryCount { get; set; }
        
        public int TimeBetweenRetries { get; set; }

        public bool RetryAfterNonSuccessfulStatusCodeReceived { get; set; }

    }

}