using Nop.Core.Domain.Directory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nop.Data.Mapping.Directory
{
    public partial class StateProvinceMap : NopEntityTypeConfiguration<StateProvince>
    {
        public override void Configure(EntityTypeBuilder<StateProvince> builder)
        {
            builder.ToTable("StateProvince");
            builder.HasKey(sp => sp.Id);
            builder.Property(sp => sp.Name).IsRequired().HasMaxLength(100);
            builder.Property(sp => sp.Abbreviation).HasMaxLength(100);


            builder.HasOne(sp => sp.Country)
                .WithMany(c => c.StateProvinces)
                .HasForeignKey(sp => sp.CountryId);
            PostInitialize();
        }
    }
}