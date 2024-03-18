using Ensuranx.Domain.Entities;
using Ensuranx.Domain.IdentityExtensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Ensuranx.Infrastructure.DbContext
{
    public class ApplicationDbContext : IdentityDbContext<AppUser, AppRole, int>
    {

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<AppRole>()
                .Property(e => e.Description)
                .HasMaxLength(250);



            builder.Entity<AppUser>()
                    .ToTable("User", "dbo");

            builder.Entity<AppRole>()
                    .ToTable("Role", "dbo");

            builder.Entity<IdentityUserRole<int>>().ToTable("UserRole");

            builder.Entity<IdentityRoleClaim<int>>()
                    .ToTable("RoleClaim", "dbo");

            builder.Entity<IdentityUserLogin<int>>()
                    .ToTable("UserLogin", "dbo");

            builder.Entity<IdentityUserToken<int>>()
                    .ToTable("UserToken", "dbo");
        }



        public DbSet<UserInfo> UserInfo { get; set; }

        public DbSet<Notifications> Notification { get; set; }
        public DbSet<NotificationTo> NotificationTo { get; set; }
        public DbSet<Business> Business { get; set; }
        public DbSet<BusinessType> BusinessType { get; set; }

        public DbSet<BusinessPackages> BusinessPackage { get; set; }
        public DbSet<EmailTemplates> EmailTemplate { get; set; }

        public DbSet<ConnectionType> ConnectionType { get; set; }
        public DbSet<Connection> Connection { get; set; }
        public DbSet<ConnectionEntity> ConnectionEntity { get; set; }
        public DbSet<Images> Image { get; set; }


        public DbSet<PaymentOption> PaymentOption { get; set; }
        public DbSet<BusinessPackageType> BusinessPackageType { get; set; }

        public DbSet<PermissionType> PermissionType { get; set; }
        public DbSet<UserPermission> UserPermission { get; set; }
        public DbSet<Permission> Permission { get; set; }
        public DbSet<PermissionPermissionType> PermissionPermissionType { get; set; }

        public DbSet<Provider>  providers { get; set; }

    }
}
