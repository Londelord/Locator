using Locator.Application.Models;
using SharpKml.Base;
using SharpKml.Dom;

namespace Locator.Application.Services;

public class KmlService
{
    private readonly List<Field> _fields = [];
    public IReadOnlyList<Field> Fields => _fields;
    
    public void Load(string fieldsPath, string centroidsPath)
    {
        var centroids = ParseCentroids(centroidsPath);
        var fields = ParseFields(fieldsPath);

        foreach (var field in fields)
        {
            if (centroids.TryGetValue(field.Name, out var centroid))
                field.Location.Center = centroid;
        }

        _fields.AddRange(fields);
    }
    
    private Dictionary<string, double[]> ParseCentroids(string filePath)
    {
        var parser = new Parser();
        parser.ParseString(File.ReadAllText(filePath), false);
        var kml = parser.Root;
        
        var result = new Dictionary<string, double[]>();

        if (kml is not Kml { Feature: Document doc }) return result;
        
        foreach (var feature in doc.Features)
        {
            if (feature is not Folder { Name: "centroids" }) continue;
            
            foreach (var placemark in GetAllPlacemarks(feature))
            {
                if (placemark.Geometry is Point { Coordinate: not null } point && !string.IsNullOrEmpty(placemark.Name))
                {
                    result[placemark.Name] = [point.Coordinate.Longitude, point.Coordinate.Latitude];
                }
            }
        }

        return result;
    }
    
    private List<Field> ParseFields(string filePath)
    {
        var parser = new Parser();
        parser.ParseString(File.ReadAllText(filePath), false);
        var kml = parser.Root;
        
        var result = new List<Field>();

        if (kml is not Kml { Feature: Document doc }) return result;
        
        foreach (var feature in doc.Features)
        {
            if (feature is not Folder { Name: "fields" }) continue;
            
            foreach (var placemark in GetAllPlacemarks(feature))
            {
                if (placemark.Geometry is not Polygon polygon) continue;

                var field = new Field
                {
                    Name = placemark.Name
                };

                if (placemark.ExtendedData != null)
                {
                    foreach (var schemaData in placemark.ExtendedData.SchemaData)
                    {
                        foreach (var data in schemaData.SimpleData)
                        {
                            if (data.Name.Equals("fid", StringComparison.OrdinalIgnoreCase) &&
                                int.TryParse(data.Text, out var id))
                            {
                                field.Id = id;
                            }
                            else if (data.Name.Equals("size", StringComparison.OrdinalIgnoreCase) &&
                                     double.TryParse(data.Text, out var size))
                            {
                                field.Size = size;
                            }
                        }
                    }
                }

                if (polygon.OuterBoundary?.LinearRing != null)
                {
                    foreach (var coord in polygon.OuterBoundary.LinearRing.Coordinates)
                    {
                        field.Location.Polygon.Add([coord.Longitude,  coord.Latitude]);
                    }
                }

                result.Add(field);
            }
        }

        return result;
    }
    
    private IEnumerable<Placemark> GetAllPlacemarks(Feature feature)
    {
        switch (feature)
        {
            case Placemark placemark:
                yield return placemark;
                break;
            case Container container:
            {
                foreach (var child in container.Features)
                {
                    foreach (var inner in GetAllPlacemarks(child))
                    {
                        yield return inner;
                    }
                }

                break;
            }
        }
    }
}