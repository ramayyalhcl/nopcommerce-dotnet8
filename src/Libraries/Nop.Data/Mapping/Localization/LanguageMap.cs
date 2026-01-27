using Nop.Core.Domain.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nop.Data.Mapping.Localization
{
    public partial class LanguageMap : NopEntityTypeConfiguration<Language>
    {
        public override void Configure(EntityTypeBuilder<Language> builder)
        {
            builder.ToTable("Language");
            builder.HasKey(l => l.Id);
            builder.Property(l => l.Name).IsRequired().HasMaxLength(100);
            builder.Property(l => l.LanguageCulture).IsRequired().HasMaxLength(20);
            builder.Property(l => l.UniqueSeoCode).HasMaxLength(2);
            builder.Property(l => l.FlagImageFileName).HasMaxLength(50);
        
            PostInitialize();
        }
    }
}