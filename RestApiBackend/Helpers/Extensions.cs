using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace RestApiBackend.Helpers
{
    public static class Extensions
    {
        public static void AddPaginationHeaders(this HttpResponse response, int pageNumber, int pageSize, int totalPages, int totalCount)
        {
            var paginationHeader = new { pageNumber, pageSize, totalPages, totalCount };

            // expone la cabecera
            response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationHeader));
            response.Headers.Add("Access-Control-Expose-Headers", "Pagination");
        }

        public static IDictionary<TKey, TValue> NullIfEmpty<TKey, TValue>(this IDictionary<TKey, TValue> dictionary)
        {
            if (dictionary == null || !dictionary.Any())
            {
                return null;
            }
            return dictionary;
        }

        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (T element in source)
            {
                action(element);
            }
            return source;
        }

        /// <summary>
        /// Filtra la consulta
        /// </summary>
        /// <param name="value">Valor a filtra</param>
        /// <param name="filterProps">Lista de propiedades filtrables de la entidad</param>
        /// <typeparam name="T">Tipo de la entidad</typeparam>
        public static IQueryable<T> Filter<T>(this IQueryable<T> query, string value, List<string> filterProps)
        {
            if (string.IsNullOrEmpty(value) || filterProps == null)
                return query;

            var parameter = Expression.Parameter(typeof(T), "e");
            var constant = Expression.Constant(value.ToLower());
            var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
            var toLowerMethod = typeof(string).GetMethod("ToLower", System.Type.EmptyTypes);
            var members = new MemberExpression[filterProps.Count()];

            for (int i = 0; i < filterProps.Count(); i++)
            {
                if (filterProps[i].Contains('.'))
                {   // el filtro es una propiedad de una entidad anidada
                    // ej. u => u.Rol.Nombre
                    Expression nestedMember = parameter;
                    foreach (var prop in filterProps[i].Split('.'))
                    {
                        nestedMember = Expression.PropertyOrField(nestedMember, prop);
                    }
                    members[i] = (MemberExpression)nestedMember;
                }
                else
                {
                    // el filtro es una propiedad de la entidad
                    // ej. u => u.Username
                    members[i] = Expression.Property(parameter, filterProps[i]);
                }
            }

            Expression searchExp = null;
            foreach (var member in members)
            {
                // e => e.Member != null
                var notNullExp = Expression.NotEqual(member, Expression.Constant(null));
                // e => e.Member.ToLower() 
                var toLowerExp = Expression.Call(member, toLowerMethod);
                // e => e.Member.Contains(value)
                var containsExp = Expression.Call(toLowerExp, containsMethod, constant);
                // e => e.Member != null && e.Member.Contains(value)
                var filterExpression = Expression.AndAlso(notNullExp, containsExp);

                searchExp = searchExp == null ? (Expression)filterExpression : Expression.OrElse(searchExp, filterExpression);
            }

            var predicate = Expression.Lambda<Func<T, bool>>(searchExp, parameter);

            return query.Where(predicate);
        }

        /// <summary>
        /// Aplica ordenamiento a la consulta
        /// </summary>
        /// <param name="orderByProperties">Lista de columnas a ordernar con el formato columna:[asc|desc]</param>
        /// <typeparam name="T">Tipo EntityBase</typeparam>
        public static IQueryable<T> Sort<T>(this IQueryable<T> query, List<string> orderByProperties) where T : class
        {
            if (orderByProperties.Count() == 0)
                orderByProperties.Add(Constants.DEFAULT_ODERING);

            var type = typeof(T);
            var parameter = Expression.Parameter(type, "p");

            for (var i = 0; i < orderByProperties.Count(); i++)
            {
                var splitedOrder = orderByProperties[i].Split(':');
                var columnName = splitedOrder[0];
                var orderType = splitedOrder.Count() > 1 ? splitedOrder[1] : "asc";
                var member = columnName.Split('.')
                    .Aggregate((Expression)parameter, Expression.PropertyOrField);
                var expression = Expression.Lambda(member, parameter);
                var orderMethod = "";

                if (i == 0)
                {
                    // la primera vez es orderBy
                    orderMethod = orderType == "asc" ? "OrderBy" : "OrderByDescending";
                }
                else
                {
                    // luego es ThenBy
                    orderMethod = orderType == "asc" ? "ThenBy" : "ThenByDescending";
                }

                Type[] types = new Type[] { type, expression.ReturnType };

                // OrderBy*(x => x.Cassette) or Order*(x => x.SlotNumber)
                // ThenBy*(x => x.Cassette) or ThenBy*(x => x.SlotNumber)
                var callExpression = Expression.Call(typeof(Queryable), orderMethod, types,
                    query.Expression, expression);

                query = query.Provider.CreateQuery<T>(callExpression);
            }

            return query;
        }
    }
}