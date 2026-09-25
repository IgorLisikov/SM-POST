using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Post.Query.Domain.Entities;

namespace Post.Query.Infrastructure.DataAccess
{
    public class PostConfiguration : IEntityTypeConfiguration<PostEntity>
    {
        public void Configure(EntityTypeBuilder<PostEntity> builder)
        {
            builder.ToTable("Post");

            builder.HasKey(x => x.PostId);

            builder.Property(x => x.Author)
              .HasMaxLength(255)
              .IsRequired();

            builder.Property(x => x.DatePosted)
              .IsRequired();

            builder.Property(x => x.Message)
              .HasMaxLength(2000)
              .IsRequired();

            builder.Property(x => x.Likes)
              .IsRequired();

            builder.HasMany(x => x.Comments)
              .WithOne(c => c.Post)
              .HasForeignKey(c => c.PostId)
              .IsRequired();
        }
    }
}
