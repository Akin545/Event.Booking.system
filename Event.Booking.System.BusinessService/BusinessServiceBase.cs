using Event.Booking.System.BusinessService.Interfaces;
using Event.Booking.System.BusinessService.Interfaces.Utilities;
using Event.Booking.System.Core.Interface;
using Event.Booking.System.Repository.Interfaces;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace Event.Booking.System.BusinessService
{
    public abstract class BusinessServiceBase<TEntity, TRepositoryManager> : IBusinessServiceBase<TEntity>
    where TEntity : IEntity, IEntityIdentity, new()
    where TRepositoryManager : IRepositoryBase<TEntity>
    {
        protected TEntity Entity { get; set; } = new TEntity();
        protected ILogger<TEntity> HealthLogger { get; set; }
        protected readonly TRepositoryManager RepositoryManager;

        protected readonly IGlobalDateTimeSettings GlobalDateTimeSettings;

        protected readonly IGlobalService GlobalService;
        protected readonly IServiceScopeFactory ScopeFactory;
        

        public BusinessServiceBase(TRepositoryManager repository
            , IGlobalDateTimeSettings globalDateTimeBusinessServices
            , ILogger<TEntity> logger
            , IGlobalService globalService
            , IServiceScopeFactory scopeFactory)
        {
            HealthLogger = logger;
            RepositoryManager = repository;
            this.GlobalDateTimeSettings = globalDateTimeBusinessServices;
            ScopeFactory = scopeFactory;
           
            using (var scope = ScopeFactory.CreateScope())
            {
                GlobalService = scope.ServiceProvider.GetRequiredService<IGlobalService>();
            }

        }


        public virtual async Task<Guid> AddAsync(TEntity item)
        {
            CheckIfNull(item);
            CheckIfAddedEntityHasId(item.Id);
            var id = await RepositoryManager.AddAsync(item);
            return id;
        }

        public virtual async Task<TEntity> GetAsync(Guid id)
        {
            ValidateId(id);
            var result = await RepositoryManager.GetAsync(id);

            return result;
        }

        public virtual async Task<TEntity> GetAsync(Guid id, params string[] includes)
        {
            ValidateId(id);
            var result = await RepositoryManager.GetAsync(id, includes);

            return result;
        }

        public virtual async Task<List<TEntity>> ListAsync(int pageNumber)
        {
            var result = await RepositoryManager.ListAsync(pageNumber);
            return result;
        }

        public virtual async Task UpdateAsync(TEntity item)
        {
            CheckIfNull(item);
            ValidateId(item?.Id);

            await RepositoryManager.UpdateAsync(item);
        }

     
        protected void CheckIfNull(TEntity item)
        {
            var typeName = Entity?.GetType()?.Name;
            if (item == null)
            {
                throw new NullReferenceException($"Invalid {typeName} item");
            }

        }

        protected virtual void CheckIfAddedEntityHasId(long id, [CallerLineNumber] int lineNumber = 0, [CallerFilePath] string caller = "", [CallerMemberName] string memberName = "")
        {
            if (id > 0)
            {
                throw new Exception($"Invalid {Entity?.GetType()?.Name} parameter, method name:{memberName}, class name: {caller}, line number: {lineNumber}");
            }
        }

        protected void CheckIfAddedEntityHasId(Guid id, [CallerLineNumber] int lineNumber = 0, [CallerFilePath] string caller = "", [CallerMemberName] string memberName = "")
        {
            if (id != Guid.Empty)
            {
                throw new Exception($"Invalid {Entity?.GetType()?.Name} parameter, method name:{memberName}, class name: {caller}, line number: {lineNumber}");
            }
        }
        protected void ValidateId(Guid? id, [CallerLineNumber] int lineNumber = 0, [CallerFilePath] string caller = "", [CallerMemberName] string memberName = "")
        {
            if (id == default || id == null || id == Guid.Empty)
            {
                throw new Exception($"Invalid {Entity?.GetType()?.Name} parameter, method name:{memberName}, class name: {caller}, line number: {lineNumber}");
            }
        }

        protected virtual void ValidateId(long? id, [CallerLineNumber] int lineNumber = 0, [CallerFilePath] string caller = "", [CallerMemberName] string memberName = "")
        {
            if (id < 1)
            {
                throw new Exception($"Invalid {Entity?.GetType()?.Name} parameter, method name:{memberName}, class name: {caller}, line number: {lineNumber}");
            }
        }
    }
}