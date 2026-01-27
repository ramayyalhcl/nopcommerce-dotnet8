using Nop.Core.Domain.Messages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nop.Data.Mapping.Messages
{
    public partial class NewsLetterSubscriptionMap : NopEntityTypeConfiguration<NewsLetterSubscription>
    {
        public override void Configure(EntityTypeBuilder<NewsLetterSubscription> builder)
        {
            builder.ToTable("NewsLetterSubscription");
            builder.HasKey(nls => nls.Id);

            builder.Property(nls => nls.Email).IsRequired().HasMaxLength(255);
            PostInitialize();
        }
    }
}