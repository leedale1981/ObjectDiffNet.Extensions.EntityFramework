using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using ObjectDiffNet.Common;

namespace ObjectDiffNet.Extensions.EntityFramework;

public static class EntityFrameworkDiffer
{
    /// <summary>
    /// Returns differences for the specified <paramref name="entry"/>.
    /// </summary>
    /// <param name="differ">IDiffer to extend</param>
    /// <param name="entry">EntityEntry to apply differences to.</param>
    /// <returns>IEnumerable of Differences</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static IEnumerable<Difference> GetDifferences(this IDiffer differ, EntityEntry entry)
    {
        if (entry == null)
        {
            throw new ArgumentNullException(nameof(entry));
        }

        var original = entry.OriginalValues.ToObject();
        var current = entry.CurrentValues.ToObject();

        return differ.GetDifferences(original, current);
    }
    
    /// <summary>
    /// Returns differences for all entities in the specified <paramref name="dbContext"/>.
    /// </summary>
    /// <param name="differ"></param>
    /// <param name="dbContext"></param>
    /// <returns></returns>
    public static IEnumerable<Difference> GetDifferences(this IDiffer differ, DbContext dbContext)
    {
        List<Difference> differences = new List<Difference>();
        IEnumerable<EntityEntry> entries = dbContext.ChangeTracker.Entries();
        
        foreach (var entry in entries)
        {
            var original = entry.OriginalValues.ToObject();
            var current = entry.CurrentValues.ToObject();

            differences.AddRange(differ.GetDifferences(original, current));
        }

        return differences;
    }
    
    /// <summary>
    /// Returns differences for all entities of the specified <paramref name="entityType"/> in the specified <paramref name="dbContext"/>.
    /// </summary>
    /// <param name="differ"></param>
    /// <param name="dbContext"></param>
    /// <param name="entityType"></param>
    /// <returns></returns>
    public static IEnumerable<Difference> GetDifferences(this IDiffer differ, DbContext dbContext, Type entityType)
    {
        List<Difference> differences = new List<Difference>();
        IEnumerable<EntityEntry> entries = dbContext.ChangeTracker.Entries()
            .Where(e => e.Entity.GetType() == entityType);
        
        foreach (var entry in entries)
        {
            var original = entry.OriginalValues.ToObject();
            var current = entry.CurrentValues.ToObject();

            differences.AddRange(differ.GetDifferences(original, current));
        }

        return differences;
    }
}