using Nop.Core.Domain.Affiliates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nop.Data.Mapping.Affiliates
{
    public partial class AffiliateMap : NopEntityTypeConfiguration<Affiliate>
    {
        public override void Configure(EntityTypeBuilder<Affiliate> builder)
        {
            builder.ToTable("Affiliate");
            builder.HasKey(a => a.Id);

            builder.HasOne(a => a.Address).WithMany().HasForeignKey(x => x.AddressId).OnDelete(DeleteBehavior.Restrict);
            PostInitialize();
        }
    }
}