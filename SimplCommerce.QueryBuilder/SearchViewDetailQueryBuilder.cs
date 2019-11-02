
using Antlr4.StringTemplate;
using Newtonsoft.Json.Linq;

namespace SimplCommerce.QueryBuilder
{
    public class SearchViewDetailQueryBuilder : ISearchViewDetailQueryBuilder
    {
        const string QueryTemplate = @"{
                                        ""query"": {
                                              ""bool"":{
                                                ""must"":[
                                                       {
                                                        ""match_phrase"": 
                                                            {
                                                            ""<idfieldname>"":""<fieldvalue>""
                                                            }
                                                        },                                                        
                                                         {
                                                           ""match_phrase"": {
                                                                ""ispublished"": ""true""
                                                            }
                                                        }
                                                    ]                                               
                                             }
                                          }
                                       
                                     }";
       
        public string GetQuery(long productId)
        {
            var queryTemplate = new Template(QueryTemplate);
            queryTemplate.Add("idfieldname", "_id");
            queryTemplate.Add("fieldvalue", productId);
            var queryToBeUsed = queryTemplate.Render();
            queryToBeUsed = JToken.Parse(queryToBeUsed).ToString();
            return queryToBeUsed;
        }
    }
}
