using Microsoft.EntityFrameworkCore;
using APMoodle.Data;
using APMoodle.Services;
using APMoodle.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddRazorPages();
builder.Services.AddControllers();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register email service
builder.Services.AddScoped<IEmailService, EmailService>();
// builder.Services.AddScoped<IStudentService, StudentService>();
// builder.Services.AddScoped<IQuizService, QuizService>();
// builder.Services.AddScoped<IAuthService, AuthService>();

// Session import
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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
