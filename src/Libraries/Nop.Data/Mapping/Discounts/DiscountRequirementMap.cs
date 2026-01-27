using Nop.Core.Domain.Discounts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nop.Data.Mapping.Discounts
{
    public partial class DiscountRequirementMap : NopEntityTypeConfiguration<DiscountRequirement>
    {
        public override void Configure(EntityTypeBuilder<DiscountRequirement> builder)
        {
            builder.ToTable("DiscountRequirement");
            builder.HasKey(requirement => requirement.Id);

            builder.Ignore(requirement => requirement.InteractionType);
            builder.HasMany(requirement => requirement.ChildRequirements)
                .WithOne()
                .HasForeignKey(requirement => requirement.ParentId);
            PostInitialize();
        }
    }
}