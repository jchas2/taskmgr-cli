using System.Linq.Expressions;
using System.Reflection;

namespace Task.Manager.Cli.Utils;

public static class QueryableExtensions
{
    public static IOrderedQueryable<TSource> DynamicOrderBy<TSource>(
        this IQueryable<TSource> source,
        string propertyName,
        bool isDescending = false)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));

        if (string.IsNullOrWhiteSpace(propertyName)) {
            throw new ArgumentException($"{nameof(propertyName)} cannot be null or whitespace.", nameof(propertyName));
        }

        var entityType = typeof(TSource);

        var propertyInfo = entityType.GetProperty(
            propertyName, 
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

        if (propertyInfo == null) {
            throw new InvalidOperationException($"Property '{propertyName}' not found on type '{entityType.Name}'.");
        }

        var parameter = Expression.Parameter(entityType, "p");
        var propertyAccess = Expression.MakeMemberAccess(parameter, propertyInfo);
        var orderByExp = Expression.Lambda(propertyAccess, parameter);

        string methodName = isDescending ? "OrderByDescending" : "OrderBy";

        // Find the generic OrderBy/OrderByDescending method via reflection.
        var resultExp = Expression.Call(
            typeof(Queryable), 
            methodName,
            [entityType, propertyInfo.PropertyType], 
            source.Expression, 
            Expression.Quote(orderByExp));

        return (IOrderedQueryable<TSource>)source.Provider.CreateQuery<TSource>(resultExp);
    }
}
