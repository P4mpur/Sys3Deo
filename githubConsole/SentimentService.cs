using VaderSharp2;

public class SentimentService
{
    private readonly SentimentIntensityAnalyzer _analyzer;

    public SentimentService()
    {
        _analyzer = new SentimentIntensityAnalyzer();
    }

    public SentimentAnalysisResults AnalyzeSentiment(string text)
    {
        return _analyzer.PolarityScores(text);
    }
}
