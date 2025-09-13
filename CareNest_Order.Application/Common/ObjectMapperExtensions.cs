using System.Linq.Expressions;
using System.Reflection;

namespace CareNest_Order.Application.Common
{
    public static class ObjectMapperExtensions
    {
        // Map từng phần tử trong list nguồn sang list đích
        public static List<TDestination> MapListPropertiesTo<TSource, TDestination>(this IEnumerable<TSource> sourceList)
            where TDestination : new()
        {
            if (sourceList == null)
                throw new ArgumentNullException(nameof(sourceList));

            var result = new List<TDestination>();

            foreach (var source in sourceList)
            {
                var destination = new TDestination();
                source.MapPropertiesTo(destination);
                result.Add(destination);
            }

            return result;
        }

        // Hàm map từng object như ở trên
        public static void MapPropertiesTo<TSource, TDestination>(this TSource source, TDestination destination)
        {
            if (source == null || destination == null)
                throw new ArgumentNullException("Source or Destination is null");

            var sourceProps = typeof(TSource).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            var destProps = typeof(TDestination).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

            foreach (var sourceProp in sourceProps)
            {
                var destProp = Array.Find(destProps, p =>
                    p.Name == sourceProp.Name &&
                    p.CanWrite &&
                    p.PropertyType.IsAssignableFrom(sourceProp.PropertyType));

                if (destProp != null)
                {
                    var value = sourceProp.GetValue(source);
                    destProp.SetValue(destination, value);
                }
            }
        }

        /// <summary>
        /// Tạo biểu thức ánh xạ giữa hai kiểu
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TDestination"></typeparam>
        /// <returns></returns>
        public static Expression<Func<TSource, TDestination>> CreateMapExpression<TSource, TDestination>()
        where TDestination : new()
        {
            var sourceType = typeof(TSource);
            var destinationType = typeof(TDestination);

            var parameter = Expression.Parameter(sourceType, "src");

            var bindings = new List<MemberBinding>();

            var destinationProperties = destinationType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                        .Where(p => p.CanWrite);

            foreach (var destProp in destinationProperties)
            {
                // Tìm property trong source trùng tên và kiểu tương thích
                var sourceProp = sourceType.GetProperty(destProp.Name, BindingFlags.Public | BindingFlags.Instance);
                if (sourceProp != null && destProp.PropertyType.IsAssignableFrom(sourceProp.PropertyType))
                {
                    var sourcePropAccess = Expression.Property(parameter, sourceProp);
                    var binding = Expression.Bind(destProp, sourcePropAccess);
                    bindings.Add(binding);
                }
            }

            var body = Expression.MemberInit(Expression.New(destinationType), bindings);

            return Expression.Lambda<Func<TSource, TDestination>>(body, parameter);
        }
    }
}
