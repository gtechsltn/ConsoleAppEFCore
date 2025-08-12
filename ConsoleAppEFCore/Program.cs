using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

class Program
{
    static async Task Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                services.AddDbContextFactory<AppDbContext>(options =>
                    options.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=ConsoleEFCoreDb;Trusted_Connection=True;TrustServerCertificate=True;Encrypt=True;"));

                services.AddTransient<DataService>();
            })
            .Build();

        await host.Services.GetRequiredService<DataService>().RunAsync();
    }
}

public class DataService
{
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;

    public DataService(IDbContextFactory<AppDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task RunAsync()
    {
        // 1. Khởi tạo database và migration
        using (var context = _dbContextFactory.CreateDbContext())
        {
            await context.Database.EnsureCreatedAsync(); // Hoặc context.Database.MigrateAsync()
            Console.WriteLine("Database created or already exists.");
        }

        // 2. Thêm một sản phẩm mới
        Console.WriteLine("Adding new product...");
        await AddProductAsync("Laptop");

        // 3. Hiển thị tất cả sản phẩm
        Console.WriteLine("Fetching all products...");
        await GetProductsAsync();
    }

    public async Task AddProductAsync(string name)
    {
        using var context = _dbContextFactory.CreateDbContext();
        context.Products.Add(new Product { Name = name });
        await context.SaveChangesAsync();
        Console.WriteLine($"Added product: {name}");
    }

    public async Task GetProductsAsync()
    {
        using var context = _dbContextFactory.CreateDbContext();
        var products = await context.Products.ToListAsync();
        foreach (var product in products)
        {
            Console.WriteLine($"ID: {product.Id}, Name: {product.Name}");
        }
    }
}

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<Product> Products { get; set; }
}

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}