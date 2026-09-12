using CustomCodeFramework.Postgres.EntityFramework.Configurations;
using Dhole.Content.Domain.Redirects.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dhole.Content.Persistence.Configurations.Redirects;

internal sealed class ContentRedirectConfiguration : EntityTypeConfigurationBase<ContentRedirect, Guid>
{
    public override void Configure(EntityTypeBuilder<ContentRedirect> builder)
    {
        base.Configure(builder);
        builder.ToTable("redirects");
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.SiteKey).HasMaxLength(80).IsRequired();
        builder.Property(x => x.SourcePath).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.TargetUrl).HasMaxLength(2000).IsRequired();
        builder.HasIndex(x => new { x.SiteKey, x.SourcePath }).IsUnique().HasFilter("is_deleted = false");
        builder.HasIndex(x => new { x.SiteKey, x.IsActive, x.ValidFromUtc, x.ValidToUtc }).HasFilter("is_deleted = false");
    }
}
