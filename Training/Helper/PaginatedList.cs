using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Training.Helper
{
    public class PaginatedList<T> : List<T>
    {
        public int PageIndex { get; private set; }
        public int TotalPage { get; private set; }

        public PaginatedList(IEnumerable<T> items, int count, int pageIndex, int pageSize = 10)
        {
            PageIndex = pageIndex;
            TotalPage = (int) Math.Ceiling(count / (double) pageSize);
            
            this.AddRange(items);
        }

        public bool HasPreviousPage
        {
            get
            {
                return (PageIndex > 1);
            }
        }

        public bool HasNextPage
        {
            get { return (PageIndex < TotalPage); }
        }

        public static async Task<PaginatedList<T>> CreateAsync(IEnumerable<T> source, int pageIndex, int pageSize)
        {
            var count = source.Count();
            var items = source.Skip((pageIndex - 1) * pageSize).Take(pageSize);
            return new PaginatedList<T>(items,count,pageIndex,pageSize);
        }
    }
}