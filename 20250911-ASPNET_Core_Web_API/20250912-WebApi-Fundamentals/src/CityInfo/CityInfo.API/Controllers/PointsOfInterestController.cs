using Asp.Versioning;
using AutoMapper;
using CityInfo.API.Models;
using CityInfo.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace CityInfo.API.Controllers
{
    [Route("api/v{version:apiVersion}/cities/{cityId}/pointsofinterest")]
    [Authorize(Policy = "MustBeFromAntwerp")]
    [ApiController]
    [ApiVersion(2)]
    public class PointsOfInterestController : ControllerBase
    {
        private readonly ILogger<PointsOfInterestController> _logger;
        private readonly IMailService _mailService;
        private readonly ICityInfoRepository cityInfoRepository;
        private readonly IMapper mapper;

        public PointsOfInterestController(ILogger<PointsOfInterestController> logger,
            IMailService mailService, 
            ICityInfoRepository cityInfoRepository,
            IMapper mapper)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mailService = mailService ?? throw new ArgumentNullException(nameof(mailService));
            this.cityInfoRepository = cityInfoRepository ?? throw new ArgumentNullException(nameof(cityInfoRepository)); 
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PointOfInterestDto>>> GetPointsOfInterest(int cityId)
        {
            var cityName = User.Claims.FirstOrDefault(c => c.Type == "city")?.Value;

            if (!await cityInfoRepository.CityNameMatchesCityId(cityName, cityId))
            {
                return Forbid();
            }

            if (!await cityInfoRepository.CityExistsAsync(cityId))
            {
                _logger.LogInformation($"City with id {cityId} wasn't found when accessing points of interest.");
                return NotFound();
            }
            var pointsOfInterest = await cityInfoRepository.GetPointsOfInterestForCityAsync(cityId);
            return Ok(mapper.Map<IEnumerable<PointOfInterestDto>>(pointsOfInterest));
        }

        [HttpGet("{pointofinterestid}", Name = "GetPointOfInterest")]
        public async Task< ActionResult<PointOfInterestDto>> GetPointOfInterest(
            int cityId, int pointOfInterestId)
        {
            if(!await cityInfoRepository.CityExistsAsync(cityId))
            {
                return NotFound();
            }

            var pointOfInterest = await cityInfoRepository
                .GetPointOfInterestForCityAsync(cityId, pointOfInterestId);

            if (pointOfInterest == null)
            {
                return NotFound();
            }
            return Ok(mapper.Map<PointOfInterestDto>(pointOfInterest));
        }
        
        [HttpPost]
        public async Task<ActionResult<PointOfInterestDto>> CreatePointOfInterest(
           int cityId,
           PointOfInterestForCreationDto pointOfInterest)
        { 
            if(!await cityInfoRepository.CityExistsAsync(cityId))
            {
                return NotFound();
            }

            var finalpointOfInterest = mapper.Map<Entities.PointOfInterest>(pointOfInterest);

            await cityInfoRepository
                .AddPointOfInterestForCityAsync(cityId, finalpointOfInterest);

            await cityInfoRepository.SaveChangesAsync();

            var createPointOfInterestToReturn = 
                mapper.Map<PointOfInterestDto>(finalpointOfInterest);

            return CreatedAtRoute("GetPointOfInterest",
                 new
                 {
                     cityId = cityId,
                     pointOfInterestId = createPointOfInterestToReturn.Id
                 },
                 createPointOfInterestToReturn);
        }
        

        [HttpPut("{pointofinterestid}")]
        public async Task<ActionResult> UpdatePointOfInterest(int cityId, int pointOfInterestId,
            PointOfInterestForUpdateDto pointOfInterest)
        {
            if (!await cityInfoRepository.CityExistsAsync(cityId))
            {
                return NotFound();
            }
            
            var pointOfInterestEntity = await cityInfoRepository
                .GetPointOfInterestForCityAsync(cityId, pointOfInterestId);
            
            if(pointOfInterestEntity == null)
            {
                return NotFound();
            }
            
            mapper.Map(pointOfInterest, pointOfInterestEntity);
            await cityInfoRepository.SaveChangesAsync();
            
            return NoContent();
        }

        [HttpPatch("{pointofinterestid}")]
        public async Task<ActionResult> PartiallyUpdatePointOfInterest(
            int cityId, int pointOfInterestId,
            JsonPatchDocument<PointOfInterestForUpdateDto> patchDocument)
        {
            if(!await cityInfoRepository.CityExistsAsync(cityId))
            {
                return NotFound();
            }

            var pointOfInterestEntity = await cityInfoRepository
                .GetPointOfInterestForCityAsync(cityId, pointOfInterestId);
            
            if (pointOfInterestEntity == null)
            {
                return NotFound();
            }

            var pointOfInterestToPatch =
                mapper.Map<PointOfInterestForUpdateDto>(pointOfInterestEntity);

            patchDocument.ApplyTo(pointOfInterestToPatch, ModelState);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!TryValidateModel(pointOfInterestToPatch))
            {
                return BadRequest(ModelState);
            }

            mapper.Map(pointOfInterestToPatch, pointOfInterestEntity);
            await cityInfoRepository.SaveChangesAsync();

            return NoContent();
        }
        
        [HttpDelete("{pointOfInterestId}")]
        public async Task<ActionResult> DeletePointOfInterest(int cityId, int pointOfInterestId)
        {
            if (!await cityInfoRepository.CityExistsAsync(cityId))
            {
                return NotFound();
            }

            var pointOfInterestEntity = await cityInfoRepository.GetPointOfInterestForCityAsync(cityId, pointOfInterestId);
            
            if (pointOfInterestEntity == null)
            {
                return NotFound();
            }

            cityInfoRepository.DeletePointOfInterest(pointOfInterestEntity);

            _mailService.Send("Point of interest deleted.",
                    $"Point of interest {pointOfInterestEntity.Name} with id {pointOfInterestEntity.Id} was deleted.");

            return NoContent();
        }

    }
}
