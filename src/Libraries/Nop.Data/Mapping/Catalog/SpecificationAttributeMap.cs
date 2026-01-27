using Nop.Core.Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nop.Data.Mapping.Catalog
{
    public partial class SpecificationAttributeMap : NopEntityTypeConfiguration<SpecificationAttribute>
    {
        public override void Configure(EntityTypeBuilder<SpecificationAttribute> builder)
        {
            builder.ToTable("SpecificationAttribute");
            builder.HasKey(sa => sa.Id);
            builder.Property(sa => sa.Name).IsRequired();
            PostInitialize();
        }
    }
}