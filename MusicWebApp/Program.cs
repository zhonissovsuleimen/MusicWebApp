using MusicWebApp.Services;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddSingleton<LikeGenerator>();
builder.Services.AddSingleton<ISongGenerator, SongGeneratorEn>();
builder.Services.AddSingleton<ISongGenerator, SongGeneratorRu>();
builder.Services.AddSingleton<CoverArtGenerator>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapGet("/api/song", (IEnumerable<ISongGenerator> gens, string locale, int start = 0, int end = 10, ulong seed = 123) =>
{
    if (start < 0 || end <= start)
    {
        return Results.BadRequest("Invalid range");
    }

    var gen = gens.FirstOrDefault(g => string.Equals(g.Locale(), locale, StringComparison.OrdinalIgnoreCase));
    if (gen is null)
    {
        return Results.NotFound($"Unsupported locale '{locale}'.");
    }

    var items = gen.Generate(start..end, seed);
    return Results.Json(items, options: (JsonSerializerOptions?)null, contentType: "application/json; charset=utf-8");
});

app.MapGet("/api/like", (LikeGenerator gen, int start = 0, int end = 10, double input = 0.5) =>
{
    if (start < 0 || end <= start)
    {
        return Results.BadRequest("Invalid range");
    }

    if (input < 0.0 || input > 10.0)
    {
        return Results.BadRequest("Input must be between 0.0 and 10.0");
    }

    var items = gen.Generate(start..end, input);
    return Results.Json(items, options: (JsonSerializerOptions?)null, contentType: "application/json; charset=utf-8");
});

app.MapGet("/api/cover", (CoverArtGenerator gen, string title, string artist, ulong seed = 123) =>
{
    if (string.IsNullOrWhiteSpace(title) && string.IsNullOrWhiteSpace(artist))
        return Results.BadRequest("Title and artist must be provided");

    var png = gen.Generate(title, artist, seed);
    return Results.File(png, "image/png");
});

app.MapGet("/api/sound", (ulong seed, int id) =>
{
    if (id < 0)
    {
        return Results.BadRequest("Invalid id");
    }

    var wav = AudioGenerator.Generate(seed, id);
    return Results.File(wav, "audio/wav");
});

app.MapRazorPages();

app.Run();
