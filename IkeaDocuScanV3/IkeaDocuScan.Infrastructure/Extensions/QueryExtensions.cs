using IkeaDocuScan.Infrastructure.Data;
using IkeaDocuScan.Infrastructure.Entities;
using IkeaDocuScan.Shared.Models.Authorization;
using Microsoft.EntityFrameworkCore;

namespace IkeaDocuScan.Infrastructure.Extensions;

/// <summary>
/// Extension methods for IQueryable to support permission-based filtering
/// </summary>
public static class QueryExtensions
{
    /// <summary>
    /// Filters documents based on user permissions.
    /// A document is accessible if ANY of the user's permissions match:
    /// - DocumentType matches (or is null in either document or permission)
    ///
    /// SuperUser bypass: If user is SuperUser, no filtering is applied.
    /// No access: If user has no access, returns empty result.
    /// </summary>
    /// <param name="query">The document query to filter</param>
    /// <param name="currentUser">The current authenticated user with permissions</param>
    /// <param name="context">Database context for UserPermission lookups</param>
    /// <returns>Filtered queryable of documents</returns>
    public static IQueryable<Document> FilterByUserPermissions(
        this IQueryable<Document> query,
        CurrentUser currentUser,
        AppDbContext context)
    {
        // SuperUser bypass - sees all documents
        if (currentUser.IsSuperUser)
            return query;

        // No access - sees no documents
        if (!currentUser.HasAccess)
            return query.Where(d => false); // Empty result

        int userId = currentUser.UserId;

        // Filter documents where ANY permission matches DocumentType
        return query.Where(doc =>
            context.UserPermissions
                .Where(p => p.UserId == userId)
                .Any(perm =>
                    // DocumentType filter: match if both null OR values equal
                    doc.DtId == null || perm.DocumentTypeId == null || doc.DtId == perm.DocumentTypeId
                )
        );
    }
}
