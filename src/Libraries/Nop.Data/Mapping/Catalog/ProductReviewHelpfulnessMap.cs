using Nop.Core.Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nop.Data.Mapping.Catalog
{
    public partial class ProductReviewHelpfulnessMap : NopEntityTypeConfiguration<ProductReviewHelpfulness>
    {
        public override void Configure(EntityTypeBuilder<ProductReviewHelpfulness> builder)
        {
            builder.ToTable("ProductReviewHelpfulness");
            builder.HasKey(pr => pr.Id);

            builder.HasOne(prh => prh.ProductReview)
                .WithMany(pr => pr.ProductReviewHelpfulnessEntries)
                .HasForeignKey(prh => prh.ProductReviewId).OnDelete(DeleteBehavior.Cascade);
            PostInitialize();
        }
    }
}