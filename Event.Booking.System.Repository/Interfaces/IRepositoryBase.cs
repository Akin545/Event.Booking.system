using Event.Booking.System.Core.Interface;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Event.Booking.System.Repository.Interfaces
{
    public interface IRepositoryBase<T> where T : IEntity
    {
        Task<Guid> AddAsync(T item);

        Task<T> GetAsync(Guid id);

        Task<T> GetAsync(Guid id, params string[] includes);

        Task<List<T>> ListAsync(int pageNumber);

        Task UpdateAsync(T item);

    }
}

