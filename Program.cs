using Microsoft.EntityFrameworkCore;
using APMoodle.Data;
using APMoodle.Services;
using APMoodle.Services.Interfaces;
using BCrypt.Net;

var builder = WebApplication.CreateBuilder(args);

// Disable file watching to avoid inotify limit
builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);
builder.Configuration.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: false);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllers();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register services
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<ILecturerService, LecturerService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IModuleService, ModuleService>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
builder.Services.AddScoped<IMaterialService, MaterialService>();
builder.Services.AddScoped<IQuizService, QuizService>();
builder.Services.AddScoped<ISessionService, SessionService>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IAnnouncementService, AnnouncementService>();
builder.Services.AddMemoryCache();

// Session services
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build(); 


// Use session after app is created
app.UseSession();
app.UseStaticFiles();
app.MapControllers(); 

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    
    Console.WriteLine("Testing database connection...");
    
    try
    {
        // Test connection
        bool canConnect = await dbContext.Database.CanConnectAsync();
        
        if (canConnect)
        {
            Console.WriteLine("Database connected successfully!");
        }
        else
        {
            Console.WriteLine("Fail to connect to database!");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex}");
    }
}

app.Run();