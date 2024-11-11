using App.Data.Infrastructure;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Cookie Authentication'� ekliyoruz
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login"; // Giri� yapmayan kullan�c�lar� y�nlendirecek URL
        options.LogoutPath = "/logout"; // ��k�� yapan kullan�c�y� y�nlendirecek URL
        options.AccessDeniedPath = "/access-denied"; // Yetkisiz eri�im yapan kullan�c�y� y�nlendirecek URL
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

app.UseAuthentication(); // Kimlik do�rulama middleware'ini kullan�yoruz
app.UseAuthorization();  // Yetkilendirme middleware'ini kullan�yoruz

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await context.Database.EnsureCreatedAsync();
}

app.Run();