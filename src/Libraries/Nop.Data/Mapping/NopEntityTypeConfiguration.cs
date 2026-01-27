using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nop.Data.Mapping
{
    public abstract class NopEntityTypeConfiguration<T> : IEntityTypeConfiguration<T> where T : class
    {
        /// <summary>
        /// Configure entity
        /// </summary>
        /// <param name="builder">Entity type builder</param>
        public abstract void Configure(EntityTypeBuilder<T> builder);

        /// <summary>
        /// Developers can override this method in custom partial classes
        /// in order to add some custom initialization code
        /// </summary>
        protected virtual void PostInitialize()
        {
            
        }
    }
}