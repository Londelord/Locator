namespace Locator.Application.Models;

public class Field
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public double Size { get; set; }
    public Location Location { get; set; } = new();
    
    public Field RevertLanLon()
    {
        var revertedField = new Field
        {
            Id = Id,
            Name = Name,
            Size = Size,
            Location = new Location
            {
                Center = [Location.Center[1], Location.Center[0]],
                Polygon = []
            }
        };

        foreach (var point in Location.Polygon)
            revertedField.Location.Polygon.Add([point[1], point[0]]);
        
        return revertedField;
    }
}