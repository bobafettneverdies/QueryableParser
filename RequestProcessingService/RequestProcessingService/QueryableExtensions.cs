using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RequestProcessingService.Processing;

namespace RequestProcessingService
{
    public static class QueryableExtensions
    {
        public static Response ProcessGridRequest<T>(this IQueryable<T> expr, IRequest request)
        {
            return new GridRequestProcessing<T>().Process(expr, request);
        }

        public static Response ProcessTypeaheadRequest<T>(this IQueryable<T> expr, IRequest request)
        {
            return new TypeaheadRequestProcessing<T>().Process(expr, request);
        }
    }
}
