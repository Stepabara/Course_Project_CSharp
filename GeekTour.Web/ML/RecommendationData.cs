using Microsoft.ML.Data;

namespace GeekTour.Web.ML;

public class RecommendationData
{
    [LoadColumn(0)] public float UserId { get; set; }
    [LoadColumn(1)] public float LocationId { get; set; }
    [LoadColumn(2)] public float Rating { get; set; }
}

public class RecommendationPrediction
{
    public float Score { get; set; }
}
