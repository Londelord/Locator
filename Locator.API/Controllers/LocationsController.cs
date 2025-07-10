using Locator.API.Contracts;
using Locator.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Locator.API.Controllers;

[ApiController]
[Route("[controller]")]
public class LocationsController : ControllerBase
{
    private readonly KmlService _kmlService;
    private readonly GeoService _geoService;

    public LocationsController(KmlService kmlService, GeoService geoService)
    {
        _kmlService = kmlService;
        _geoService = geoService;
    }

    [HttpGet]
    public IActionResult GetAllFields()
    {
        var revertedFields = _kmlService.Fields.Select(f => f.RevertLanLon()).ToList();
        return Ok(revertedFields);
    }

    [HttpGet("size")]
    public IActionResult GetSize([FromQuery] int fieldId)
    {
        var testingField = _kmlService.Fields.SingleOrDefault(f => f.Id == fieldId);

        if (testingField == null) return NotFound("Field not found");
        
        return Ok(testingField.Size);
    }

    [HttpGet("distance")]
    public IActionResult GetDistance([FromQuery] GetDistanceFromPointRequest request)
    {
        var testingField = _kmlService.Fields.SingleOrDefault(f => f.Id == request.FieldId);
        
        if (testingField == null) return NotFound("Field not found");

        var distance = _geoService.GetDistance([testingField.Location.Center[0], testingField.Location.Center[1]],
            [request.Longitude, request.Latitude]);
        
        return Ok(distance);
    }

    [HttpGet("field")]
    public IActionResult GetCorrespondingField([FromQuery] GetCorrespondingFieldRequest request)
    {
        var field = _geoService.FindFieldContainingPoint(_kmlService.Fields, [request.Longitude, request.Latitude]);
        
        return field == null ? Ok("false") : Ok(new GetCorrespondingFieldResponse(field.Id, field.Name));
    }
}