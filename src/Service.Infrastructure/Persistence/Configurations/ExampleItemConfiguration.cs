using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Service.Domain.Examples;

namespace Service.Infrastructure.Persistence.Configurations;

internal sealed class ExampleItemConfiguration : IEntityTypeConfiguration<ExampleItem>
{
    public void Configure(EntityTypeBuilder<ExampleItem> builder)
    {
        builder.ToTable("example_items");
        builder.HasKey(item => item.Id);
        builder.Property(item => item.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(item => item.Name)
            .HasColumnName("name")
            .HasMaxLength(ExampleName.MaxLength)
            .HasConversion(name => name.Value, value => ExampleName.Create(value))
            .IsRequired();
        builder.Property(item => item.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Ignore(item => item.DomainEvents);
    }
}
