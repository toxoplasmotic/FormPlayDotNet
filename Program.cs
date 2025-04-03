using FormPlay.Data;
using FormPlay.Models;
using FormPlay.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure DB context with in-memory database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseInMemoryDatabase("FormPlayDb"));

// Register application services
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<CalendarService>();
builder.Services.AddScoped<PdfService>();
builder.Services.AddScoped<TpsReportService>();
builder.Services.AddScoped<UserService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Initialize the database with seed data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var dbContext = services.GetRequiredService<ApplicationDbContext>();
    DatabaseInitializer.Initialize(dbContext);
}

// Remove HTTPS redirection in Replit environment to avoid port binding issues
// app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Ensure the app listens on the correct IP and port for Replit
// Hard-code to port 5000 for Replit compatibility
app.Run("http://0.0.0.0:5000");
