using Microsoft.EntityFrameworkCore;
using SchoolManagement.Data;

var builder = WebApplication.CreateBuilder(args);

// Configure SQLite database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=SchoolManagement.db"));

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Ensure database created and seed data
using (var scope = app.Services.CreateScope())
{
    var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    ctx.Database.EnsureCreated();
    await SeedData.EnsureSeedDataAsync(ctx);
}

// Create and seed a second database file with the same schema and sample data
{
    var secondDbPath = "SchoolManagement_Second.db";
    var optionsBuilder = new DbContextOptionsBuilder<SchoolManagement.Data.AppDbContext>();
    optionsBuilder.UseSqlite($"Data Source={secondDbPath}");

    using (var ctx2 = new SchoolManagement.Data.AppDbContext(optionsBuilder.Options))
    {
        ctx2.Database.EnsureCreated();
        await SchoolManagement.Data.SeedData.EnsureSeedDataAsync(ctx2);
    }
}

// Update student emails from @example.com to @gmail.com in both databases
using (var scope = app.Services.CreateScope())
{
    var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var toUpdate = await ctx.Students.Where(s => s.Email.EndsWith("@example.com")).ToListAsync();
    if (toUpdate.Any())
    {
        foreach (var s in toUpdate)
        {
            s.Email = s.Email.Replace("@example.com", "@gmail.com");
        }
        await ctx.SaveChangesAsync();
        Console.WriteLine($"Updated {toUpdate.Count} emails in primary DB to @gmail.com");
    }

    // second DB
    var secondOptions = new DbContextOptionsBuilder<AppDbContext>();
    secondOptions.UseSqlite("Data Source=SchoolManagement_Second.db");
    using (var ctx2 = new AppDbContext(secondOptions.Options))
    {
        var toUpdate2 = await ctx2.Students.Where(s => s.Email.EndsWith("@example.com")).ToListAsync();
        if (toUpdate2.Any())
        {
            foreach (var s in toUpdate2)
            {
                s.Email = s.Email.Replace("@example.com", "@gmail.com");
            }
            await ctx2.SaveChangesAsync();
            Console.WriteLine($"Updated {toUpdate2.Count} emails in second DB to @gmail.com");
        }
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Students}/{action=Index}/{id?}");

app.Run();
