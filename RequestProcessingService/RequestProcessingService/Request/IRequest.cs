using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RequestProcessingService
{
    public interface IRequest
    {
        int Take { get; set; }
        int Page { get; set; }
        IList<Sort> Sort { get; set; }
        IFilter Filter { get; set; }
    }
}
