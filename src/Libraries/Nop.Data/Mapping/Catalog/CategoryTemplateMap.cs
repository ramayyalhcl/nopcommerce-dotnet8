using Nop.Core.Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nop.Data.Mapping.Catalog
{
    public partial class CategoryTemplateMap : NopEntityTypeConfiguration<CategoryTemplate>
    {
        public override void Configure(EntityTypeBuilder<CategoryTemplate> builder)
        {
            builder.ToTable("CategoryTemplate");
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Name).IsRequired().HasMaxLength(400);
            builder.Property(p => p.ViewPath).IsRequired().HasMaxLength(400);
            PostInitialize();
        }
    }
}