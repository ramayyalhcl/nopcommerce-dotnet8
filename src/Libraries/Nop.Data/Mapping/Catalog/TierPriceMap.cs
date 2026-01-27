using Nop.Core.Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nop.Data.Mapping.Catalog
{
    public partial class TierPriceMap : NopEntityTypeConfiguration<TierPrice>
    {
        public override void Configure(EntityTypeBuilder<TierPrice> builder)
        {
            builder.ToTable("TierPrice");
            builder.HasKey(tp => tp.Id);
            builder.Property(tp => tp.Price).HasPrecision(18, 4);

            builder.HasOne(tp => tp.Product)
                .WithMany(p => p.TierPrices)
                .HasForeignKey(tp => tp.ProductId);

            builder.HasOne(tp => tp.CustomerRole)
                .WithMany()
                .HasForeignKey(tp => tp.CustomerRoleId)
                .OnDelete(DeleteBehavior.Cascade);
            PostInitialize();
        }
    }
}