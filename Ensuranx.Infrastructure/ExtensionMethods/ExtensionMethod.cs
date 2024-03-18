using System.Linq.Expressions;

namespace Ensuranx.Infrastructure.ExtensionMethods;

public static class ExtensionMethod
{
    private static Random rng = new Random();

    public static IQueryable<T> OrderByField<T>(this IQueryable<T> q, string SortField, bool Ascending)
    {
        var param = Expression.Parameter(typeof(T), "p");
        var prop = Expression.Property(param, SortField);
        var exp = Expression.Lambda(prop, param);
        string method = Ascending ? "OrderBy" : "OrderByDescending";
        Type[] types = new Type[] { q.ElementType, exp.Body.Type };
        var mce = Expression.Call(typeof(Queryable), method, types, q.Expression, exp);
        return q.Provider.CreateQuery<T>(mce);
    }

    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    
}
