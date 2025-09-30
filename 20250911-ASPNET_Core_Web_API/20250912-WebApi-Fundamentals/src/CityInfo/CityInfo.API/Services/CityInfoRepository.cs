using CityInfo.API.DbContexts;
using CityInfo.API.Entities;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace CityInfo.API.Services
{
    public class CityInfoRepository : ICityInfoRepository
    {
        private readonly CityInfoContext context;

        public CityInfoRepository(CityInfoContext context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }
        public async Task<IEnumerable<City>> GetCitiesAsync()
        {
            return await this.context.Cities.OrderBy(c => c.Name).ToListAsync();
        }

        public async Task<(IEnumerable<City>, PaginationMetadata)> GetCitiesAsync
            (string? name, string? searchQuery, int pageNumber, int pageSize)
        {
            //if (string.IsNullOrEmpty(name)
            //    && string.IsNullOrWhiteSpace(searchQuery))
            //{
            //    return await GetCitiesAsync();
            //}

            // collection to start from
            var collection = context.Cities as IQueryable<City>;

            if (!string.IsNullOrWhiteSpace(name))
            {
                name = name.Trim();
                collection = collection.Where(c => c.Name == name);
            }

            if(!string.IsNullOrWhiteSpace(searchQuery))
            {
                searchQuery = searchQuery.Trim();
                collection = collection.Where(c => c.Name.Contains(searchQuery)
                    || (c.Description != null && c.Description.Contains(searchQuery)));
            }

            var totalItemCount = await collection.CountAsync();

            var paginationMetadata = new PaginationMetadata(
                totalItemCount, pageSize, pageNumber);

            var collectionToReturn =  await collection.OrderBy(c => c.Name)
                .Skip(pageSize * (pageNumber - 1))
                .Take(pageSize)
                .ToListAsync();

            return (collectionToReturn, paginationMetadata);

            /*
             var collection = context.Cities as IQueryable<City>;
            if (!string.IsNullOrWhiteSpace(name))
            {
                name = name.Trim();
                collection = collection.Where(c => c.Name == name);
            }
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                searchQuery = searchQuery.Trim();
                collection = collection.Where(c => c.Name.Contains(searchQuery)
                    || (c.Description != null && c.Description.Contains(searchQuery)));
            }
            return await collection.OrderBy(c => c.Name).ToListAsync();
            */
        }

        public async Task<City?> GetCityAsync(int cityId, bool includePointsOfIntereset)
        {
            if (includePointsOfIntereset)
            {
                return await this.context.Cities
                    .Include(c => c.PointsOdInterest)
                    .Where(c => c.Id == cityId).FirstOrDefaultAsync();
            }
            return await this.context.Cities
                .Where(c => c.Id == cityId).FirstOrDefaultAsync();
        }
        public async Task<bool> CityExistsAsync(int cityId)
        {
                       return await context.Cities.AnyAsync(c => c.Id == cityId);
        }
        public async Task<PointOfInterest?> GetPointOfInterestForCityAsync(int cityId, int pointOfInterestId)
        {
            return await context.PointsOfInterest.Where(p 
                => p.CityId == cityId && p.Id == pointOfInterestId)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<PointOfInterest>> GetPointsOfInterestForCityAsync(int cityId)
        {
            return await context.PointsOfInterest
                .Where(p => p.CityId == cityId).ToListAsync();
        }

        public async Task AddPointOfInterestForCityAsync(int cityId, PointOfInterest pointOfInterest)
        {
            var city = await GetCityAsync(cityId, false);
            if (city != null)
            {
                city.PointsOdInterest.Add(pointOfInterest);
            }
        }

        public async Task<bool> SaveChangesAsync()
        {
            return (await context.SaveChangesAsync() > 0);
        }

        public void DeletePointOfInterest(PointOfInterest pointOfInterest)
        {
            //Não esquecer de anotar essa observação no padrão repository a respeito do I/O do Delete e Add e Task e void
            context.PointsOfInterest.Remove(pointOfInterest);
        }

        public async Task<bool> CityNameMatchesCityId(string? cityName, int cityId)
        {
            return await context.Cities.AnyAsync(c => c.Id == cityId && c.Name == cityName);
        }
    }
}
