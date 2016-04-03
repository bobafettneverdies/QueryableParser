using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RequestProcessingService
{
    public class TypeaheadFilter : IFilter
    {
        public TypeaheadFilter()
        {
            Fields = new List<String>();
        }

        public String Value { get; set; }
        public IList<String> Fields { get; set; }
    }
}
