using System;
using System.Linq.Expressions;

namespace RequestProcessingService.Processing
{
    public class TypeaheadRequestProcessing<TDocumentDTO> : BaseRequestProcessing<TDocumentDTO>
    {
        protected override BinaryExpression ParseFilter(ParameterExpression parameterExpression, IFilter filter)
        {
            var typeaheadFilter = filter as TypeaheadFilter;

            if (typeaheadFilter == null)
            {
                throw new Exception("Не удалось преобразовать объект к типу TypeaheadFilter");
            }

            if (String.IsNullOrWhiteSpace(typeaheadFilter.Value))
            {
                return null;
            } 
            
            var result = Expression.Equal(Expression.Constant(true), Expression.Constant(false));

            var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
            var toLowerMethod = typeof(string).GetMethod("ToLower", Type.EmptyTypes);

            //todo: отработать полнотекстовый поиск для типов полей помимо String
            foreach (var field in typeaheadFilter.Fields)
            {
                var propertyInfo = typeof(TDocumentDTO).GetProperty(field);
                var propertyType = propertyInfo.PropertyType;

                var left = Expression.Property(parameterExpression, propertyInfo);
                var right = Expression.Constant(typeaheadFilter.Value, propertyType);

                var fieldExpr = Expression.AndAlso(
                    Expression.NotEqual(left, Expression.Constant(null, left.Type)),
                    Expression.Call(Expression.Call(left, toLowerMethod), containsMethod,
                        Expression.Call(right, toLowerMethod))
                    );

                result = Expression.OrElse(result, fieldExpr);
            }

            return result;
        }
    }
}
