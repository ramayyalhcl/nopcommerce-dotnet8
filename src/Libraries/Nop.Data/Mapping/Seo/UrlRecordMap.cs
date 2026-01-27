using Nop.Core.Domain.Seo;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nop.Data.Mapping.Seo
{
    public partial class UrlRecordMap : NopEntityTypeConfiguration<UrlRecord>
    {
        public override void Configure(EntityTypeBuilder<UrlRecord> builder)
        {
            builder.ToTable("UrlRecord");
            builder.HasKey(lp => lp.Id);

            builder.Property(lp => lp.EntityName).IsRequired().HasMaxLength(400);
            builder.Property(lp => lp.Slug).IsRequired().HasMaxLength(400);
            PostInitialize();
        }
    }
}