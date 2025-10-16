using AutoMapper;
using CourseLibrary.API.Helpers;
using CourseLibrary.API.Models;
using CourseLibrary.API.ResourceParameters;
using CourseLibrary.API.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace CourseLibrary.API.Controllers;

[ApiController] 
[Route("api/authors")]
public class AuthorsController : ControllerBase
{
    private readonly ICourseLibraryRepository _courseLibraryRepository;
    private readonly IMapper _mapper;
    private readonly IPropertyMappingService _propertyMappingService;

    public AuthorsController(
        ICourseLibraryRepository courseLibraryRepository,
        IMapper mapper, IPropertyMappingService propertyMappingService)
    {
        _courseLibraryRepository = courseLibraryRepository ??
            throw new ArgumentNullException(nameof(courseLibraryRepository));
        _mapper = mapper ??
            throw new ArgumentNullException(nameof(mapper));
        this._propertyMappingService = propertyMappingService;
    }
    /*
    [HttpGet] 
    [HttpHead]
    public async Task<ActionResult<IEnumerable<AuthorDto>>> GetAuthors(
        [FromQuery] string? mainCategory = "", string? searchQuery = "")
    {
        // throw new Exception("Test exception");

        // get authors from repo
        var authorsFromRepo = await _courseLibraryRepository
            .GetAuthorsAsync(mainCategory, searchQuery); 

        // return them
        return Ok(_mapper.Map<IEnumerable<AuthorDto>>(authorsFromRepo));
    }*/

    [HttpHead]
    [HttpGet(Name ="GetAuthors")]
    //[HttpGet("GetAuthorsWithResourceParameters", Name = "GetAuthorsWithResourceParameters")]
    //public async Task<ActionResult<IEnumerable<AuthorDto>>> GetAuthorsWithResourceParameters([FromQuery]
    public async Task<ActionResult<IEnumerable<AuthorDto>>> GetAuthors([FromQuery]
        AuthorResourceParameters authorResourceParameters)
    {
        if (!_propertyMappingService
            .ValidMappingExistsFor<AuthorDto, Entities.Author>(
            authorResourceParameters.OrderBy))
        {
            return BadRequest();
        }

        // get authors from repo
        var authorsFromRepo = await _courseLibraryRepository
            .GetAuthorsAsync(authorResourceParameters);
     
        var previousPageLink = authorsFromRepo.HasPrevious ?
            CreateAuthorsResourceUri(authorResourceParameters, 
            ResourceUriType.PreviousPage) : null;

        var nextPageLink = authorsFromRepo.HasNext ?
            CreateAuthorsResourceUri(authorResourceParameters, 
            ResourceUriType.NextPage) : null;

        var paginationMetadata = new
        {
            totalCount = authorsFromRepo.TotalCount,
            pageSize = authorsFromRepo.PageSize,
            currentPage = authorsFromRepo.CurrentPage,
            totalPages = authorsFromRepo.TotalPages,
            previousPageLink = previousPageLink!,
            nextPageLink = nextPageLink!
        };

        Response.Headers.Add("X-Pagination",
            JsonSerializer.Serialize(paginationMetadata));

        // return them
        return Ok(_mapper.Map<IEnumerable<AuthorDto>>(authorsFromRepo));
    }

    private string? CreateAuthorsResourceUri(
        AuthorResourceParameters authorResourceParameters,
        ResourceUriType type)
    {

        switch (type)
        {
            case ResourceUriType.PreviousPage:
                    //return Url.Link("GetAuthorsWithResourceParameters",
                    return Url.Link("GetAuthors",
                    new
                    {
                        orderby = authorResourceParameters.OrderBy,
                        mainCategory = authorResourceParameters.MainCategory ?? "Verdao",
                        searchQuery = authorResourceParameters.SearchQuery ?? "Reidelas",
                        pageNumber = authorResourceParameters.PageNumber - 1,
                        pageSize = authorResourceParameters.PageSize
                    });
            case ResourceUriType.NextPage:
                    //return Url.Link("GetAuthorsWithResourceParameters",
                    return Url.Link("GetAuthors",
                    new
                    {
                        orderby = authorResourceParameters.OrderBy,
                        mainCategory = authorResourceParameters.MainCategory ?? "Verdao",
                        searchQuery = authorResourceParameters.SearchQuery ?? "Palmeiras",
                        pageNumber = authorResourceParameters.PageNumber + 1,
                        pageSize = authorResourceParameters.PageSize
                    });
            default:
                    //return Url.Link("GetAuthorsWithResourceParameters",
                    return Url.Link("GetAuthors",
                    new
                    {
                        orderby = authorResourceParameters.OrderBy,
                        mainCategory = authorResourceParameters.MainCategory ?? "Verdao",
                        searchQuery = authorResourceParameters.SearchQuery?? "Giuseppe",
                        pageNumber = authorResourceParameters.PageNumber,
                        pageSize = authorResourceParameters.PageSize
                    });
        }
    }


    [HttpGet("{authorId}", Name = "GetAuthor")]
    public async Task<ActionResult<AuthorDto>> GetAuthor(Guid authorId)
    {
        // get author from repo
        var authorFromRepo = await _courseLibraryRepository
            .GetAuthorAsync(authorId);

        if (authorFromRepo == null)
        {
            return NotFound();
        }

        // return author
        return Ok(_mapper.Map<AuthorDto>(authorFromRepo));
    }

    [HttpPost]
    public async Task<ActionResult<AuthorDto>> CreateAuthor(AuthorForCreationDto author)
    {
        var authorEntity = _mapper.Map<Entities.Author>(author);

        _courseLibraryRepository.AddAuthor(authorEntity);
        await _courseLibraryRepository.SaveAsync();

        var authorToReturn = _mapper.Map<AuthorDto>(authorEntity);

        return CreatedAtRoute("GetAuthor",
            new { authorId = authorToReturn.Id },
            authorToReturn);
    }

    [HttpOptions()]
    public IActionResult GetAuthorsOptions()
    {
        Response.Headers.Add("Allow", "GET, HEAD, POST, OPTIONS");
        return Ok();
    }

}
