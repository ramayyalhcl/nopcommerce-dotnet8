using Nop.Core.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nop.Data.Mapping.Common
{
    public partial class SearchTermMap : NopEntityTypeConfiguration<SearchTerm>
    {
        public override void Configure(EntityTypeBuilder<SearchTerm> builder)
        {
            builder.ToTable("SearchTerm");
            builder.HasKey(st => st.Id);
            PostInitialize();
        }
    }
}