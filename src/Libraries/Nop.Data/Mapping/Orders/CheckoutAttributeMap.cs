using Nop.Core.Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nop.Data.Mapping.Orders
{
    public partial class CheckoutAttributeMap : NopEntityTypeConfiguration<CheckoutAttribute>
    {
        public override void Configure(EntityTypeBuilder<CheckoutAttribute> builder)
        {
            builder.ToTable("CheckoutAttribute");
            builder.HasKey(ca => ca.Id);
            builder.Property(ca => ca.Name).IsRequired().HasMaxLength(400);

            builder.Ignore(ca => ca.AttributeControlType);
            PostInitialize();
        }
    }
}