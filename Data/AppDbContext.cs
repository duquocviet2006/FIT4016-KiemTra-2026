using Microsoft.EntityFrameworkCore;
using SchoolManagement.Models;

namespace SchoolManagement.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<School> Schools { get; set; } = null!;
        public DbSet<Student> Students { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<School>(entity =>
            {
                entity.ToTable("schools");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired();
                entity.HasIndex(e => e.Name).IsUnique();
                entity.Property(e => e.Principal).IsRequired();
                entity.Property(e => e.Address).IsRequired();
            });

            modelBuilder.Entity<Student>(entity =>
            {
                entity.ToTable("students");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FullName).IsRequired();
                entity.Property(e => e.StudentId).IsRequired();
                entity.HasIndex(e => e.StudentId).IsUnique();
                entity.Property(e => e.Email).IsRequired();
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasOne(s => s.School)
                      .WithMany(sch => sch.Students)
                      .HasForeignKey(s => s.SchoolId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }

        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateTimestamps()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is School || e.Entity is Student);

            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Property("CreatedAt").CurrentValue = DateTime.UtcNow;
                }
                if (entry.State == EntityState.Modified)
                {
                    entry.Property("UpdatedAt").CurrentValue = DateTime.UtcNow;
                }
            }
        }
    }

        public static class SeedData
        {
            private static readonly List<(string Name, string Principal, string Address)> HanoiUniversities = new()
            {
                ("Đại học Quốc gia Hà Nội", "PGS. TS. Nguyễn Văn A", "144 Xuân Thủy, Cầu Giấy, Hà Nội"),
                ("Trường Đại học Bách khoa Hà Nội", "PGS. TS. Trần Văn B", "1 Đại Cồ Việt, Hai Bà Trưng, Hà Nội"),
                ("Trường Đại học Sư phạm Hà Nội", "PGS. TS. Lê Thị C", "136 Xuân Thủy, Cầu Giấy, Hà Nội"),
                ("Trường Đại học Ngoại thương (Hà Nội)", "PGS. TS. Phạm Văn D", "91 Chùa Láng, Đống Đa, Hà Nội"),
                ("Trường Đại học Ngoại ngữ - ĐHQG Hà Nội", "PGS. TS. Nguyễn Thị E", "2 Phạm Văn Đồng, Cầu Giấy, Hà Nội"),
                ("Đại học Y Hà Nội", "PGS. TS. Hoàng Văn F", "1 Tôn Thất Tùng, Đống Đa, Hà Nội"),
                ("Học viện Nông nghiệp Việt Nam", "PGS. TS. Dương Văn G", "Gia Lâm, Hà Nội"),
                ("Trường Đại học Kinh tế Quốc dân", "PGS. TS. Bùi Thị H", "207 Giải Phóng, Đống Đa, Hà Nội"),
                ("Học viện Tài chính", "PGS. TS. Ngô Văn I", "58 Hoàng Diệu, Ba Đình, Hà Nội"),
                ("Trường Đại học Kiến trúc Hà Nội", "PGS. TS. Trần Thị J", "1 Lê Đại Hành, Hai Bà Trưng, Hà Nội")
            };

            public static async Task EnsureSeedDataAsync(AppDbContext ctx)
            {
                // If no schools exist, create them and students
                if (!ctx.Schools.Any())
                {
                    var schools = HanoiUniversities.Select(u => new School
                    {
                        Name = u.Name,
                        Principal = u.Principal,
                        Address = u.Address
                    }).ToList();

                    ctx.Schools.AddRange(schools);
                    await ctx.SaveChangesAsync();

                    var rand = new Random();
                    var students = new List<Student>();
                    for (int i = 1; i <= 20; i++)
                    {
                        var school = schools[rand.Next(schools.Count)];
                        students.Add(new Student
                        {
                            FullName = $"Student {i} Name",
                            StudentId = $"SID{i:00000}",
                            Email = $"student{i}@example.com",
                            Phone = (1000000000 + i).ToString(),
                            SchoolId = school.Id
                        });
                    }

                    ctx.Students.AddRange(students);
                    await ctx.SaveChangesAsync();
                    return;
                }

                // If schools exist, update their info to match Hanoi universities
                var existingSchools = await ctx.Schools.OrderBy(s => s.Id).ToListAsync();
                for (int i = 0; i < HanoiUniversities.Count; i++)
                {
                    if (i < existingSchools.Count)
                    {
                        var ex = existingSchools[i];
                        var u = HanoiUniversities[i];
                        ex.Name = u.Name;
                        ex.Principal = u.Principal;
                        ex.Address = u.Address;
                        ctx.Schools.Update(ex);
                    }
                    else
                    {
                        // add missing schools
                        var u = HanoiUniversities[i];
                        ctx.Schools.Add(new School
                        {
                            Name = u.Name,
                            Principal = u.Principal,
                            Address = u.Address
                        });
                    }
                }

                await ctx.SaveChangesAsync();
            }
        }
}
