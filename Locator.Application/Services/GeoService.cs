using Locator.Application.Models;
using NetTopologySuite.Algorithm.Locate;
using NetTopologySuite.Geometries;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;
using Location = NetTopologySuite.Geometries.Location;

namespace Locator.Application.Services;

public class GeoService
{
    private static readonly GeographicCoordinateSystem Wgs84 = GeographicCoordinateSystem.WGS84;
    
    private ProjectedCoordinateSystem GetUtmZone(double longitude, double latitude)
    {
        var zone = (int)((longitude + 180) / 6) + 1;
        var isNorth = latitude >= 0;
        return ProjectedCoordinateSystem.WGS84_UTM(zone, isNorth);
    }
    
    public double GetDistance(double[] point1, double[] point2)
    {
        var utm = GetUtmZone(point1[0], point1[1]);

        var transform = new CoordinateTransformationFactory()
            .CreateFromCoordinateSystems(Wgs84, utm)
            .MathTransform;

        var p1 = transform.Transform(point1);
        var p2 = transform.Transform(point2);

        var dx = p2[0] - p1[0];
        var dy = p2[1] - p1[1];

        return Math.Sqrt(dx * dx + dy * dy);
    }
    
    public Field? FindFieldContainingPoint(IReadOnlyCollection<Field> fields, double[] point)
    {
        var geomFactory = new GeometryFactory();
        var testPoint = geomFactory.CreatePoint(new Coordinate(point[0], point[1]));

        foreach (var field in fields)
        {
            if (field.Location.Polygon.Count < 3)
                continue;

            var coordinates = field.Location.Polygon
                .Select(p => new Coordinate(p[0], p[1]))
                .ToList();

            var ring = geomFactory.CreateLinearRing(coordinates.ToArray());
            var polygon = geomFactory.CreatePolygon(ring);

            var locator = new SimplePointInAreaLocator(polygon);
            var result = locator.Locate(testPoint.Coordinate);

            if (result != Location.Exterior)
                return field;
        }

        return null;
    }
}