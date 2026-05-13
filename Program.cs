using Microsoft.EntityFrameworkCore;
using SkillForge.Data;
using SkillForge.Interfaces.Auth;
using SkillForge.Interfaces.Courses;
using SkillForge.Interfaces.Instructors;
using SkillForge.Interfaces.Payments;
using SkillForge.Interfaces.Students;
using SkillForge.Interfaces.Common;
using SkillForge.Services.Auth;
using SkillForge.Services.Courses;
using SkillForge.Services.Instructors;
using SkillForge.Services.Payments;
using SkillForge.Services.Students;
using SkillForge.Services.Common;
using SkillForge.Services;
using Microsoft.AspNetCore.Authentication.Google;

var builder = WebApplication.CreateBuilder(args);

// Load optional secret configuration
var secretConfigPath = Path.Combine(builder.Environment.ContentRootPath, "appsettings.secret.json");
if (File.Exists(secretConfigPath))
{
    builder.Configuration.AddJsonFile(secretConfigPath, optional: true, reloadOnChange: false);
}
    
// Add services to the container.
builder.Services.AddControllersWithViews();

// Register Common Services
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IMediaService, MediaService>();

// Register Auth Services
builder.Services.AddScoped<IOtpService, OtpService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Register Course Services
builder.Services.AddScoped<ICourseQueryService, CourseQueryService>();
builder.Services.AddScoped<ICourseContentService, CourseContentService>();
builder.Services.AddScoped<ICourseManagementService, CourseManagementService>();
builder.Services.AddScoped<ICourseProgressService, CourseProgressService>();

// Register Student & Instructor Services
builder.Services.AddScoped<IStudentActivityService, StudentActivityService>();
builder.Services.AddScoped<IInstructorService, InstructorService>();

// Register Enrollment Service
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();

// Register Database
builder.Services.AddDbContext<SkillForgeDbContext>(options => 
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "Cookies";
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
.AddCookie("Cookies", options =>
{
    options.LoginPath = "/User/Auth/StudentLogin";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(10);
    options.SlidingExpiration = true;
})
.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["GoogleAuth:ClientId"];
    options.ClientSecret = builder.Configuration["GoogleAuth:ClientSecret"];
});


// Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(24);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
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
