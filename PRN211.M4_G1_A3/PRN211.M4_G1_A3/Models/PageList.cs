using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project_WebMVC.Models
{
    public class PageList<T> : List<T>
    {
        public int PageIndex { get; private set; }
        public int TotalPages { get; private set; }

        public bool HasPreviousPage
        {
            get
            {
                return PageIndex > 1;
            }
        }
        public bool HasNextPage
        {
            get
            {
                return PageIndex < TotalPages;
            }
        }
        public PageList(List<T> items, int count, int pageIndex, int pageSize)
        {
            PageIndex = pageIndex;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            AddRange(items);
        }

        public static async Task<PageList<T>> CreateAsync(IQueryable<T> source, int pageIndex, int pageSize)
        {
            var count = source.Count();
            var items = await source.Skip((pageIndex - 1) * pageSize)
                .Take(pageSize).ToListAsync();
            return new PageList<T>(items, count, pageIndex, pageSize);
        }
    }
}
