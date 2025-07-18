using Api.Application;
using Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace Api.Infrastructure;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<Payment> Payments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure User aggregate
        modelBuilder.Entity<User>(b =>
        {
            b.HasKey(u => u.Id);
            b.OwnsMany(u => u.Orders, od =>
            {
                od.WithOwner().HasForeignKey("UserId");
                od.HasKey("Id");
                od.Property<DateTime>("PlacedAt");
                od.OwnsOne(o => o.Batch, bno =>
                {
                    bno.Property(b => b.Value).HasColumnName("Batch");
                });
                od.Property<decimal>("Quantity").HasPrecision(18, 2);
                od.OwnsOne(o => o.Charge, co =>
                {
                    co.Property(p => p.Amount).HasColumnName("Charge_Amount").HasPrecision(18, 2);
                    co.Property(p => p.Currency).HasColumnName("Charge_Currency");
                });
            });
            b.OwnsMany(u => u.Payments, pd =>
            {
                pd.WithOwner().HasForeignKey("UserId");
                pd.HasKey("Id");
                pd.Property<DateTime>("Date");
                pd.OwnsOne(p => p.Amount, ao =>
                {
                    ao.Property(p => p.Amount).HasColumnName("Amount_Amount").HasPrecision(18, 2);
                    ao.Property(p => p.Currency).HasColumnName("Amount_Currency");
                });
            });
        });
    }
}

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _db;
    public UserRepository(ApplicationDbContext db) => _db = db;

    public User GetById(int userId)
        => _db.Users
              .Include(u => u.Orders)
              .Include(u => u.Payments)
              .SingleOrDefault(u => u.Id == userId);

    public IEnumerable<User> GetAll()
        => _db.Users.AsNoTracking().ToList();

    public void Save(User user)
    {
        if (_db.Entry(user).State == EntityState.Detached)
            _db.Users.Add(user);
        _db.SaveChanges();
    }
}

public class BatchAssignmentService : IBatchAssignmentService
{
    private int _current = 1;
    public BatchNumber GetCurrentBatch() => new BatchNumber(_current);
    public void AdvanceToNextBatch() => _current++;
}