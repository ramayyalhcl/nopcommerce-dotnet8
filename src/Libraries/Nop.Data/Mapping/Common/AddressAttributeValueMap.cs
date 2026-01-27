using Nop.Core.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nop.Data.Mapping.Common
{
    public partial class AddressAttributeValueMap : NopEntityTypeConfiguration<AddressAttributeValue>
    {
        public override void Configure(EntityTypeBuilder<AddressAttributeValue> builder)
        {
            builder.ToTable("AddressAttributeValue");
            builder.HasKey(aav => aav.Id);
            builder.Property(aav => aav.Name).IsRequired().HasMaxLength(400);

            builder.HasOne(aav => aav.AddressAttribute)
                .WithMany(aa => aa.AddressAttributeValues)
                .HasForeignKey(aav => aav.AddressAttributeId);
            PostInitialize();
        }
    }
}