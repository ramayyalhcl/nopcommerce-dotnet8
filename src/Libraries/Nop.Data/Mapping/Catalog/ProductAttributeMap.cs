using Nop.Core.Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nop.Data.Mapping.Catalog
{
    public partial class ProductAttributeMap : NopEntityTypeConfiguration<ProductAttribute>
    {
        public override void Configure(EntityTypeBuilder<ProductAttribute> builder)
        {
            builder.ToTable("ProductAttribute");
            builder.HasKey(pa => pa.Id);
            builder.Property(pa => pa.Name).IsRequired();
            PostInitialize();
        }
    }
}