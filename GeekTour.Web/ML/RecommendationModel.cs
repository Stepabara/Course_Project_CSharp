using Microsoft.ML;
using Microsoft.ML.Trainers;

namespace GeekTour.Web.ML;

public class RecommendationModel
{
    private readonly MLContext _mlContext;
    private ITransformer? _model;
    private PredictionEngine<RecommendationData, RecommendationPrediction>? _predictionEngine;

    public RecommendationModel()
    {
        _mlContext = new MLContext(seed: 42);
    }

    public void Train(List<RecommendationData> trainingData)
    {
        if (trainingData.Count < 10) return; // Need minimum data

        var dataView = _mlContext.Data.LoadFromEnumerable(trainingData);

        var pipeline = _mlContext.Recommendation().Trainers.MatrixFactorization(
            labelColumnName: "Rating",
            matrixColumnIndexColumnName: "UserId",
            matrixRowIndexColumnName: "LocationId",
            numberOfIterations: 20,
            approximationRank: 100);

        _model = pipeline.Fit(dataView);
        _predictionEngine = _mlContext.Model.CreatePredictionEngine<RecommendationData, RecommendationPrediction>(_model);
    }

    public float Predict(float userId, float locationId)
    {
        if (_predictionEngine == null) return 3.0f; // Default fallback

        var prediction = _predictionEngine.Predict(new RecommendationData
        {
            UserId = userId,
            LocationId = locationId
        });

        return Math.Clamp(prediction.Score, 1.0f, 5.0f);
    }

    public List<(int LocationId, float Score)> GetTopRecommendations(float userId, List<int> allLocationIds, int count = 5)
    {
        if (_predictionEngine == null) return new List<(int, float)>();

        var scores = new List<(int LocationId, float Score)>();
        foreach (var locId in allLocationIds)
        {
            var score = Predict(userId, locId);
            scores.Add((locId, score));
        }

        return scores.OrderByDescending(s => s.Score).Take(count).ToList();
    }
}
