using Microsoft.EntityFrameworkCore;
using System.IO;
using SkillForge.Services;
using SkillForge.Data;

var builder = WebApplication.CreateBuilder(args);

// Load optional secret configuration (not checked into source)
var secretConfigPath = Path.Combine(builder.Environment.ContentRootPath, "appsettings.secrect.json");
if (File.Exists(secretConfigPath))
{
    builder.Configuration.AddJsonFile(secretConfigPath, optional: true, reloadOnChange: false);
}

// Add services to the container.
builder.Services.AddControllersWithViews();

//Register EMail service
builder.Services.AddScoped<EmailService>();

//Register AUthService
builder.Services.AddScoped<AuthService>();

//Register Database
builder.Services.AddDbContext<SkillForgeDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//Register for use Cookie
builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options =>
    {
        options.LoginPath = "/User/Account/StudentLogin";

        //logout after 10 minute of non activity
        options.ExpireTimeSpan = TimeSpan.FromMinutes(10);
        options.SlidingExpiration = true;
    });

//register session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(24); //session dies after 1 day
    options.Cookie.HttpOnly = true; //js can not read cookie, save from js- xss attack
    options.Cookie.IsEssential = true;  //store cookire regardless user permision of browser behaviour

});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    
    app.UseHsts();
}


app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();


app.MapControllerRoute(
    name: "myArea",
    pattern: "{area}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
