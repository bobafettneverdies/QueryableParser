using System;
using System.ComponentModel;

namespace RequestProcessingService
{
    public class Sort
    {
        public String Field { get; set; }
        public String Dir { get; set; }

        public String Member { get { return Field; } }
        public ListSortDirection SortDirection { get { return !String.IsNullOrEmpty(Dir) && Dir.ToLower() == "asc" ? ListSortDirection.Ascending : ListSortDirection.Descending; } }
    }
}
