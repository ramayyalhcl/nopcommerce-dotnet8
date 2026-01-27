using Nop.Core.Domain.Forums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nop.Data.Mapping.Forums
{
    public partial class ForumPostVoteMap : NopEntityTypeConfiguration<ForumPostVote>
    {
        public override void Configure(EntityTypeBuilder<ForumPostVote> builder)
        {
            builder.ToTable("Forums_PostVote");
            builder.HasKey(fpv => fpv.Id);

            builder.HasOne(fpv => fpv.ForumPost)
                .WithMany()
                .HasForeignKey(fpv => fpv.ForumPostId)
                .OnDelete(DeleteBehavior.Cascade);
            PostInitialize();
        }
    }
}