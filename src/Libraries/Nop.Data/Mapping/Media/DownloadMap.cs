using Nop.Core.Domain.Media;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nop.Data.Mapping.Media
{
    public partial class DownloadMap : NopEntityTypeConfiguration<Download>
    {
        public override void Configure(EntityTypeBuilder<Download> builder)
        {
            builder.ToTable("Download");
            builder.HasKey(p => p.Id);
            builder.Property(p => p.DownloadBinary);
            PostInitialize();
        }
    }
}