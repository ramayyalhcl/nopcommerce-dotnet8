using Nop.Core.Domain.Customers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nop.Data.Mapping.Customers
{
    public partial class RewardPointsHistoryMap : NopEntityTypeConfiguration<RewardPointsHistory>
    {
        public override void Configure(EntityTypeBuilder<RewardPointsHistory> builder)
        {
            builder.ToTable("RewardPointsHistory");
            builder.HasKey(rph => rph.Id);

            builder.Property(rph => rph.UsedAmount).HasPrecision(18, 4);

            builder.HasOne(rph => rph.Customer)
                .WithMany()
                .HasForeignKey(rph => rph.CustomerId);

            builder.HasOne(rph => rph.UsedWithOrder)
                .WithOne(o => o.RedeemedRewardPointsEntry)
                .OnDelete(DeleteBehavior.Restrict);
            PostInitialize();
        }
    }
}