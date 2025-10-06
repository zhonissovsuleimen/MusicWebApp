using MusicWebApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddSingleton<ISongGenerator, SongGeneratorEn>();

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

app.MapGet("/api/{locale}/song", (IEnumerable<ISongGenerator> generators, string locale, int start = 0, int end = 10, ulong seed = 123) =>
{
    if (start < 0 || end <= start)
    {
        return Results.BadRequest("Invalid range");
    }

    var gen = generators.FirstOrDefault(g => string.Equals(g.Locale(), locale, StringComparison.OrdinalIgnoreCase));
    if (gen is null)
    {
        return Results.NotFound($"Unsupported locale '{locale}'.");
    }

    return Results.Ok(gen.Generate(start..end, seed));
});


app.MapRazorPages();

app.Run();
