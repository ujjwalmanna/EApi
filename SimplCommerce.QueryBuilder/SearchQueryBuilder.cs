using System;
using System.Collections.Generic;
using System.Text;
using SimplCommerce.Module.Catalog.ViewModels;
using Antlr4.StringTemplate;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace SimplCommerce.QueryBuilder
{
    public class SearchQueryBuilder : ISearchQueryBuilder
    {
        const string PaginationFormat = @"""from"": <pagestartvalue>,""size"": <pagesize>";

        const string SortingTemplate = @"""sort"": [
            {
            ""<fieldname>"": { 
                    ""order"": ""<sortingorder>""
                } 
            }
            ]";

        const string AggregateQueryTemplate = @"""aggs"":{
            <aggregationquery>
            }";
        const string DefaultQueryTemplate = @"""match_all"":{}";

        const string BoolQueryTemplate = @"""bool"":{
                                            <mustquery>
                                            ,<shouldquery>
                                         }";

        const string MustQueryTemplate = @"""must"":[
                   <mustquery>
                ]";
        const string ShouldQueryTemplate = @"""should"":[
                   <shouldquery>
                ]";

        const string NumberRangeQueryTemplate = @"{
            ""range"": 
                {
                ""<fieldname>"": { 
                        ""lte"": <endnumberrange>,
                        ""gte"": <startnumberrange>
                     
                    }
                }
            },";
        const string DateRangeQueryTemplate = @"{
            ""range"": 
                {
                ""<fieldname>"": {                         
                        ""gte"": ""<daterange>""
                     
                    }
                }
            },";
        const string MatchPhraseQueryTemplate= @"{
            ""match_phrase"": 
                {
                ""<fieldname>"":""<fieldvalue>""
                }
            },";
        const string TermsQueryTemplate = @"{
            ""terms"": 
                {
                ""<fieldname>"":[<fieldvalue>]
                }
            },";
        const string AggregateTermQueryWithOutSizeTemplate = @"
            ""<aggregatename>"": 
                {
                ""terms"":{""field"":""<fieldname>""}
                }
            ,";
        const string AggregateTermQueryWithSizeTemplate = @"        
            ""<aggregatename>"": 
                {
                ""terms"":{""field"":""<fieldname>"",""size"":<fieldsize>}
                }
            ,";

        const string QueryTemplate = @"{<paginationquery>
                                        ,<sortingquery>
                                        ,""query"": {
                                                <innerquery>
                                          }
                                        ,<aggregationquery>
                                     }";
        public string GetQuery(SearchOption option) {

            var queryTemplate = new Template(QueryTemplate);
            queryTemplate.Add("paginationquery", GetPagingQuery(option));
            queryTemplate.Add("sortingquery", GetSortQuery(option));
            queryTemplate.Add("innerquery", GetInnerQuery(option));
            queryTemplate.Add("aggregationquery", GetAggregationQuery(option));
            var queryToBeUsed = queryTemplate.Render();
            queryToBeUsed = JToken.Parse(queryToBeUsed).ToString();
            return queryToBeUsed;
        }

        internal string GetPagingQuery(SearchOption option)
        {
            var pageTemplate = new Template(PaginationFormat);
            pageTemplate.Add("pagestartvalue", option.Page);
            pageTemplate.Add("pagesize", option.PageSize==0?10:option.PageSize);
            var pageQuery = pageTemplate.Render();
            return pageQuery;
        }
        internal string GetSortQuery(SearchOption option)
        {
            var sortingTemplate = new Template(SortingTemplate);
            string sortfieldName = "publishedon";
            string sortOrder = "desc";
            if (option.Sort == "price-desc") {
                sortfieldName = "price";
                sortOrder = "desc";
            }
            else if(option.Sort == "price-asc") {
                sortfieldName = "price";
                sortOrder = "asc";
            }
            sortingTemplate.Add("fieldname", sortfieldName);
            sortingTemplate.Add("sortingorder",sortOrder);
            var sortingQuery = sortingTemplate.Render();
            return sortingQuery;
        }
        internal string GetInnerQuery(SearchOption option)
        {
            var innerQuery = string.Empty;
            var mustQuery = GetMustQueryContent(option);
            var shouldQuery = GetShouldQueryContent(option);
            if (!string.IsNullOrEmpty(mustQuery))
            {
                var boolQueryTemplate = new Template(BoolQueryTemplate);
                boolQueryTemplate.Add("mustquery", mustQuery.Trim(','));
                boolQueryTemplate.Add("shouldquery", shouldQuery.Trim(','));
                innerQuery = boolQueryTemplate.Render();
            }
            else
            {
                var defaultQueryTemplate = new Template(DefaultQueryTemplate);
                innerQuery = defaultQueryTemplate.Render();
            }
            return innerQuery;
        }
        
        internal string GetAggregationQuery(SearchOption option)
        {
            string aggregationQuery = string.Empty;
            var categoryAggregationQueryTemplate = new Template(AggregateTermQueryWithOutSizeTemplate);
            categoryAggregationQueryTemplate.Add("aggregatename", "category");
            categoryAggregationQueryTemplate.Add("fieldname", "categoryid");
            aggregationQuery += categoryAggregationQueryTemplate.Render();
            var brandAggregationQueryTemplate = new Template(AggregateTermQueryWithSizeTemplate);
            brandAggregationQueryTemplate.Add("aggregatename", "brand");
            brandAggregationQueryTemplate.Add("fieldname", "brandid");
            brandAggregationQueryTemplate.Add("fieldsize", "10");
            aggregationQuery += brandAggregationQueryTemplate.Render();
            aggregationQuery = aggregationQuery.Trim(',');

            var aggregationTemplate = new Template(AggregateQueryTemplate);
            aggregationTemplate.Add("aggregationquery", aggregationQuery);
            var finalAggregationQuery = aggregationTemplate.Render();
            return finalAggregationQuery;

        }


        internal string GetMustQueryContent(SearchOption option)
        {
            var mustQuery = GeRangeQueryContent(option) + GetMatchPhraseQuery(option)+GetTermsQuery(option);
            if (string.IsNullOrEmpty(mustQuery))
                return mustQuery;
            var mustQueryTemplate = new Template(MustQueryTemplate);
            mustQueryTemplate.Add("mustquery", mustQuery.Trim(','));
            return mustQueryTemplate.Render();
        }

        internal string GetShouldQueryContent(SearchOption option)
        {
            var shouldQuery = GetTermsQueryForCategory(option);
            if (string.IsNullOrEmpty(shouldQuery))
                return shouldQuery;

            var shouldQueryTemplate = new Template(ShouldQueryTemplate);
            shouldQueryTemplate.Add("shouldquery", shouldQuery.Trim(','));
            return shouldQueryTemplate.Render();
        }

        internal string GeRangeQueryContent(SearchOption option)
        {
            var rangeQuery = NumberRangeQuery(option) + DateRangeQuery(option);
            return rangeQuery;
        }

        internal string NumberRangeQuery(SearchOption option)
        {
            var rangeQuery = "";
            if (option.MinPrice != null && option.MaxPrice != null && option.MaxPrice.Value>option.MinPrice.Value)
            {
                var rangeNumberTemplate = new Template(NumberRangeQueryTemplate);
                rangeNumberTemplate.Add("fieldname", "price");
                rangeNumberTemplate.Add("startnumberrange", option.MinPrice);
                rangeNumberTemplate.Add("endnumberrange", option.MaxPrice);
                rangeQuery += rangeNumberTemplate.Render();
            }
            return rangeQuery;
        }

        internal string DateRangeQuery(SearchOption option)
        {
            var rangeQuery = "";
            if (option.DateRange != null)
            {
                var rangeDaterTemplate = new Template(DateRangeQueryTemplate);
                var dateToBeUsed = DateTime.Today.AddMonths(-option.DateRange.Value).ToString("yyyy-MM-dd") + @"T23:59:59.999";
                rangeDaterTemplate.Add("fieldname", "publishedon");
                rangeDaterTemplate.Add("daterange", dateToBeUsed);
                rangeQuery += rangeDaterTemplate.Render();
            }
            return rangeQuery;
        }

        internal string GetMatchPhraseQuery(SearchOption option)
        {
            string matchPhraseQuery = string.Empty;
            if (!string.IsNullOrEmpty(option.Query?.Trim()))
            {
                var matchPhraseTemplate = new Template(MatchPhraseQueryTemplate);
                matchPhraseTemplate.Add("fieldname", "name");
                matchPhraseTemplate.Add("fieldvalue", option.Query);
                matchPhraseQuery = matchPhraseTemplate.Render();
            }
            return matchPhraseQuery;
        }

        internal string GetTermsQuery(SearchOption option)
        {
            string termsQuery = string.Empty;
           
            if (!string.IsNullOrEmpty(option.Brand?.Trim()))
            {
                List<string> brandIds = option.Brand.Split(',').Select(c => c).ToList();
                var brandIdToBeUsed = @"""" + brandIds.Aggregate((a, b) => $@"{a}""" + "," + $@"""{b}") + @"""";

                var termsQueryTemplate = new Template(TermsQueryTemplate);
                termsQueryTemplate.Add("fieldname", "brandid");
                termsQueryTemplate.Add("fieldvalue", brandIdToBeUsed);
                termsQuery += termsQueryTemplate.Render();
            }
            return termsQuery;
        }

        internal string GetTermsQueryForCategory(SearchOption option)
        {
            string termsQuery = string.Empty;
            if (!string.IsNullOrEmpty(option.Category?.Trim()))
            {
                List<string> categoryIds = option.Category.Split(',').Select(c => c).ToList();
                var catIdToBeUsed = @"""" + categoryIds.Aggregate((a, b) => $@"{a}""" + "," + $@"""{b}") + @"""";
                var termsQueryTemplate = new Template(TermsQueryTemplate);
                termsQueryTemplate.Add("fieldname", "categoryid");
                termsQueryTemplate.Add("fieldvalue", catIdToBeUsed);
                termsQuery += termsQueryTemplate.Render();
            }
            if (!string.IsNullOrEmpty(option.Category?.Trim()))
            {
                List<string> categoryIds = option.Category.Split(',').Select(c => c).ToList();
                var catIdToBeUsed = @"""" + categoryIds.Aggregate((a, b) => $@"{a}""" + "," + $@"""{b}") + @"""";
                var termsQueryTemplate = new Template(TermsQueryTemplate);
                termsQueryTemplate.Add("fieldname", "categoryparentid");
                termsQueryTemplate.Add("fieldvalue", catIdToBeUsed);
                termsQuery += termsQueryTemplate.Render();
            }          
            return termsQuery;
        }
    }
}
