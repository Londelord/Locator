using Locator.Application.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<KmlService>(sp =>
{
    var env = sp.GetRequiredService<IWebHostEnvironment>();
    
    var sourcesDir = Path.Combine(env.ContentRootPath, "Sources");
    var fieldsPath = Path.Combine(sourcesDir, "fields.kml");
    var centroidsPath = Path.Combine(sourcesDir, "centroids.kml");

    var service = new KmlService();
    service.Load(fieldsPath, centroidsPath);
    return service;
});

builder.Services.AddScoped<GeoService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();