using Microsoft.Extensions.Configuration; 
using Google.Cloud.AIPlatform.V1; 
using Google.Protobuf.WellKnownTypes;
using Google.Apis.Auth.OAuth2; 
using Grpc.Auth; 
using System.IO; 

namespace FoodAI.Services
{
    public interface IGeminiAIService
    {
        Task<string> GenerateContentAsync(string prompt);
    }

    public class GeminiAIService : IGeminiAIService
    {
        private readonly PredictionServiceClient _predictionServiceClient;
        private readonly string _projectId;
        private readonly string _location;
        private readonly string _publisher;
        private readonly string _model;

        public GeminiAIService(IConfiguration configuration)
        {
            _projectId = configuration.GetValue<string>("GeminiAI:ProjectId");
            _location = configuration.GetValue<string>("GeminiAI:Location");
            _publisher = configuration.GetValue<string>("GeminiAI:Publisher");
            _model = configuration.GetValue<string>("GeminiAI:Model");

            GoogleCredential credential = null;
            var googleCredentialsJson = Environment.GetEnvironmentVariable("GOOGLE_CREDENTIALS_JSON");

            if (!string.IsNullOrEmpty(googleCredentialsJson))
            {
                Console.WriteLine("Found GOOGLE_CREDENTIALS_JSON environment variable. Attempting to load credentials from JSON string.");
                credential = GoogleCredential.FromJson(googleCredentialsJson)
                                             .CreateScoped(PredictionServiceClient.DefaultScopes);
            }
            else
            {
                Console.WriteLine("GOOGLE_CREDENTIALS_JSON environment variable not set. Attempting default credentials.");
                // Fallback to default credential discovery
                credential = GoogleCredential.GetApplicationDefault()
                                             .CreateScoped(PredictionServiceClient.DefaultScopes);
            }

            _predictionServiceClient = new PredictionServiceClientBuilder
            {
                Endpoint = $"{_location}-aiplatform.googleapis.com",
                ChannelCredentials = credential.ToChannelCredentials() // Explicitly set credentials
            }.Build();
        }

        public async Task<string> GenerateContentAsync(string prompt)
        {
            var endpoint = EndpointName.FromProjectLocationPublisherModel(_projectId, _location, _publisher, _model);

            var predictRequest = new PredictRequest
            {
                Endpoint = endpoint.ToString(),
                Instances =
                {
                    new Google.Protobuf.WellKnownTypes.Value
                    {
                        StructValue = new Struct
                        {
                            Fields =
                            {
                                { "prompt", Google.Protobuf.WellKnownTypes.Value.ForString(prompt) }
                            }
                        }
                    }
                }
            };

            var response = await _predictionServiceClient.PredictAsync(predictRequest);

            if (response.Predictions.Count == 0) return "No response from AI.";
            var prediction = response.Predictions.First();
            return prediction.StructValue.Fields["content"].StringValue;

        }
    }
}
