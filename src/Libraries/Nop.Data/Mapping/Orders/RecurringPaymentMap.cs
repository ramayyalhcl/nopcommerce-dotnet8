using Nop.Core.Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nop.Data.Mapping.Orders
{
    public partial class RecurringPaymentMap : NopEntityTypeConfiguration<RecurringPayment>
    {
        public override void Configure(EntityTypeBuilder<RecurringPayment> builder)
        {
            builder.ToTable("RecurringPayment");
            builder.HasKey(rp => rp.Id);

            builder.Ignore(rp => rp.NextPaymentDate);
            builder.Ignore(rp => rp.CyclesRemaining);
            builder.Ignore(rp => rp.CyclePeriod);



            //builder.HasOne(rp => rp.InitialOrder).WithOptional().Map(x => x.MapKey("InitialOrderId")).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(rp => rp.InitialOrder)
                .WithMany()
                .HasForeignKey(o => o.InitialOrderId)
                .OnDelete(DeleteBehavior.Restrict);
            PostInitialize();
        }
    }
}