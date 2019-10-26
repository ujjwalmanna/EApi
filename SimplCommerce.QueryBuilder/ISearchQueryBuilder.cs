using SimplCommerce.Module.Catalog.ViewModels;

namespace SimplCommerce.QueryBuilder
{
    public interface ISearchQueryBuilder
    {
        string GetQuery(SearchOption request);
    }
}
