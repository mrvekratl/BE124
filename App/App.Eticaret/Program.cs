using App.Data.Infrastructure;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Cookie Authentication'ı ekliyoruz
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login"; // Giriş yapmayan kullanıcıları yönlendirecek URL
        options.LogoutPath = "/logout"; // Çıkış yapan kullanıcıyı yönlendirecek URL
        options.AccessDeniedPath = "/access-denied"; // Yetkisiz erişim yapan kullanıcıyı yönlendirecek URL
    });



builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer")));

// Register DataRepository for Dependency Injection
builder.Services.AddScoped(typeof(IDataRepository<>), typeof(DataRepository<>));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // Kimlik doğrulama middleware'ini kullanıyoruz
app.UseAuthorization();  // Yetkilendirme middleware'ini kullanıyoruz

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await context.Database.EnsureCreatedAsync();
}

app.Run();