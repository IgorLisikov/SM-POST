using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Post.Query.Domain.Entities;

namespace Post.Query.Infrastructure.DataAccess
{
    public class CommentConfiguration : IEntityTypeConfiguration<CommentEntity>
    {
        public void Configure(EntityTypeBuilder<CommentEntity> builder)
        {
            builder.ToTable("Comment");

            builder.HasKey(x => x.CommentId);

            builder.Property(x => x.Username)
              .HasMaxLength(255)
              .IsRequired();

            builder.Property(x => x.CommentDate)
              .IsRequired();

            builder.Property(x => x.Comment)
              .HasMaxLength(2000)
              .IsRequired();

            builder.Property(x => x.Edited)
              .IsRequired();

            builder.Property(x => x.PostId)
              .IsRequired();
        }
    }
}
