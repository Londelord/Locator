namespace Locator.Application.Models;

public class Location
{
    public double[] Center { get; set; }
    public List<double[]> Polygon { get; set; } = [];
}