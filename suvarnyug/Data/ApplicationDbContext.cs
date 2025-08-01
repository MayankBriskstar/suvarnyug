using Microsoft.EntityFrameworkCore;
using suvarnyug.Models;
using Suvarnyug.Models;

namespace Suvarnyug.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Biodata> Biodata { get; set; }
        public DbSet<BiodataImage> BiodataImages { get; set; }
        public DbSet<Forum> Forums { get; set; }
        public DbSet<Reply> Replies { get; set; }
        public DbSet<ReplyLike> ReplyLikes { get; set; }
        public DbSet<Biodata> Biodatas { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<ForumLike> ForumLikes { get; set; }
        public DbSet<ProfileViewHistory> ProfileViewHistory { get; set; }
        public DbSet<ContactUs> ContactUs { get; set; }
        public DbSet<InterestedProfile> InterestedProfiles { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<ChatRoom> ChatRooms { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<State> States { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<UserVerification> UserVerification { get; set; }
        public DbSet<UserLogin> UserLogins { get; set; }
        public DbSet<DirectPayment> DirectPayments { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();
            modelBuilder.Entity<BiodataImage>()
                  .HasKey(b => b.ImageId);

            modelBuilder.Entity<BiodataImage>()
                .HasOne(b => b.Biodata)
                .WithMany(b => b.Images)
                .HasForeignKey(b => b.BiodataId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ReplyLike>()
            .HasKey(rl => rl.Id);

            modelBuilder.Entity<ReplyLike>()
                .HasOne(rl => rl.Reply)
                .WithMany(r => r.Likes)
                .HasForeignKey(rl => rl.ReplyId);

            modelBuilder.Entity<ReplyLike>()
                .HasOne(rl => rl.User)
                .WithMany(u => u.ReplyLikes)
                .HasForeignKey(rl => rl.UserId);

            modelBuilder.Entity<ForumLike>()
               .HasKey(fl => fl.Id);

            modelBuilder.Entity<ForumLike>()
                .HasOne(fl => fl.Forum)
                .WithMany(f => f.ForumLikes)
                .HasForeignKey(fl => fl.ForumId);

            modelBuilder.Entity<ForumLike>()
                .HasOne(fl => fl.User)
                .WithMany(u => u.ForumLikes)
                .HasForeignKey(fl => fl.UserId);
            modelBuilder.Entity<Subscription>()
                .HasOne<User>(s => s.User)
                .WithMany()
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ContactUs>()
                .HasKey(c => c.Id);
            modelBuilder.Entity<Subscription>()
                .Property(s => s.PlanType)
                .HasConversion<string>();
            modelBuilder.Entity<DirectPayment>().HasKey(dp => dp.PaymentId);
            modelBuilder.Entity<ProfileViewHistory>(entity =>
            {
                entity.HasKey(e => e.ViewId);
                entity.HasIndex(e => new { e.UserId, e.BiodataId }).IsUnique();

                entity.HasOne(e => e.User)
                      .WithMany(u => u.ProfileViews)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Biodata)
                      .WithMany(b => b.ProfileViews)
                      .HasForeignKey(e => e.BiodataId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
