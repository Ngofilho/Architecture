using CityInfo.API.Entities;

namespace CityInfo.API.Services
{
    public interface ICityInfoRepository
    {
        Task<IEnumerable<Entities.City>> GetCitiesAsync();
        Task<(IEnumerable<City>, PaginationMetadata)> GetCitiesAsync(string? name, string? searchQuery, int pageNumber, int pageSize);
        Task<Entities.City?> GetCityAsync(int cityId, bool includePointsOfIntereset);
        Task<IEnumerable<Entities.PointOfInterest>> GetPointsOfInterestForCityAsync(int cityId);
        Task<Entities.PointOfInterest?> GetPointOfInterestForCityAsync(int cityId, int pointOfInterestId);
        Task<bool> CityExistsAsync(int cityId);
        Task AddPointOfInterestForCityAsync(int cityId, Entities.PointOfInterest pointOfInterest);
        void DeletePointOfInterest(Entities.PointOfInterest pointOfInterest);
        Task<bool> CityNameMatchesCityId(string? cityName, int cityId);
        Task<bool> SaveChangesAsync();
    }
}
