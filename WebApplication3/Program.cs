using WebApplication3.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var localCallingGuideBaseUrl = builder.Configuration["LocalCallingGuide:BaseUrl"]
    ?? throw new InvalidOperationException("LocalCallingGuide:BaseUrl is not configured.");

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddHttpClient<ILocalCallingGuideService, LocalCallingGuideService>(httpClient =>
{
    httpClient.BaseAddress = new Uri(localCallingGuideBaseUrl);
    httpClient.Timeout = TimeSpan.FromSeconds(10);
});

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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();
app.Run();
