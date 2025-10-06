var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddSingleton<MusicWebApp.Services.SongGenerator>();

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

app.MapGet("/api/song", (MusicWebApp.Services.SongGenerator gen, int start = 0, int end = 10, ulong seed = 123) =>
{
    if (start < 0 || end <= start)
    {
        return Results.BadRequest("Invalid range");
    }

    return Results.Ok(gen.Generate(start..end, seed));
});


app.MapRazorPages();

app.Run();
