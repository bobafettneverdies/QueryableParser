using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;

namespace RequestProcessingService.Processing
{
    public abstract class BaseRequestProcessing<TDocumentDTO>
    {
        public Response Process(IQueryable<TDocumentDTO> expr, IRequest request)
        {
            if (request == null)
                return new Response();

            try
            {
                if (request.Filter != null)
                {
                    expr = ApplyFilters(expr, request.Filter);
                }

                expr = request.Sort
                    .Where(sort => !String.IsNullOrWhiteSpace(sort.Field))
                    .Aggregate(expr,
                        (current, sort) =>
                            ApplySort(current, sort, sort == request.Sort.First()));

            }
            catch (Exception e)
            {
                return new Response
                {
                    Success = false,
                    ErrorList = new List<string> { e.Message }
                };
            }
            
            var result = new Response()
            {
                Total = expr.Count(),
                Data = expr.Skip(request.Take * (request.Page - 1)).Take(request.Take).ToList(),
                Success = true
            };

            return result;
        }

        protected virtual IQueryable<TDocumentDTO> ApplyFilters(IQueryable<TDocumentDTO> expr, IFilter filter)
        {
            var type = typeof(TDocumentDTO);
            var arg = Expression.Parameter(type, "x");

            var filterExpression = ParseFilter(arg, filter);

            if (filterExpression == null)
            {
                return expr;
            }

            var delegateType = typeof(Func<,>).MakeGenericType(typeof(TDocumentDTO), typeof(bool));
            var lambda = Expression.Lambda(delegateType, filterExpression, arg);

            var result = (IQueryable<TDocumentDTO>)typeof(Queryable).GetMethods().Single(
                method => method.Name == "Where"
                        && method.IsGenericMethodDefinition
                        && method.GetGenericArguments().Length == 1
                        && method.GetParameters().Length == 2
                        //todo: придумать что-нибудь более наглядное, чем этот признак
                        && ((method.GetParameters()[1]).ParameterType).GenericTypeArguments[0].GenericTypeArguments.Count() == 2)
                .MakeGenericMethod(type)
                .Invoke(null, new object[] { expr, lambda });

            return result;
        }

        protected virtual BinaryExpression ParseFilter(ParameterExpression parameterExpression, IFilter filter)
        {
            throw new NotImplementedException();
        }

        protected virtual IQueryable<TDocumentDTO> ApplySort(IQueryable<TDocumentDTO> expr, Sort sortDescriptor, bool isFirstSort = true)
        {
            var type = typeof(TDocumentDTO);
            var arg = Expression.Parameter(type, "x");

            var propertyInfo = type.GetProperty(sortDescriptor.Member);

            var propertyExpression = Expression.Property(arg, propertyInfo);
            var propertyType = propertyInfo.PropertyType;

            var delegateType = typeof(Func<,>).MakeGenericType(typeof(TDocumentDTO), propertyType);
            var lambda = Expression.Lambda(delegateType, propertyExpression, arg);

            var methodName = isFirstSort
                ? (sortDescriptor.SortDirection == ListSortDirection.Ascending ? "OrderBy" : "OrderByDescending")
                : (sortDescriptor.SortDirection == ListSortDirection.Ascending ? "ThenBy" : "ThenByDescending");

            var result = (IQueryable<TDocumentDTO>)typeof(Queryable).GetMethods().Single(
                method => method.Name == methodName
                          && method.IsGenericMethodDefinition
                          && method.GetGenericArguments().Length == 2
                          && method.GetParameters().Length == 2)
                .MakeGenericMethod(type, propertyType)
                .Invoke(null, new object[] { expr, lambda });

            return result;
        }
    }
}
