using Nop.Core.Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nop.Data.Mapping.Catalog
{
    public partial class RelatedProductMap : NopEntityTypeConfiguration<RelatedProduct>
    {
        public override void Configure(EntityTypeBuilder<RelatedProduct> builder)
        {
            builder.ToTable("RelatedProduct");
            builder.HasKey(c => c.Id);
            PostInitialize();
        }
    }
}