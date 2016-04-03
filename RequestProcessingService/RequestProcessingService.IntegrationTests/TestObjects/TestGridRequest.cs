using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RequestProcessingService.IntegrationTests.TestObjects
{
    public class TestGridRequest : IRequest
    {
        public TestGridRequest()
        {
            Filter = new GridFilter();
            Sort = new List<Sort>();
        }

        public int Take { get; set; }

        public int Page { get; set; }

        public IList<Sort> Sort { get; set; }

        public IFilter Filter { get; set; }
    }
}
