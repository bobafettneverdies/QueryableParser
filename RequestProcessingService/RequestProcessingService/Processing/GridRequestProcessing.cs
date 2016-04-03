using System;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;

namespace RequestProcessingService.Processing
{
    public class GridRequestProcessing<TDocumentDTO> : BaseRequestProcessing<TDocumentDTO>
    {
        protected override BinaryExpression ParseFilter(ParameterExpression parameterExpression, IFilter filter)
        {
            var gridFilter = filter as GridFilter;

            if (gridFilter == null)
            {
                throw new Exception("Не удалось преобразовать объект к типу GridFilter");
            }

            if (gridFilter.Filters.Count == 0 && String.IsNullOrWhiteSpace(gridFilter.Value))
            {
                return null;
            }

            return ParseGridFilter(parameterExpression, gridFilter);
        }

        private BinaryExpression ParseGridFilter(ParameterExpression parameterExpression,
            GridFilter gridFilterDescriptor)
        {
            //Если фильтр сложный, то рекурсивно вызываем ParseFilter для всех дочерних фильтров
            if (gridFilterDescriptor.Filters.Any())
            {
                if (gridFilterDescriptor.Logic.ToLower() == "and")
                {
                    var andResult = Expression.Equal(Expression.Constant(true), Expression.Constant(true));
                    return gridFilterDescriptor.Filters.Aggregate(andResult,
                        (current, filter) =>
                            Expression.AndAlso(current, ParseGridFilter(parameterExpression, filter)));
                }
                else
                {
                    var orResult = Expression.Equal(Expression.Constant(true), Expression.Constant(false));
                    return gridFilterDescriptor.Filters.Aggregate(orResult,
                        (current, filter) =>
                            Expression.OrElse(current, ParseGridFilter(parameterExpression, filter)));
                }
            }

            var propertyInfo = typeof(TDocumentDTO).GetProperty(gridFilterDescriptor.Field);
            var propertyType = propertyInfo.PropertyType;

            //Конвертируем значение фильтра из строки в нужный тип данных
            var filterValue = TypeDescriptor.GetConverter(propertyType).ConvertFrom(gridFilterDescriptor.Value);

            var left = Expression.Property(parameterExpression, propertyInfo);
            var right = Expression.Constant(filterValue, propertyType);

            //в зависимости от типа оператора генерируем Expression
            return ExpressionResolver(left, right, gridFilterDescriptor.Operator);
        }

        private BinaryExpression ExpressionResolver(Expression left, Expression right, String filterOperator)
        {
            //Методы, необходимые для фильрации по строкам
            var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
            var startsWithMethod = typeof(string).GetMethod("StartsWith", new[] { typeof(string) });
            var endsWithMethod = typeof(string).GetMethod("EndsWith", new[] { typeof(string) });
            var stringEmpty = Expression.Constant(String.Empty, typeof(string));

            //Приведение строковых полей к нижнему регистру, необходимо для нерегистрозависимого поиска по строковым полям
            var toLowerMethod = typeof(string).GetMethod("ToLower", Type.EmptyTypes);
            var leftLower = left.Type == typeof(string) ? Expression.Call(left, toLowerMethod) : null;
            var rightLower = right.Type == typeof(string) ? Expression.Call(right, toLowerMethod) : null;

            //Методы, необходимые для фромализации дат
            var addHoursMethod = typeof(DateTime).GetMethod("AddHours");
            var addMinutesMethod = typeof(DateTime).GetMethod("AddMinutes");
            var maxHours = Expression.Constant((double)23);
            var maxMinutes = Expression.Constant((double)59);

            switch (filterOperator.ToLower())
            {
                case "eq":
                    return Expression.Equal(left, right);
                case "neq":
                    return Expression.NotEqual(left, right);
                case "gt":
                    if (right.Type != typeof(DateTime))
                    {
                        return Expression.GreaterThan(left, right);
                    }
                    else
                    {
                        return Expression.GreaterThan(left,
                            Expression.Call(Expression.Call(right, addHoursMethod, maxHours), addMinutesMethod,
                                maxMinutes));
                    }
                case "lt":
                    return Expression.LessThan(left, right);
                case "gte":
                    return Expression.GreaterThanOrEqual(left, right);
                case "lte":
                    if (right.Type != typeof(DateTime))
                    {
                        return Expression.LessThanOrEqual(left, right);
                    }
                    else
                    {
                        return Expression.LessThanOrEqual(left,
                            Expression.Call(Expression.Call(right, addHoursMethod, maxHours), addMinutesMethod,
                                maxMinutes));
                    }
                case "contains":
                    return
                        Expression.OrElse(
                            Expression.AndAlso(Expression.NotEqual(left, Expression.Constant(null, left.Type)),
                                Expression.Call(leftLower, containsMethod, rightLower)),
                            Expression.Equal(right, stringEmpty)
                            );
                case "doesnotcontain":
                    return
                        Expression.OrElse(
                            Expression.AndAlso(Expression.NotEqual(left, Expression.Constant(null, left.Type)),
                                Expression.Not(Expression.Call(leftLower, containsMethod, rightLower))),
                            Expression.Equal(left, Expression.Constant(null, left.Type))
                            );
                case "startswith":
                    return
                        Expression.OrElse(
                            Expression.AndAlso(Expression.NotEqual(left, Expression.Constant(null, left.Type)),
                                Expression.Call(leftLower, startsWithMethod, rightLower)),
                            Expression.Equal(right, stringEmpty)
                            );
                case "endswith":
                    return
                        Expression.OrElse(
                            Expression.AndAlso(Expression.NotEqual(left, Expression.Constant(null, left.Type)),
                                Expression.Call(leftLower, endsWithMethod, rightLower)),
                            Expression.Equal(right, stringEmpty)
                            );
                default:
                    return Expression.Equal(left, right);
            }
        }
    }
}
