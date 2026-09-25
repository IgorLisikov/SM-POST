using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Post.Query.Domain.Entities;

namespace Post.Query.Infrastructure.DataAccess
{
    public class ProcessedEventConfiguration : IEntityTypeConfiguration<ProcessedEvent>
    {
        public void Configure(EntityTypeBuilder<ProcessedEvent> builder)
        {
            builder.ToTable("ProcessedEvent");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.AggregateId)
              .IsRequired();

            builder.Property(x => x.Version)
              .IsRequired();

            builder.Property(x => x.ProcessedAt)
              .IsRequired();

            builder.HasIndex(p => new { p.AggregateId, p.Version })
                .IsUnique();
        }
    }
}
