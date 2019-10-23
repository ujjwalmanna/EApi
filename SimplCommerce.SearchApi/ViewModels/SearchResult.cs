using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using SimplCommerce.Module.Catalog.ViewModels;

namespace SimplCommerce.SearchApi.ViewModels
{
    public class SearchResult
    {
        public long BrandId { get; set; }

        public string BrandName { get; set; }

        public string BrandSlug { get; set; }

        public int TotalProduct { get; set; }

        public IList<ProductThumbnail> Products { get; set; } = new List<ProductThumbnail>();

        public IList<FilterCategory> Categories { get; set; } = new List<FilterCategory>();
        public FilterOption FilterOption { get; set; }

        public SearchOption CurrentSearchOption { get; set; }

        public IList<SelectListItem> AvailableSortOptions => new List<SelectListItem> {
            new SelectListItem { Text = "Date", Value = "date-desc" },
            new SelectListItem { Text = "Price: Low to High", Value = "price-asc" },
            new SelectListItem { Text = "Price: High to Low", Value = "price-desc" }
        };

        public IList<SelectListItem> NumberPagesOptions => new List<SelectListItem> {
            new SelectListItem { Text = "10", Value = "10" },
            new SelectListItem { Text = "20", Value = "20" },
            new SelectListItem { Text = "30", Value = "30" },
           new SelectListItem { Text = "40", Value = "40" },
            new SelectListItem { Text = "50", Value = "50" },


        };
    }
}