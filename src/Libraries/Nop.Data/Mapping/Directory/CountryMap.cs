using Nop.Core.Domain.Directory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nop.Data.Mapping.Directory
{
    public partial class CountryMap : NopEntityTypeConfiguration<Country>
    {
        public override void Configure(EntityTypeBuilder<Country> builder)
        {
            builder.ToTable("Country");
            builder.HasKey(c =>c.Id);
            builder.Property(c => c.Name).IsRequired().HasMaxLength(100);
            builder.Property(c =>c.TwoLetterIsoCode).HasMaxLength(2);
            builder.Property(c =>c.ThreeLetterIsoCode).HasMaxLength(3);
            PostInitialize();
        }
    }
}