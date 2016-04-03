using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RequestProcessingService
{
    public class GridFilter : IFilter
    {
        public GridFilter()
        {
            Filters = new List<GridFilter>();
        }

        public String Logic { get; set; }
        public String Field { get; set; }
        public String Operator { get; set; }
        public String Value { get; set; }
        public IList<GridFilter> Filters { get; set; }
    }
}
