using Microsoft.Extensions.Configuration; 
using Google.Cloud.AIPlatform.V1; 
using Google.Protobuf.WellKnownTypes; 

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

            _predictionServiceClient = new PredictionServiceClientBuilder
            {
                Endpoint = $"{_location}-aiplatform.googleapis.com"
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
