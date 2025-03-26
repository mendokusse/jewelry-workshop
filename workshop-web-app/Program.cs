using Microsoft.AspNetCore.Authentication.Cookies;
using workshop_web_app.DataAccess;
using workshop_web_app.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";          // путь для входа
        options.LogoutPath = "/Account/Logout";          // путь для выхода
        options.AccessDeniedPath = "/Account/AccessDenied"; // опционально
    });

// Регистрация необходимых сервисов
builder.Services.AddTransient<PostgresDataAccess>();
builder.Services.AddScoped<MaterialRepository>();
builder.Services.AddScoped<OrderRepository>();
builder.Services.AddScoped<ProductTypeRepository>();
builder.Services.AddScoped<RoleRepository>();
builder.Services.AddScoped<UnitRepository>();
builder.Services.AddScoped<UserRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();