using Microsoft.EntityFrameworkCore;
using APMoodle.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

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
            Console.WriteLine("✅ Database connected successfully!");
            
            // Display counts
            Console.WriteLine($"- Students: {await dbContext.Students.CountAsync()}");
            Console.WriteLine($"- Lecturers: {await dbContext.Lecturers.CountAsync()}");
            Console.WriteLine($"- Admins: {await dbContext.Admins.CountAsync()}");
            Console.WriteLine($"- Modules: {await dbContext.Modules.CountAsync()}");
            Console.WriteLine($"- Materials: {await dbContext.Materials.CountAsync()}");
            Console.WriteLine($"- Quizzes: {await dbContext.Quizzes.CountAsync()}");
            Console.WriteLine($"- Questions: {await dbContext.Questions.CountAsync()}");
            Console.WriteLine($"- Announcements: {await dbContext.Announcements.CountAsync()}");
        }
        else
        {
            Console.WriteLine("❌ Fail to connect to database!");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error: {ex}");
    }
}

app.Run();
