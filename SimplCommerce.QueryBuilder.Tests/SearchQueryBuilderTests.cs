using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using SimplCommerce.Module.Catalog.ViewModels;
using SimplCommerce.QueryBuilder;
using System.Text.RegularExpressions;

namespace SimplCommerce.QueryBuilder.Tests
{
    [TestClass]
    public class SearchQueryBuilderTests
    {
        [TestMethod]
        public void SearchQuery_Contain_Pagination_Query_When_Pagination_Present()
        {
            var searchOption = new SearchOption
            {
                Page = 10,
                PageSize = 20,
                MaxPrice = 100,
                MinPrice = 10,
                DateRange = 2,
                Query = "Air",
                Sort = "price-asc"
            };
            var queryBuilder = new SearchQueryBuilder();
            var query = queryBuilder.GetQuery(searchOption);
            query.ShouldContain(@"""from"": 10");
            query.ShouldContain(@"""size"": 20");

        }

        [TestMethod]
        public void SearchQuery_Contain_Pagination_Query_When_Pagination_Not_Present()
        {
            var searchOption = new SearchOption
            {
                MaxPrice = 100,
                MinPrice = 10,
                DateRange = 2,
                Query = "Air",
                Sort = "price-asc"
            };
            var queryBuilder = new SearchQueryBuilder();
            var query = queryBuilder.GetQuery(searchOption);
            query.ShouldContain(@"""from"": 0");
            query.ShouldContain(@"""size"": 10");
        }
        [TestMethod]
        public void SearchQuery_Contain_Default_Sort_Query_When_No_Sort_Present()
        {
            var searchOption = new SearchOption
            {
                Brand = "10",
                Category = "11,12",
                MaxPrice = 100,
                MinPrice = 10,
                DateRange = 2,
                Query = "Air"
            };
            var queryBuilder = new SearchQueryBuilder();
            var query = queryBuilder.GetQuery(searchOption);
            query.ShouldContain(@"""publishedon""");
            query.ShouldContain(@"""order"": ""desc""");
        }

        [TestMethod]
        public void SearchQuery_Contain_Price_Sort_In_Desc_Order_When_Sort_Desc_Present()
        {
            var searchOption = new SearchOption
            {
                Brand = "10",
                Category = "11,12",
                MaxPrice = 100,
                MinPrice = 10,
                DateRange = 2,
                Query = "Air",
                Sort = "price-desc"
            };
            var queryBuilder = new SearchQueryBuilder();
            var query = queryBuilder.GetQuery(searchOption);
            query.ShouldContain(@"""price""");
            query.ShouldContain(@"""order"": ""desc""");
        }

        [TestMethod]
        public void SearchQuery_Contain_Price_Sort_In_Asc_Order_When_Sort_Present()
        {
            var searchOption = new SearchOption
            {
                Brand = "10",
                Category = "11,12",
                MaxPrice = 100,
                MinPrice = 10,
                DateRange = 2,
                Query = "Air",
                Sort = "price-asc"
            };
            var queryBuilder = new SearchQueryBuilder();
            var query = queryBuilder.GetQuery(searchOption);
            query.ShouldContain(@"""price""");
            query.ShouldContain(@"""order"": ""asc""");
        }

        [TestMethod]
        public void SearchQuery_Contain_Price_Range_When_Price_Range_Present()
        {
            var searchOption = new SearchOption
            {
                MaxPrice = 100,
                MinPrice = 10,
                DateRange = null,
                Sort = "price-asc"
            };
            var queryBuilder = new SearchQueryBuilder();
            var query = queryBuilder.GetQuery(searchOption);
            query.ShouldContain(@"""range""");
            query.ShouldContain(@"""price""");
            query.ShouldContain(@"""lte"": 100");
            query.ShouldContain(@"""gte"": 10");
        }

        [TestMethod]
        public void SearchQuery_Contain_DateRange_When_Date_Range_Present()
        {
            var searchOption = new SearchOption
            {
                MaxPrice = null,
                MinPrice = null,
                DateRange = 2,
                Sort = "price-asc"
            };
            var queryBuilder = new SearchQueryBuilder();
            var query = queryBuilder.GetQuery(searchOption);
            query.ShouldContain(@"""range""");
            query.ShouldContain(@"""publishedon""");
            query.ShouldNotContain(@"""lte""");
            query.ShouldContain(@"""gte""");
        }

        [TestMethod]
        public void SearchQuery_Contain_MultiplrRange_When_Price_And_DateRange_Present()
        {
            var searchOption = new SearchOption
            {
                Brand = "10",
                Category = "11,12",
                MaxPrice = 1000,
                MinPrice = 10,
                DateRange = 2,
                Query = "Air",
                Sort = "price-asc"
            };
            var queryBuilder = new SearchQueryBuilder();
            var query = queryBuilder.GetQuery(searchOption);
            var rangeCount= Regex.Matches(query, @"""range""").Count;
            rangeCount.ShouldBe(2);        }

        [TestMethod]
        public void SearchQuery_Contain_Name_Field_When_Keyword_Present()
        {
            var searchOption = new SearchOption
            {
                Brand = "10",
                MaxPrice = 1000,
                MinPrice = 10,
                DateRange = 2,
                Query = "Air",
                Sort = "price-asc"
            };
            var queryBuilder = new SearchQueryBuilder();
            var query = queryBuilder.GetQuery(searchOption);
            query.ShouldContain(@"""match_phrase""");
            query.ShouldContain(@"""name"": ""Air""");
        }

        [TestMethod]
        public void SearchQuery_Contain_CategoryIds_Field_When_CategoryIds_Present()
        {
            var searchOption = new SearchOption
            {
                Category ="11,123",
                MaxPrice = 1000,
                MinPrice = 10,
                DateRange = 2,
                Query = "Air",
                Sort = "price-asc"
            };
            var queryBuilder = new SearchQueryBuilder();
            var query = queryBuilder.GetQuery(searchOption);
            query.ShouldContain(@"""should""");
            query.ShouldContain(@"""terms""");
            query.ShouldContain("\"categoryid\": [\r\n              \"11\",\r\n              \"123\"\r\n            ]\r\n");
            query.ShouldContain("\"categoryparentid\": [\r\n              \"11\",\r\n              \"123\"\r\n            ]\r\n");
        }

        [TestMethod]
        public void SearchQuery_Contain_Single_Category_Field_When_Single_CategoryId_Present()
        {
            var searchOption = new SearchOption
            {
                Category = "11",
                MaxPrice = 1000,
                MinPrice = 10,
                DateRange = 2,
                Query = "Air",
                Sort = "price-asc"
            };
            var queryBuilder = new SearchQueryBuilder();
            var query = queryBuilder.GetQuery(searchOption);
            query.ShouldContain(@"""should""");
            query.ShouldContain(@"""terms""");
            query.ShouldContain("\"categoryid\": [\r\n              \"11\"\r\n            ]\r\n");
            query.ShouldContain("\"categoryparentid\": [\r\n              \"11\"\r\n            ]\r\n");

        }

        [TestMethod]
        public void SearchQuery_Contain_Multiple_BrandId_Field_When_Multiple_BrandId_Present()
        {
            var searchOption = new SearchOption
            {
                Brand = "11,13,15,16",
                MaxPrice = 1000,
                MinPrice = 10,
                DateRange = 2,
                Query = "Air",
                Sort = "price-asc"
            };
            var queryBuilder = new SearchQueryBuilder();
            var query = queryBuilder.GetQuery(searchOption);
            query.ShouldContain(@"""terms""");
            query.ShouldContain("\"brandid\": [\r\n              \"11\",\r\n              \"13\",\r\n              \"15\",\r\n              \"16\"\r\n            ]\r\n");
        }
        [TestMethod]
        public void SearchQuery_Contain_Single_BrandId_Field_When_Single_BrandId_Present()
        {
            var searchOption = new SearchOption
            {
                Brand = "11",
                MaxPrice = 1000,
                MinPrice = 10,
                DateRange = 2,
                Query = "Air",
                Sort = "price-asc"
            };
            var queryBuilder = new SearchQueryBuilder();
            var query = queryBuilder.GetQuery(searchOption);
            query.ShouldContain(@"""terms""");
            query.ShouldContain("\"brandid\": [\r\n              \"11\"\r\n            ]\r\n");
        }

        [TestMethod]
        public void SearchQuery_Contain_Aggregation_Query()
        {
            var searchOption = new SearchOption
            {
                Brand = "11",
                MaxPrice = 1000,
                MinPrice = 10,
                DateRange = 2,
                Query = "Air",
                Sort = "price-asc"
            };
            var queryBuilder = new SearchQueryBuilder();
            var query = queryBuilder.GetQuery(searchOption);
            query.ShouldContain(@"""aggs""");
            var fieldCount = Regex.Matches(query, "field").Count;
            fieldCount.ShouldBe(2);
        }

        [TestMethod]
        public void SearchQuery_Contain_All_Query()
        {
            var searchOption = new SearchOption
            {
                Brand = "14",
                Category= "13606",
                MaxPrice = 10000,
                MinPrice = 10,
                DateRange = 36,
                Query = "Air",
                Sort = "price-asc"
            };
            var queryBuilder = new SearchQueryBuilder();
            var query = queryBuilder.GetQuery(searchOption);
            query.ShouldContain(@"""aggs""");
            query.ShouldContain(@"""range""");
            query.ShouldContain(@"""terms""");
            query.ShouldContain(@"""match_phrase""");
            query.ShouldContain(@"""from""");
            query.ShouldContain(@"""lte""");
            query.ShouldContain(@"""gte""");
            var fieldCount = Regex.Matches(query, "field").Count;
            fieldCount.ShouldBe(2);
        }




        [TestMethod]
        public void SearchQuery_Contain_Default_Query_When_No_Filter_Present()
        {
            var searchOption = new SearchOption
            {                
            };
            var queryBuilder = new SearchQueryBuilder();
            var query = queryBuilder.GetQuery(searchOption);
            query.ShouldContain(@"""match_all"": {}");
        }

    }
}
