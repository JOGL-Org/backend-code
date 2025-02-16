using Jogl.Server.DB.Context;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace Jogl.Server.DB
{
    public class BaseQuery<T> : IBaseQuery<T>
    {
        protected readonly IConfiguration _configuration;
        protected readonly IOperationContext _context;

        protected IAggregateFluent<T> _query;

        public BaseQuery(IConfiguration configuration, IOperationContext context, IAggregateFluent<T> query)
        {
            _configuration = configuration;
            _context = context;
            _query = query;
        }

        public IBaseQuery<T> Filter(Expression<Func<T, bool>> filter)
        {
            if (filter != null)
                _query = _query.Match(filter);

            return this;
        }

        public IBaseQuery<T> Page(int page, int pageSize)
        {
            _query = _query.Skip((page - 1) * pageSize).Limit(pageSize);
            return this;
        }

        public List<T> ToList()
        {
            return _query.ToList();
        }

        public long Count()
        {
            return _query.Count().SingleOrDefault()?.Count ?? 0;
        }

        public bool Any()
        {
            return _query.Any();
        }
    }
}