namespace TimescaleWebAPI.Application.Extensions;

public static class StatisticsExtensions
{
    public static double CalculateMedian(this IEnumerable<double> values)
    {
        var sortedValues = values.OrderBy(v => v).ToList();
        var count = sortedValues.Count;
        
        if (count == 0) return 0;
        
        if (count % 2 == 0)
        {
            var middle1 = sortedValues[count / 2 - 1];
            var middle2 = sortedValues[count / 2];
            return (middle1 + middle2) / 2.0;
        }
        
        return sortedValues[count / 2];
    }
}