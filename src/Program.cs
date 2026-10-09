using Infrastructure.Database;
using MudBlazor.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;

using src.Components;
using ProjectMagpie.Database;
using ProjectMagpie.Services;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddUserSecrets<Program>();


var keysPath = builder.Configuration["DataProtection:KeysPath"];
if (!string.IsNullOrWhiteSpace(keysPath))
{
    builder.Services.AddDataProtection()
        .SetApplicationName("ProjectMagpie")
        .PersistKeysToFileSystem(new DirectoryInfo(keysPath));
}

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

builder.Services.AddScoped<IImageService, ImageService>();
// Add MudBlazor services
builder.Services.AddMudServices();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
var imagesFolder = builder.Configuration["ImageStorage:Path"];
if (string.IsNullOrWhiteSpace(imagesFolder))
    imagesFolder = Path.Combine(builder.Environment.WebRootPath, "ThingImages");
else if (!Path.IsPathRooted(imagesFolder))
    imagesFolder = Path.Combine(builder.Environment.ContentRootPath, imagesFolder);

Directory.CreateDirectory(imagesFolder);
builder.Services.AddSingleton(new ImageStorageOptions(imagesFolder, "/ThingImages"));

var app = builder.Build();

using (var db = app.Services
    .GetRequiredService<IDbContextFactory<ApplicationDbContext>>()
    .CreateDbContext())
{
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

app.UseHttpsRedirection();


app.UseAntiforgery();

// Directory.CreateDirectory(Path.Combine(app.Environment.WebRootPath, "ThingImages"));
app.UseStaticFiles();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(imagesFolder),
    RequestPath = "/ThingImages"
});

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
