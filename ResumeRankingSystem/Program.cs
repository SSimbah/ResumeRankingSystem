using Domain.DataAccess;
using Microsoft.EntityFrameworkCore;
using ResumeRankingSystem.Services;

var builder = WebApplication.CreateBuilder(args);
//var builder = WebApplication.CreateBuilder(new WebApplicationOptions
//{
//    Args = args,
//    ApplicationName = typeof(Program).Assembly.GetName().Name,
//    ContentRootPath = AppContext.BaseDirectory,
//    WebRootPath = "wwwroot"
//});

////builder.WebHost.UseUrls("http://0.0.0.0:7178", "https://0.0.0.0:7178");
//builder.WebHost.UseUrls("http://0.0.0.0:7179", "https://0.0.0.0:7180");

// Register DbContext with the DI container
builder.Services.AddDbContext<DatabaseDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DatabaseDbContext")));


// Add session services
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Adjust as needed
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddHttpClient<PreprocessingHelper>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5000/");
});
builder.Services.AddScoped<ResumeRanker>(provider =>
{
    var httpClient = provider.GetRequiredService<HttpClient>();
    return new ResumeRanker(httpClient);
});

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Add middleware to use sessions
app.UseSession();

// Existing middleware
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
