
using EmployeeManagement.MVC.Handlers;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Register MVC
builder.Services.AddControllersWithViews();

// Register HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Register JWT Authorization Handler
builder.Services.AddTransient<JwtAuthorizationHandler>();

var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"]
    ?? throw new InvalidOperationException("API base URL is missing.");
if (!Uri.TryCreate(apiBaseUrl, UriKind.Absolute, out var apiBaseUri) ||
    (builder.Environment.IsProduction() && apiBaseUri.Scheme != Uri.UriSchemeHttps))
{
    throw new InvalidOperationException("API base URL must be absolute and use HTTPS in production.");
}
if (builder.Environment.IsProduction() &&
    string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("ApiSettings__BaseUrl")))
{
    throw new InvalidOperationException("Set ApiSettings__BaseUrl in secure production configuration.");
}

// Configure API HttpClient
builder.Services
    .AddHttpClient("EmployeeManagementAPI", client =>
    {
        client.BaseAddress = apiBaseUri;
    })
    .AddHttpMessageHandler<JwtAuthorizationHandler>();

// Cookie Authentication
builder.Services
    .AddAuthentication(
        CookieAuthenticationDefaults.AuthenticationScheme
    )
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";

        options.Cookie.Name = builder.Environment.IsDevelopment()
            ? "EmployeeManagement.Auth"
            : "__Host-EmployeeManagement.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.Path = "/";
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy =
            builder.Environment.IsDevelopment()
                ? CookieSecurePolicy.SameAsRequest
                : CookieSecurePolicy.Always;

        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });
    
builder.Services.AddAuthorization();

var app = builder.Build();

// Configure HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Employees}/{action=Index}/{id?}"
).WithStaticAssets();

app.Run();
