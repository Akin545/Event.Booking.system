using Event.Booking.System.Core.Interface;
using Event.Booking.System.Repository.Interfaces;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore.Infrastructure;

using System.Data;
using System.Data.Common;
using System.Data.SqlTypes;
using System.Linq.Expressions;

namespace Event.Booking.System.Repository
{
    public abstract class RepositoryBase<TEntity> : IRepositoryBase<TEntity>
       where TEntity : class, IEntity, IEntityIdentity, new()
    {

        protected TEntity Entity { get; set; } = new TEntity();
        protected int SkippedDbRecordSize { get => MaxPageSize * (CurrentPageNumber - 1); }

        protected short MaxPageSize
        {
            get
            {
                _ = short.TryParse(ConfigSetting["MaxPageSize"], out short maxPageSize);
                return maxPageSize;
            }
        }

        protected readonly IConfiguration ConfigSetting;
        protected readonly IServiceScopeFactory ScopeFactory;
        protected readonly ILogger<TEntity> HealthLogger;

        protected int CurrentPageNumber
        {
            get
            {
                if (_currentPageNumber < 1)
                {
                    _currentPageNumber = 1;
                }
                return _currentPageNumber;
            }
            set
            {
                _currentPageNumber = value;
            }
        }
        private int _currentPageNumber { get; set; }


        protected RepositoryBase(IConfiguration configuration, ILogger<TEntity> logger
            , IServiceScopeFactory scopeFactory
            )
        {
            ConfigSetting = configuration;
            HealthLogger = logger;
            ScopeFactory = scopeFactory;
        }

        protected void CheckConnection(AppDbContext databaseContext)
        {
            var connection = databaseContext.Database.GetDbConnection();

            if (connection.State == ConnectionState.Closed)
            {
                GetDbConnection(connection);
            }
        }

        protected void GetDbConnection(DbConnection dbConnection)
        {
            var conn = dbConnection;

            if (conn.State == ConnectionState.Closed)
            {
                try
                {
                    conn.Open();

                    if (conn.State == ConnectionState.Closed || conn.State == ConnectionState.Broken)
                    {
                        //SetToken(conn);
                    }
                }
                catch (Exception ex)
                {
                    //SetToken(conn);
                }
            }
            else if (conn.State == ConnectionState.Broken)
            {
                //SetToken(conn);
            }
        }

        public virtual async Task<Guid> AddAsync(TEntity entity)
        {
            try
            {
                using (var scope = ScopeFactory.CreateScope())
                {
                    var databaseContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    await databaseContext.Set<TEntity>().AddAsync(entity);
                    await databaseContext.SaveChangesAsync();
                }

                var typeName = Entity?.GetType()?.Name;
                HealthLogger.LogInformation($" Successfully Added {typeName}'s Id: '{entity?.Id} ");

                var _ = entity ?? throw new NullReferenceException();
                return entity.Id;
            }
            catch (Exception ex)
            {
                HealthLogger.LogError(ex, " error at AddAsync with page number", entity);

                throw;
            }
        }

        public virtual async Task<TEntity> GetAsync(Guid id)
        {
            try
            {
                using (var scope = ScopeFactory.CreateScope())
                {
                    var databaseContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    var entity = await databaseContext.Set<TEntity>()
                                              .Where(x => x.Id.Equals(id))
                                              .SingleOrDefaultAsync();

                    var typeName = Entity?.GetType()?.Name;


                    HealthLogger.LogInformation($" Successfully retrieved {typeName} with the Id: '{entity?.Id} ");

                    return entity;
                }
            }
            catch (Exception ex)
            {
                HealthLogger.LogError(ex, " error at GetAsync with page number", id);

                throw;
            }
        }

        public virtual async Task<TEntity> GetAsync(Guid id, params string[] includes)
        {
            try
            {
                using (var scope = ScopeFactory.CreateScope())
                {
                    var databaseContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    {
                        var query = databaseContext.Set<TEntity>().AsQueryable();

                        if (includes != null)
                        {
                            foreach (var include in includes)
                            {
                                query = query.Include(include);
                            }
                        }

                        var entity = await query.SingleOrDefaultAsync(x => x.Id == id);

                        var typeName = Entity?.GetType()?.Name;
                        HealthLogger.LogInformation($" Successfully retrieved {typeName} with the Id: '{entity?.Id} ");

                        return entity ?? new TEntity();
                    }
                }
            }
            catch (Exception ex)
            {
                var obj = new object[] { includes, id };
                HealthLogger.LogError(ex, " error at GetAsync ", obj);

                throw;
            }
        }

        public virtual async Task<List<TEntity>> ListAsync(int pageNumber)
        {
            try
            {
                CurrentPageNumber = pageNumber;

                using (var scope = ScopeFactory.CreateScope())
                {
                    var databaseContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    List<TEntity> result = await databaseContext.Set<TEntity>().AsNoTracking()
                                                .OrderBy(r => r.Id)
                                                .Skip(SkippedDbRecordSize)
                                                .Take(MaxPageSize)
                                                .ToListAsync();

                    var typeName = Entity?.GetType()?.Name;
                    HealthLogger.LogInformation($" Successfully retrieved {typeName}'s List with page number {pageNumber} ");
                    CurrentPageNumber = 0;

                    return result;
                }
            }
            catch (SqlNullValueException s)
            {
                HealthLogger.LogError(s, " SqlNullValueException at ListAsync with page number", pageNumber);

                throw;
            }
            catch (Exception ex)
            {
                HealthLogger.LogError(ex, " error at ListAsync with page number", pageNumber);

                throw;
            }
        }
        public virtual async Task UpdateAsync(TEntity entity)
        {
            try
            {
                using (var scope = ScopeFactory.CreateScope())
                {
                    var databaseContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    databaseContext.Entry(entity).State = EntityState.Modified;
                    await databaseContext.SaveChangesAsync();
                }

                var typeName = Entity?.GetType()?.Name;
                HealthLogger.LogInformation($" Successfully Updated {typeName}'s Id: '{entity?.Id} ");
            }
            catch (Exception ex)
            {
                HealthLogger.LogError(ex, " error at UpdateAsync ", entity);

                throw;
            }
        }

    }
}
