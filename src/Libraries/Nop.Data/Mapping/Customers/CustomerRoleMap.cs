using Nop.Core.Domain.Customers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nop.Data.Mapping.Customers
{
    public partial class CustomerRoleMap : NopEntityTypeConfiguration<CustomerRole>
    {
        public override void Configure(EntityTypeBuilder<CustomerRole> builder)
        {
            builder.ToTable("CustomerRole");
            builder.HasKey(cr => cr.Id);
            builder.Property(cr => cr.Name).IsRequired().HasMaxLength(255);
            builder.Property(cr => cr.SystemName).HasMaxLength(255);
            PostInitialize();
        }
    }
}