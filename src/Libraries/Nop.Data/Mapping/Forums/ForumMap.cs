using Nop.Core.Domain.Forums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nop.Data.Mapping.Forums
{
    public partial class ForumMap : NopEntityTypeConfiguration<Forum>
    {
        public override void Configure(EntityTypeBuilder<Forum> builder)
        {
            builder.ToTable("Forums_Forum");
            builder.HasKey(f => f.Id);
            builder.Property(f => f.Name).IsRequired().HasMaxLength(200);
            
            builder.HasOne(f => f.ForumGroup)
                .WithMany(fg => fg.Forums)
                .HasForeignKey(f => f.ForumGroupId);
            PostInitialize();
        }
    }
}