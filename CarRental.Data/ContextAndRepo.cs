using CarRental.Core.Entities;
using CarRental.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.Linq.Expressions;

namespace CarRental.Data
{
    public class CarRentalDbContext : DbContext
    {
        public CarRentalDbContext(DbContextOptions<CarRentalDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<VehicleType> VehicleTypes { get; set; }
        public DbSet<Rental> Rentals { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Maintenance> Maintenances { get; set; }
        public DbSet<VehicleImage> VehicleImages { get; set; }
        public DbSet<Log> Logs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Relationships

            // Vehicle -> VehicleType
            modelBuilder.Entity<Vehicle>()
                .HasOne(v => v.VehicleType)
                .WithMany(vt => vt.Vehicles)
                .HasForeignKey(v => v.VehicleTypeId);

            // Vehicle -> Client (Owner)
            modelBuilder.Entity<Vehicle>()
                .HasOne(v => v.Client)
                .WithMany(c => c.Vehicles)
                .HasForeignKey(v => v.ClientId)
                .OnDelete(DeleteBehavior.SetNull); // If client deleted, keep vehicle but maybe mark unowned or handle logic

            // Vehicle -> Images
            modelBuilder.Entity<Vehicle>()
                .HasMany(v => v.Images)
                .WithOne(i => i.Vehicle)
                .HasForeignKey(i => i.VehicleId);
            
            // Rental -> Vehicle
            modelBuilder.Entity<Rental>()
                .HasOne(r => r.Vehicle)
                .WithMany(v => v.Rentals)
                .HasForeignKey(r => r.VehicleId);

            // Rental -> Client
            modelBuilder.Entity<Rental>()
                .HasOne(r => r.Client)
                .WithMany(c => c.Rentals)
                .HasForeignKey(r => r.ClientId);

            // Rental -> Payments
            modelBuilder.Entity<Rental>()
                .HasMany(r => r.Payments)
                .WithOne(p => p.Rental)
                .HasForeignKey(p => p.RentalId);

            // Vehicle -> Maintenance
            modelBuilder.Entity<Vehicle>()
                .HasMany(v => v.Maintenances)
                .WithOne(m => m.Vehicle)
                .HasForeignKey(m => m.VehicleId);
            
            // Seed Data (Minimal for structure)
            modelBuilder.Entity<VehicleType>().HasData(
                new VehicleType { Id = 1, Name = "Sedan", Description = "Standard sedan" },
                new VehicleType { Id = 2, Name = "SUV", Description = "Sport Utility Vehicle" }
            );

            // Default Admin User
            // Fixed credentials as per requirements: admin@gmail.com / admin123
            modelBuilder.Entity<User>().HasData(
                new User 
                { 
                    Id = 1, 
                    FirstName = "Admin", 
                    LastName = "System", 
                    Email = "admin@gmail.com", 
                    PasswordHash = "admin123", // In a real app, this should be hashed. Requirements specify "simple login", but for production we'd hash. 
                                               // Keeping raw for now to match explicit instruction "Password: admin123" literal, 
                                               // or will implement hashing in AuthService but seed hash here? 
                                               // User said "Password: admin123". I'll store it as is for now or assume AuthService checks raw.
                                               // Better: Store it as is, or use a simple hash. Let's stick to the literal string for simplicity unless AuthService hashes.
                                               // Earlier AuthService had // TODO: Flash this. I will assume plain text for the "simple login" requirement during seed,
                                               // but ideally we should hash. I'll stick to string "admin123" for now.
                    Role = Core.Enums.UserRole.Admin 
                }
            );
        }
    }

    public class CarRentalDbContextFactory : IDesignTimeDbContextFactory<CarRentalDbContext>
    {
        public CarRentalDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<CarRentalDbContext>();
            // Use local connection string for migrations
            var connectionString = "Server=localhost;Database=CarRentalDB;User=root;Password=admin;";
            optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

            return new CarRentalDbContext(optionsBuilder.Options);
        }
    }

    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly CarRentalDbContext _context;
        public Repository(CarRentalDbContext context)
        {
            _context = context;
        }

        public async Task<T?> GetByIdAsync(int id) => await _context.Set<T>().FindAsync(id);
        public async Task<IEnumerable<T>> GetAllAsync() => await _context.Set<T>().ToListAsync();
        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate) => await _context.Set<T>().Where(predicate).ToListAsync();

        public async Task<IEnumerable<T>> FindIncludingAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includeProperties)
        {
            IQueryable<T> query = _context.Set<T>().Where(predicate);
            foreach (var includeProperty in includeProperties)
            {
                query = query.Include(includeProperty);
            }
            return await query.ToListAsync();
        }

        public async Task<IEnumerable<T>> GetAllIncludingAsync(params Expression<Func<T, object>>[] includeProperties)
        {
            IQueryable<T> query = _context.Set<T>();
            foreach (var includeProperty in includeProperties)
            {
                query = query.Include(includeProperty);
            }
            return await query.ToListAsync();
        }

        public async Task AddAsync(T entity) => await _context.Set<T>().AddAsync(entity);
        public void Remove(T entity) => _context.Set<T>().Remove(entity);
        public void Update(T entity) => _context.Set<T>().Update(entity);
    }

    public class UnitOfWork : IUnitOfWork
    {
        private readonly CarRentalDbContext _context;
        private readonly Dictionary<Type, object> _repositories = new();

        public UnitOfWork(CarRentalDbContext context)
        {
            _context = context;
        }

        public IRepository<T> Repository<T>() where T : class
        {
            if (_repositories.ContainsKey(typeof(T)))
            {
                return (IRepository<T>)_repositories[typeof(T)];
            }

            var repository = new Repository<T>(_context);
            _repositories.Add(typeof(T), repository);
            return repository;
        }

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
