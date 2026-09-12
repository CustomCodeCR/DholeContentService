using CustomCodeFramework.Postgres.EntityFramework.Repositories;
using Dhole.Content.Application.Abstractions.Repositories;
using Dhole.Content.Domain.Forms;
using Dhole.Content.Domain.Forms.Entities;
using Dhole.Content.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Dhole.Content.Persistence.Repositories;

public sealed class MarketingFormFieldRepository(ServiceDbContext db) : EfRepository<MarketingFormField, Guid>(db), IMarketingFormFieldRepository
{
    public Task<bool> ExistsByFieldKeyAsync(Guid formId, string fieldKey, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var normalizedKey = MarketingFormRules.NormalizeFieldKey(fieldKey);
        return db.MarketingFormFields.AnyAsync(x => !x.IsDeleted && x.FormId == formId && x.FieldKey == normalizedKey &&
            (!excludeId.HasValue || x.Id != excludeId.Value), cancellationToken);
    }

    public async Task<IReadOnlyCollection<MarketingFormField>> GetByFormAsync(Guid formId, CancellationToken cancellationToken = default)
        => await db.MarketingFormFields.AsNoTracking()
            .Where(x => x.FormId == formId && !x.IsDeleted)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);
}
