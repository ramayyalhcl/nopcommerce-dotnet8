using Nop.Core.Domain.Polls;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nop.Data.Mapping.Polls
{
    public partial class PollAnswerMap : NopEntityTypeConfiguration<PollAnswer>
    {
        public override void Configure(EntityTypeBuilder<PollAnswer> builder)
        {
            builder.ToTable("PollAnswer");
            builder.HasKey(pa => pa.Id);
            builder.Property(pa => pa.Name).IsRequired();

            builder.HasOne(pa => pa.Poll)
                .WithMany(p => p.PollAnswers)
                .HasForeignKey(pa => pa.PollId).OnDelete(DeleteBehavior.Cascade);
            PostInitialize();
        }
    }
}