using AutoMapper;

using Event.Booking.System.BusinessService.Interfaces;
using Event.Booking.System.BusinessService.Interfaces.Utilities;
using Event.Booking.System.Core.Exceptions;
using Event.Booking.System.Core.Interface;

using Microsoft.AspNetCore.Mvc;

using System.Security.Principal;

namespace Event.Booking.system.Controllers
{
    public class BookingSystemControllerBase : Controller
    {
        [ApiController]
        [Route("api/[controller]")]
        public abstract class BookingControllerBase<TDTO, TEntity, TBusinessServiceManager>
     : ControllerBase
     where TEntity : class, IEntity, IEntityIdentity, new()
     where TDTO : class, IDto, IDtoIdentity, new()
     where TBusinessServiceManager : IBusinessServiceBase<TEntity>
        {
            protected TEntity Entity { get; set; } = new TEntity();
            protected TDTO DTO { get; set; } = new TDTO();
            protected readonly IServiceScopeFactory ScopeFactory;
            protected readonly TBusinessServiceManager BusinessServiceManager;
            protected readonly IGlobalDateTimeSettings GlobalDateTimeSettings;
            protected readonly IGlobalService GlobalService;
            protected readonly IMapper MapperManager;

            protected readonly ILogger<TEntity> HealthLogger;

            protected readonly IConfiguration ConfigSettings;


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


            public BookingControllerBase(ILogger<TEntity> logger
                                      , TBusinessServiceManager businessService
                                      , IGlobalDateTimeSettings globalDateTimeBusinessServices
                                      , IMapper mapper
                                      , IConfiguration configuration
                                      , IGlobalService globalService
                                      , IServiceScopeFactory scopeFactory)
            {
                BusinessServiceManager = businessService;
                this.GlobalDateTimeSettings = globalDateTimeBusinessServices;
                ScopeFactory = scopeFactory;

                using (var scope = ScopeFactory.CreateScope())
                {
                    GlobalService = scope.ServiceProvider.GetRequiredService<IGlobalService>();

                }

                HealthLogger = logger;
                MapperManager = mapper;
                ConfigSettings = configuration;
            }

            [HttpPost]
            protected virtual async Task<IActionResult> AddAsync([FromBody] TDTO item)
            {
                var name = typeof(TEntity).Name;

                try
                {

                    //if (User.Identity.IsAuthenticated)
                    //{

                    if (item == null || !ModelState.IsValid)
                    {
                        return BadRequest($"Invalid {name} State");
                    }

                    TEntity result = MapDTOToEntity(item);
                    SetAuditInformation(result);

                    await BusinessServiceManager.AddAsync(result);
                    return CreatedAtAction($"GetAsync", result.Id);
                    //}
                    //else
                    //{
                    //    return Unauthorized();
                    //}
                }
                catch (PermissionBaseException ex)
                {
                    var errorMessage = $"error at AddAsync of {name}Controller ";
                    HealthLogger.LogError(ex, errorMessage);
                    return BadRequest(ex.Message);
                }
                catch (Exception ex)
                {
                    var errorMessage = $"error at AddAsync of {name}Controller";
                    HealthLogger.LogError(ex, errorMessage);

                    return StatusCode(500);
                }
            }

            [HttpGet("{id}")]
            protected virtual async Task<IActionResult> GetAsync(Guid id)
            {
                var name = typeof(TEntity).Name;

                try
                {
                    //if (User.Identity.IsAuthenticated)
                    //{

                    var entity = await BusinessServiceManager.GetAsync(id);
                    TDTO result = ConvertEntityToDTO(entity);

                    if (result == null)
                    {
                        return NotFound(id);
                    }

                    return Ok(result);
                    //}
                    //else
                    //{
                    //    return Unauthorized();
                    //}
                }
                catch (PermissionBaseException ex)
                {
                    var errorMessage = $"error at GetAsync of {name}Controller with Id : {id}  ";
                    HealthLogger.LogError(ex, errorMessage);
                    return BadRequest(ex.Message);
                }
                catch (Exception ex)
                {
                    var errorMessage = $"error at GetAsync of {name}Controller with Id : {id}  ";
                    HealthLogger.LogError(ex, errorMessage);
                    return StatusCode(500);
                }
            }

            [HttpGet]
            protected virtual async Task<IActionResult> ListAsync([FromQuery] int? pageNumber)
            {
                var name = typeof(TEntity).Name;

                try
                {

                    //if (User.Identity.IsAuthenticated)
                    //{
                    CurrentPageNumber = pageNumber ?? 0;

                    var entities = await BusinessServiceManager.ListAsync(CurrentPageNumber) as List<TEntity>;
                    CurrentPageNumber = 0;

                    if (entities == null || !entities.Any())
                    {
                        return NotFound();
                    }

                    var result = MapperManager.Map<List<TDTO>>(entities);
                    return Ok(result);
                    //}
                    //else
                    //{
                    //    return Unauthorized();
                    //}
                }
                catch (PermissionBaseException ex)
                {
                    var errorMessage = $"error at ListAsync of {name}Controller with pageNumber : {pageNumber} ";
                    HealthLogger.LogError(ex, errorMessage);
                    return BadRequest(ex.Message);
                }
                catch (Exception ex)
                {
                    var errorMessage = $"error at ListAsync of {name}Controller with pageNumber : {pageNumber} ";
                    HealthLogger.LogError(ex, errorMessage);
                    return StatusCode(500);
                }
            }

            [HttpPut("{id}")]
            protected virtual async Task<IActionResult> UpdateAsync([FromBody] TDTO item, Guid id)
            {
                var name = typeof(TEntity).Name;

                try
                {

                    //if (User.Identity.IsAuthenticated)
                    //{
                    if (item == null || !ModelState.IsValid)
                    {
                        return BadRequest($"Invalid {name} State");
                    }

                    item.Id = id;
                    var entity = await TrackedEntityForUpdateAsync(item);

                    if (entity == null)
                    {
                        return NotFound(item.Id);
                    }

                    SetAuditInformation(entity, true);
                    await BusinessServiceManager.UpdateAsync(entity);
                    return Ok();
                    //}
                    //else
                    //{
                    //    return Unauthorized();
                    //}
                }
                catch (PermissionBaseException ex)
                {
                    var errorMessage = $"error at UpdateAsync of {name}Controller  ";
                    HealthLogger.LogError(ex, errorMessage);
                    return BadRequest(ex.Message);
                }
                catch (Exception ex)
                {
                    var errorMessage = $"error at UpdateAsync of {name}Controller ";
                    HealthLogger.LogError(ex, errorMessage);
                    return StatusCode(500);
                }
            }


            #region  TrackedEntityForUpdateAsync
            protected virtual async Task<TEntity> TrackedEntityForUpdateAsync(TDTO TDTO)
            {
                TEntity entity = await BusinessServiceManager.GetAsync(TDTO.Id);

                var _ = entity ?? throw new ArgumentNullException($"{typeof(TEntity).Name} is not found for id : {TDTO.Id}");

                return SetUpdateAuditInformation(TDTO, entity);
            }

            protected virtual async Task<TEntity> TrackedEntityForUpdateAsync<T>(T TDTO)
                where T : IDto, IDtoIdentity
            {
                TEntity entity = await BusinessServiceManager.GetAsync(TDTO.Id);

                var _ = entity ?? throw new ArgumentNullException($"{typeof(TEntity).Name} is not found for id : {TDTO.Id}");

                return SetUpdateAuditInformation<T, TEntity>(TDTO, entity);
            }


            protected virtual async Task<TE> TrackedEntityForUpdateAsync<T, TE>(T TDTO)
                where T : class, IDto, IDtoIdentity, new()
                where TE : class, IEntity, IEntityIdentity, new()
            {
                TE entity = await BusinessServiceManager.GetAsync(TDTO.Id) as TE;

                var _ = entity ?? throw new ArgumentNullException($"{typeof(TE).Name} is not found for id : {TDTO.Id}");

                return SetUpdateAuditInformation<T, TE>(TDTO, entity);
            }

            #endregion  TrackedEntityForUpdateAsync


            #region  SetUpdateAuditInformation
            protected virtual TEntity SetUpdateAuditInformation<T>(T TDTO, TEntity entity)
                where T : IDto
            {
                // Store the created by audit information
                var createdBy = entity.CreatedBy;
                var createdOn = entity.CreatedDate;
                MapperManager.Map(TDTO, entity);

                // Set the created by audit information 
                entity.CreatedBy = createdBy;
                entity.CreatedDate = createdOn;

                return entity;
            }

            protected virtual TE SetUpdateAuditInformation<TT, TE>(TT TDTO, TE entity)
                where TT : IDtoIdentity
                where TE : IEntity
            {
                // Store the created by audit information
                var createdBy = entity.CreatedBy;
                var createdOn = entity.CreatedDate;
                MapperManager.Map(TDTO, entity);

                // Set the created by audit information as it may be lost during mapping
                entity.CreatedBy = createdBy;
                entity.CreatedDate = createdOn;

                return entity;
            }

            protected virtual TEntity SetUpdateAuditInformation(TDTO TDTO, TEntity entity)
            {
                // Store the created by audit information
                var createdBy = entity.CreatedBy;
                var createdOn = entity.CreatedDate;
                MapperManager.Map(TDTO, entity);

                // Set the created by audit information
                entity.CreatedBy = createdBy;
                entity.CreatedDate = createdOn;

                return entity;
            }

            #endregion  SetUpdateAuditInformation


            #region  SetAuditInformation
            protected virtual void SetAuditInformation<T>(T entity, bool isUpdate = false)
                where T : IEntity
            {
                var utcTime = GlobalDateTimeSettings.CurrentDateTime;
                var currentUserId = Guid.NewGuid();//GlobalService.CurrentUserId;
                var date = new DateTime(utcTime.Year, utcTime.Month, utcTime.Day, utcTime.Hour, utcTime.Minute, utcTime.Second, DateTimeKind.Utc);

                if (entity != null)
                {
                    if (!isUpdate)
                    {
                        entity.CreatedBy = currentUserId;
                        entity.CreatedDate = date;
                    }
                    else
                    {
                        entity.UpdatedBy = currentUserId;
                        entity.UpdatedDate = date;
                    }
                }
            }

            protected virtual void SetAuditInformation(TEntity entity, bool isUpdate = false)
            {
                var utcTime = GlobalDateTimeSettings.CurrentDateTime;
                var currentUserId = Guid.NewGuid();//GlobalService.currentUserId;
                var date = new DateTime(utcTime.Year, utcTime.Month, utcTime.Day, utcTime.Hour, utcTime.Minute, utcTime.Second, DateTimeKind.Utc);

                if (entity != null)

                    if (!isUpdate)
                    {
                        entity.CreatedBy = currentUserId;
                        entity.CreatedDate = date;
                    }
                    else
                    {
                        entity.UpdatedBy = currentUserId;
                        entity.UpdatedDate = date;
                    }
            }


            #endregion  SetAuditInformation



            #region  ConvertEntityToDTO
            protected virtual List<TDTO> ConvertEntityToDTO(List<TEntity> entities)
            {
                var result = MapperManager.Map<List<TDTO>>(entities);
                return result;
            }


            protected virtual TDTO ConvertEntityToDTO(TEntity entity)
            {
                return MapperManager.Map<TEntity, TDTO>(entity);
            }

            protected virtual List<TOtherviewModel> ConvertEntityToDTO<TOtherEntity, TOtherviewModel>(List<TOtherEntity> entities)
                where TOtherEntity : class, IEntity, new()
                where TOtherviewModel : class, IDto, new()
            {
                var result = MapperManager.Map<List<TOtherEntity>, List<TOtherviewModel>>(entities);
                return result;
            }

            protected TOtherviewModel ConvertEntityToDTO<TOtherEntity, TOtherviewModel>(TOtherEntity entity)
                where TOtherEntity : class, IEntity, new()
                where TOtherviewModel : class, IDto, new()
            {
                return MapperManager.Map<TOtherEntity, TOtherviewModel>(entity);
            }

            #endregion  ConvertEntityToDTO



            #region  MapDTOToEntity
            protected virtual TEntity MapDTOToEntity(TDTO item)
            {
                return MapperManager.Map<TDTO, TEntity>(item);
            }

            protected virtual TOtherEntity MapDTOToEntity<TOtherviewModel, TOtherEntity>(TOtherviewModel item)
                where TOtherEntity : IEntityIdentity
               where TOtherviewModel : IDtoIdentity, new()
            {
                return MapperManager.Map<TOtherviewModel, TOtherEntity>(item);
            }

            protected virtual TOtherEntity2 MapDTOToEntityWithNoID<TOtherviewModel2, TOtherEntity2>(TOtherviewModel2 item)
              where TOtherEntity2 : IEntityIdentity
             where TOtherviewModel2 : IDtoNoID, new()
            {
                return MapperManager.Map<TOtherviewModel2, TOtherEntity2>(item);
            }

            protected virtual TOtherEntity MapDTOToEntityWithNoIdentity<TOtherviewModel, TOtherEntity>(TOtherviewModel item)
               where TOtherEntity : IEntity
              where TOtherviewModel : IDto, new()
            {
                return MapperManager.Map<TOtherviewModel, TOtherEntity>(item);
            }

            #endregion  MapDTOToEntity

        }
    }
}
