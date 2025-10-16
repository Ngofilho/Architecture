# [ASP.NET Core Web Api Deep Dive](https://app.pluralsight.com/library/courses/asp-dot-net-core-6-web-api-deep-dive)


## Chapter 3 : Designing the Outer Facing Contract
Consists of three big concepts a consumer of an API uses to interact with that API

- First, the resource identifiers. In other words, the URIs where the resources can be found. 
- Combined with [HTTP methods](https://datatracker.ietf.org/doc/html/rfc9110) like GET to get resources, POST to create them, and other ones. These methods are part of the HTTP standard. 
- Third is the optional payload. For example, when creating a resource, the HTTP request will have to contain a representation of the resource you want to create. When getting a resource, the HTTP response will contain a resource representation in its response body

Resource Identifier: Covers the fact that resources are identified by URIs.
Guidelines: A URI should always be a noun.   Avoid ~~http://api/getauthors~~ use instead `http://api/authors` with verb GET;
Convey meaning when choosing nouns.

- Follow through on this principle for predictability
Avoid ~~api/something/somethingelse/employees~~ use instead `api/employees`
Avoid ~~api/id/employees/~~ use instead `api/employees/{employeeId}`

- Represent hierarchy when naming resources  
api/authors/{authorId}/courses
api/authors/{authorId}/courses/{courseId}

- Filters, sorting orders aren't resources.
They should be passed by query string
~~api/authors/orderby/name~~ use instead api/authors?ordeby=name


- **Routing**   
 Matches a request URI to an action on a controller achieved by 
```csharp
app.MapControllers();
```

|HTTP Method|Request Payload|Sample URI|Response Payload|
|-|-|-|-|
|GET|-|`/api/authors /api/authors/{authorId}`|author collection single author|
|POST|single author|`/api/authors`|single author|
|PUT|single author|`/api/authors/{authorId}`|single author or empty|
|PATCH|JsonPatchDocument on author|`/api/authors/{authorId}`|single author or empty|
|DELETE|-|`/api/authors/{authorId}`|-|
|HEAD|-|`/api/authors /api/authors/{authorId}`|-|
|OPTIONS|-|`/api/…`|-|


**Handling Faults**

|Errors|Faults|
|-|-|
|Consumer passes invalid data to the API, and the API correctly rejects this | Api fails to return a response to a valid request|
Level 400 status code |Level 500 status code|
*Do not contribute* to API availability| *Do contribute* to API availability|

**Avoiding Exposing Implementaiton Details**
```csharp
if (app.Environment.IsDevelopment())
{
	app.UseDeveloperException();
}
else
{
	app.UseExceptionHandler(appBuilder =>
	{
		appBuilder.Run(async context =>
		{
			context.Response.StatusCode = 500;
			await context.Response.WriteAsync("An unexpected fault happened. Try again later.");
		}
	});
}
```
---

## Chapter 4 - Manipulating Resources

### Method Safety and Idempotency

Method is considered safe when it does not change the resource representation. `GETS` and `HEADS` are safe methods. the side effects of calling it once are the same side effects that happen when calling it multiple times.  

Method is considered idempotent when the same request can be made multiple times with the same effect as making it once. `PUTS`, `DELETES` and `HEADS` are idempotent methods.   

|HTTP Method|Safe|Idempotent|
|-|-|-|
|GET|Yes|Yes|
|OPTIONS|Yes|Yes|
|HEAD|Yes|Yes|
|POST|No|No|
|DELETE|No|Yes|
|PUT|No|Yes|
|PATCH|No|No|

Method safety and idempotency help decide which
method to use for which use case

### Advanced resource creation scenarios
It's a good practice to annotate ApiControllers with `[ApiController]` attribute. The ApiController attribute adds a requirement for attribute‑based routing. 
1. When we looked into routing, we learned that route templates should be applied with attributes when building APIs   
2. what is returned in case of an error follows a certain format, the ProblemDetails format.   
3. Bind Inferred Source   
	1. `FromBody` is inferred for complex type parameters thanks to the `[ApiController]`, ASP.Net by default try to bind the complex model to the body of the request.  
	2. `FromForm` is inferred for action parameters of type `IFormFile` and `IFormFileCollection`.  
	3. `FromRoute` is inferred for any action parameter name matching a parameter in the route template. When more than one route matches an action parameter, any route value is considered `FromRoute`.  
	4. `FromQuery` is inferred for any other action parameters.    

### Creating a set of Father items along side with its children on one go.   
```csharp
        /*
        1. The GET method is a bit special because it needs to take a list of ids as input.
        2. The ids are passed in the route, and since it's a list of ids, they need to be enclosed in parentheses and bound to specialized class that inherits from IModelBinder. Check the course of customization of Asp.Net Core model binding for more details.
        */
        [HttpGet("({authorIds})", Name = "GetAuthorCollection")]
        public async Task<ActionResult<IEnumerable<AuthorForCreationDto>>> GetAuthorCollection(
            [ModelBinder(BinderType = typeof(ArrayModelBinder))]            
            [FromRoute] IEnumerable<Guid> authorIds)
        {
            var authorEntities = await _courseLibraryRepository.GetAuthorsAsync(authorIds);

            // do we have all requested authors?
            if (authorIds.Count() != authorEntities.Count())
            {
                return NotFound();
            }

            // map
            var authorsToReturn = _mapper.Map<IEnumerable<AuthorDto>>(authorEntities);
            return Ok(authorsToReturn);
        }

        /*The post method is usual as always. Nothing special but the return response from it
        It's been used the CreatedAtRoute method to return a 201 status code along with a Location header. 
        The location header contains the URI of the newly created resource plus ids to be used on the GET method and the response body contains the newly created resources.
        */
        [HttpPost]
        public async Task<ActionResult<IEnumerable<AuthorDto>>> CreateAuthorCollection(IEnumerable<AuthorForCreationDto> authorCollection)
        {
            var authorEntities = _mapper.Map<IEnumerable<Author>>(authorCollection);
            foreach (var author in authorEntities)
            {
                _courseLibraryRepository.AddAuthor(author);
            }
            await _courseLibraryRepository.SaveAsync();

            var authorCollectionToReturn = _mapper.Map<IEnumerable<AuthorDto>>(authorEntities);

            var authorIdsAsString = string.Join(",",
                authorCollectionToReturn.Select(a => a.Id));

            return CreatedAtRoute("GetAuthorCollection",
                new {authorIds = authorIdsAsString},
                authorCollectionToReturn);
        }
```

The model binder exmaple
```csharp
    /*
    Class used to bind a list of guids from the route and to be used on the GET method of the AuthorCollectionController
    */
    public class ArrayModelBinder : IModelBinder
    {
        // This is the only method (BindModelAsync) to implement from this interface 
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            // Our binder works only on enumerable types
            if (!bindingContext.ModelMetadata.IsEnumerableType)
            {
                bindingContext.Result = ModelBindingResult.Failed();
                return Task.CompletedTask;
            }

            // Get the inputed value through the value provider
            var value = bindingContext.ValueProvider
                .GetValue(bindingContext.ModelName).ToString();
            
            // If that value is null or whitespace,we return null
            if (string.IsNullOrEmpty(value))
            {
                bindingContext.Result = ModelBindingResult.Success(null);
                return Task.CompletedTask;
            }

            // The value isn't null or whitespace
            // and the type of the model is enumerable
            // Get the enumerable's type, and a converter
            var elementType = bindingContext.ModelType.GetTypeInfo().GenericTypeArguments[0];
            var converter = TypeDescriptor.GetConverter(elementType);

            // Converter each item in the value list to the enumerable type
            var values = value.Split(new[] { "," },
                StringSplitOptions.RemoveEmptyEntries)
            .Select(x => converter.ConvertFromString(x.Trim()))
                .ToArray();

            // Create an array of that type, and set it as the Model value 
            var typedValues = Array.CreateInstance(elementType, values.Length);
            values.CopyTo(typedValues, 0);
            bindingContext.Model = typedValues;

            // return a successful result, passing in the Model
            bindingContext.Result = ModelBindingResult.Success(bindingContext.Model);
            return Task.CompletedTask;
        }
    }

```

Example of the POST method request. Mind the parenthesis in the URI request. It's obligatory   
```json
https://localhost:5001/api/authorcollections/(guid_01,guid_02,guid_n)
http://localhost:5000/api/authorcollections/(8e5f2179-e312-4b2d-9074-bdd0164f00f5,39b3d850-3d5d-4afe-83c6-2a19bac09ec2)
```


### PATCH vs PUT   
`http://localhost:5001/api/authors/25141d83-4584-4487-a306-0441695d8e24`
`POST` with id in the route turns the verb idempotent, which by default it's not. In this kind of scenario the best approach is use the `405 - Method not allowed` or `409 - Conflict`. 

When issuing a `PUT` request, all fields of the resource should be overwritten or set to their default values. When issuing a PUT request, all fields of the resource should be overwritten or set to their default values. If a field is missing, that field should be put to its default value. Regarding the response, the updated resource or an empty response are valid.  

When in need to partially update a resource, `PATCH` comes in hand.  
The URI is the same as for PUT, but the request payload is somewhat special. It's a `JsonPatchDocument`.   
The response can follow the same policy of the PUT response.  
It's the JsonPatch standard that defines a JSON document structure for expressing a sequence of operations to apply to a JSON document.  
The `application/json‑patch+json` media type is used to identify such PATCH documents.  

There's six different operations possible. 
The **add** operation will add a property at a path location with a specific value, passed through via value. If it is used on a path that exists, the property value will be replaced. If it is used on an un‑existing path, the property should be added to the resource. But something like that is typically only possible when working with dynamic resources, often in CRM‑like systems.  
The **remove** operation will remove a property, or in non‑dynamic cases, set it to its default value. It only has one property that has to be set next to the operation, path.   
The **Replace** replaces the value at the specified path with the provided value. It's functionally the same as a remove operation, followed by an add operation.    
The **Copy** will take the value from the from property and copy it over to the path property. It is thus an add operation at the path location with the value specified in the from member.   
The **Move** then will copy over the value at the from property to the path property and remove the value at the from property. This operation is functionally identical to a remove operation on the from location, followed by an add operation at the path location with a removed value.   
The **Test** tests that a value at a target location is equal to a specified value.   

<details><summary>Example of the 6 Possible JsonPatchDocument Operations and the C# method to handle</summary>

```json
[{
"op":"add",
"from":"/a/b",
"value":"palmeiras"
},
{
"op":"remove",
"from":"/a/b"
},
{
"op":"replace",
"from":"/a/b",
"path":"verdao"
},
{
"op":"copy",
"from":"/a/b",
"path":"/a/c"
},
{
"op":"move",
"from":"a/b",
"path":"/a/c"
},
{
"op":"test",
"from":"/a/b",
"path":"Palmeiras"
}
]
```

The C# code to handle the JsonPatch operation. It requires the `Microsoft.AspNetCore.JsonPatch` package.
```csharp
[HttpPatch("{courseId}")]
    public async Task<IActionResult> PartiallyUpdateCurseForAuthor(
        Guid authorId,
        Guid courseId,
        JsonPatchDocument<CourseForUpdateDto> patchDocument)
    {
        if (!await _courseLibraryRepository.AuthorExistsAsync(authorId))
        {
            return NotFound();
        }

        var courseForAuthorFromRepo = await _courseLibraryRepository
            .GetCourseAsync(authorId, courseId);
        
        if (courseForAuthorFromRepo == null)
        {
            var courseDto = new CourseForUpdateDto();
            patchDocument.ApplyTo(courseDto);
            var courseToAdd = _mapper.Map<Entities.Course>(courseDto);
            courseToAdd.Id = courseId;

            _courseLibraryRepository.AddCourse(authorId, courseToAdd);
            await _courseLibraryRepository.SaveAsync();

            var courseToReturn = _mapper.Map<CourseDto>(courseToAdd);
            return CreatedAtRoute("GetCourseForAuthor",
                new { authorId, courseId = courseToReturn.Id },
                courseToReturn);
        }

        var courseToPatch = _mapper.Map<CourseForUpdateDto>(courseForAuthorFromRepo);

        patchDocument.ApplyTo(courseToPatch);

        _mapper.Map(courseToPatch, courseForAuthorFromRepo);

        _courseLibraryRepository.UpdateCourse(courseForAuthorFromRepo);

        await _courseLibraryRepository.SaveAsync();
        
        return NoContent();
    }
```
</details>

Now, these cases are not limited to simple properties on a resource. You can manipulate array properties, You can access nested properties. You can even add a list of items to an array in one go. So, path doesn't have to be a simple property, and value doesn't have to be one string value. From these, it follows that patch is neither safe nor idempotent. It changes resource representations, and as it can add to an array, sending it multiple times will have different outcomes.   
The most important thing to remember is that a JSON PATCH document is essentially a list of operations that have to be applied to the resource, which thus allows for partial updates.    

It's the middleware used return either json or xml.  
```csharp
builder.Services.AddControllers(configure =>
{ 
	configure.ReturnHttpNotAcceptable = true; 
})
.AddXmlDataContractSerializerFormatters()
AddNewtonsoftJson(setupAction => 
    {
        setupAction.SeriallizerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
    })
```

with this above ordenation, if it's no passed the `accept:application/json` in the header request, the response will be a xml, this happens due the middleware order.  


### Upserting
Considerations regarding the Architecture

|Server responsible for URI|Consumer responsible for URI|
|-|-|
|PUT/PATCH request must go to an existing URI|PUT/PATCH request can be sent to an unexisting URI, because the consumer is allowed to create id|
|It it doesn't exist, a 404 is returned|If it doesn't exist, the resource is created|
|POST must be used for creation as we cannot know the URI in advance|PUT/PATCH can be used for creation: upserting|


<details><suumary>

**Upserting with PUT**
</summary>

```csharp
    [HttpPut("{courseId}")]
    public async Task<IActionResult> UpdateCourseForAuthor(Guid authorId,
      Guid courseId,
      CourseForUpdateDto course)
    {
        if (!await _courseLibraryRepository.AuthorExistsAsync(authorId))
        {
            return NotFound();
        }

        var courseForAuthorFromRepo = await _courseLibraryRepository
            .GetCourseAsync(authorId, courseId);

        if (courseForAuthorFromRepo == null)
        {
            var courseToAdd = _mapper.Map<Entities.Course>(course);
            courseToAdd.Id = courseId;
            _courseLibraryRepository.AddCourse(authorId, courseToAdd);
            await _courseLibraryRepository.SaveAsync();

            var courseToReturn = _mapper.Map<CourseDto>(courseToAdd);
            return CreatedAtRoute("GetCourseForAuthor",
                new { authorId, courseId = courseToReturn.Id }, 
                courseToReturn);
        }

        _mapper.Map(course, courseForAuthorFromRepo);

        _courseLibraryRepository.UpdateCourse(courseForAuthorFromRepo);

        await _courseLibraryRepository.SaveAsync();
        return NoContent();
    }
```
</details>

<details><suumary>

**Upserting with PATCH**  
```csharp
    [HttpPatch("{courseId}")]
    public async Task<IActionResult> PartiallyUpdateCourseForAuthor(
        Guid authorId,
        Guid courseId,
        JsonPatchDocument<CourseForUpdateDto> patchDocument)
    {
        if (!await _courseLibraryRepository.AuthorExistsAsync(authorId))
        {
            return NotFound();
        }

        var courseForAuthorFromRepo = await _courseLibraryRepository
            .GetCourseAsync(authorId, courseId);

        if (courseForAuthorFromRepo == null)
        {
            var courseDto = new CourseForUpdateDto();
            patchDocument.ApplyTo(courseDto);
            var courseToAdd = _mapper.Map<Entities.Course>(courseDto);
            courseToAdd.Id = courseId;

            _courseLibraryRepository.AddCourse(authorId, courseToAdd);
            await _courseLibraryRepository.SaveAsync();

            var courseToReturn = _mapper.Map<CourseDto>(courseToAdd);
            return CreatedAtRoute("GetCourseForAuthor",
                new { authorId, courseId = courseToReturn.Id }, 
                courseToReturn);
        }

        var courseToPatch = _mapper.Map<CourseForUpdateDto>(
            courseForAuthorFromRepo);
        patchDocument.ApplyTo(courseToPatch);

        _mapper.Map(courseToPatch, courseForAuthorFromRepo);

        _courseLibraryRepository.UpdateCourse(courseForAuthorFromRepo);

        await _courseLibraryRepository.SaveAsync();

        return NoContent();
    }

```
</details>

### Supporting OPTIONS   
An OPTIONS request represents a request for information about the communication options available at a certain URI. It allows a clients to determine the options and/or requirements associated with a resource, or the capabilities of an API. OPTIONS tell us whether or not we can get the resource POST with deleted and so on. It thus works on the resource level. OPTIONS should be returned in the allow response letter as a comma‑separated list of method names.   
OPTIONS tell us whether or not we can get the resource POST with deleted and so on. It thus works on the resource level. These OPTIONS should be returned in the allow response letter as a comma‑separated list of method names.  
We could, by the way, include a response body which describes the options. But the format of that is not covered by the HTTP standard.  

Sample of `OPTIONS` implementation  
```csharp
    [HttpOptions()]
    public IActionResult GetAuthorsOptions()
    {
        Response.Headers.Add("Allow", "GET,HEAD,POST,OPTIONS");
        return Ok();
    }
```
### Inspecting input formatters
This guarantees the managing of the XML and Json either in the request and the response.  
```csharp
builder.Services.AddControllers(configure =>
        {
            configure.ReturnHttpNotAcceptable = true;
        })
        .AddNewtonsoftJson(setupAction =>
        {
            setupAction.SerializerSettings.ContractResolver =
                new CamelCasePropertyNamesContractResolver();
        })
        .AddXmlDataContractSerializerFormatters();
```
### HTTP method overview by use case
---

## Chapter 5 - Validating Data and Reporting Validation Errors  
### **Working with Validation in a RESTful World**   
1- **Defining validation rules**  
 - In ASP.NET Core rules are defined throught
	1. Data Annotations   
	2. Implementing `IValidatableObject` 
  
2- **Checking validation rules**   
 - Model State
	1. It's dictionary containing the state of the model and model binding validation.   
	2. Contains a collection of erros messages for each property value submitted.      
	If one of them is false, the `ModelState.IsValid()` is `false`  

3- **Reporting validation errors**  
 - Response status should be `422` status code. This means that the server understands the Content‑Type of the request       
    1. Unprocessable Content  
    2. `415` is inappropriate because the `Content-Type` is understood although incorrect.       
 - Response body should contain validation errors   
    1. Problem details RFC   

When a validation error happens, the consumer of the API needs to be notified. It's a mistake the client made, so that warrants a 400 level status code.  

### **Validation and the `ApiController` Attribute**   
 Whenever a controller is annotated with it, it will automatically return a 400 Bad Request on validation errors. So, annotations are checked during model binding and affect the ModelState dictionary. The `ApiController` attribute ensures that in the case of an invalid ModelState, a 400 Bad Request is returned with the validation errors in the response body.
 
 **Customizing Error Messages**  
 ```csharp
 [Required(ErrorMessage="Yout should fill out a title")]
 [MaxLength(100, ErrorMessage= "The title shouldn't have more than 100 characters"]
 public string Title {get;set;} = string.Empty;
 ```

### Reporting Validation Errors   
[Problem details for HTTP APIs RFC](https://tools.ietf.org/html/rfc7807)  
- Defines common error formats for those applications that need one  
- Allows identifying distinct problem types specific to API needs  

This is the desired reporting validation from the RFC. To achieve this, it's necessary to extend the `ApiController` implementation.  

```json
// Content-Type: application/problem+json
{
"errors":
{
	"title": [ 
		"The title shouldn't have more than 100 characters."
	] },
"type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
"title": "One or more validation errors occurred.",
"status": 422,
"detail": "See the errors property for details.",
"instance": "/api/authors/2902b665-1190-4c70-9915-b9c2d7680450/courses",
"extensions": {
"traceId": "0HLO3MNBSPFI2:00000001"
}}
```

To achieve the response above, it's sugested to implement the code bellow
```csharp
 builder.Services.AddControllers(configure =>
        {
            configure.ReturnHttpNotAcceptable = true;
        })
        .AddNewtonsoftJson(setupAction =>
        {
            setupAction.SerializerSettings.ContractResolver =
                new CamelCasePropertyNamesContractResolver();
        })
        .AddXmlDataContractSerializerFormatters()
        .ConfigureApiBehaviorOptions(setupAction =>
        {
            setupAction.InvalidModelStateResponseFactory = context =>
            {
                // create a validation problem details object
                var problemDetailsFactory = context.HttpContext.RequestServices
                    .GetRequiredService<ProblemDetailsFactory>();

                var validationProblemDetails = problemDetailsFactory
                    .CreateValidationProblemDetails(
                        context.HttpContext,
                        context.ModelState);

                // add additional info not added by default
                validationProblemDetails.Detail = 
                    "See the errors field for details.";
                validationProblemDetails.Instance = 
                    context.HttpContext.Request.Path;

                // report invalid model state responses as validation issues
                validationProblemDetails.Type = 
                    "https://courselibrary.com/modelvalidationproblem";
                validationProblemDetails.Status = 
                    StatusCodes.Status422UnprocessableEntity;
                validationProblemDetails.Title = 
                    "One or more validation errors occurred.";

                return new UnprocessableEntityObjectResult(
                    validationProblemDetails)
                {
                    ContentTypes = { "application/problem+json" }
                };
            };
        });
```

### Validation with Custom IValidatableObject

```csharp
public abstract class CourseForManipulationDto : IValidatableObject
{
    [Required(ErrorMessage = "You should fill out a title.")]
    [MaxLength(100, ErrorMessage = "The title shouldn't have more than 100 characters.")]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1500, ErrorMessage = "The description shouldn't have more than 1500 characters.")]
    public virtual string Description { get; set; } = string.Empty;

    public IEnumerable<ValidationResult> Validate(
      ValidationContext validationContext)
    {
        if (Title == Description)
        {
            yield return new ValidationResult(
            "The provided description should be different from the title.",
            new[] { "Course" });
        }
    }
}
```

### Validation with a Custom Attribute

```csharp
[CourseTitleMustBeDifferentFromDescription]
public abstract class CourseForManipulationDto
{
    [Required(ErrorMessage = "You should fill out a title.")]
    [MaxLength(100, ErrorMessage = "The title shouldn't have more than 100 characters.")]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1500, ErrorMessage = "The description shouldn't have more than 1500 characters.")]
    public virtual string Description { get; set; } = string.Empty;    
}
```

The CourseTitleMustBeDifferentFromDescription sample code
```csharp
using CourseLibrary.API.Models;
using System.ComponentModel.DataAnnotations;

namespace CourseLibrary.API.ValidationAttributes;

public class CourseTitleMustBeDifferentFromDescriptionAttribute
    : ValidationAttribute
{
    public CourseTitleMustBeDifferentFromDescriptionAttribute()
    {
    }

    protected override ValidationResult? IsValid(object? value, 
        ValidationContext validationContext)
    {
        if (validationContext.ObjectInstance is not 
            CourseForManipulationDto course)
        {
            throw new Exception($"Attribute " +
                $"{nameof(CourseTitleMustBeDifferentFromDescriptionAttribute)} " +
                $"must be applied to a " +
                $"{nameof(CourseForManipulationDto)} or derived type.");
        }

        if (course.Title == course.Description)
        {
            return new ValidationResult(
            "The provided description should be different from the title.",
                new[] { nameof(CourseForManipulationDto) });
        }

        return ValidationResult.Success;
    }
}
```
 Even though at class level, the same rules still apply, at property level, custom attributes get executed before the Validate method gets called, and that can come in handy for property level validation.

---

## Chapter 6 - Supporting Filtering and Searching

### Filtering
Filtering allows you to be precise by adding filters until you get exactly the result you want.
In the example bellow, it is filtering the courses from the author.

<details><summary> Filter Sample Code</summary>

```csharp
//Controller
public async Task<ActionResult<IEnumerable<AuthorDto>>> GetAuthors(
    [FromQuery]string? mainCategory) // the filter parameter is optional hence the string?
{
    var coursesToReturn = await _courseLibraryRepository
        .GetAuthorsAsync(mainCategory);
    return Ok(coursesToReturn);
}

//Repository
public async Task<IEnumerable<Author>> GetAuthorsAsync(string? mainCategory)
{
    // If the filter parameter is null or whitespace, return all authors without the filter.
    if (string.IsNullOrWhiteSpace(mainCategory))
    {
        // can be a call to the repository method to get all authors
        return await _context.Authors.ToListAsync();
    })

    // remove all the spaces of the filter parameter
    mainCategory = mainCategory.Trim();

    // filter and return the result
    return await _context.Authors
        .Where(a => a.MainCategory == mainCategory)
        .ToListAsync();}

```

</details>

### Searching
Searcing allows you to go wider - it's used when you don't exactly know which items will be in the collection.

<details><summary> Search Sample Code</summary>

```csharp
//Controller
public async Task<ActionResult<IEnumerable<AuthorDto>>> GetAuthors(
    [FromQuery]string? searchQuery) // the search parameter is optional hence the string?
{
    var coursesToReturn = await _courseLibraryRepository
        .GetAuthorsAsync(searchQuery);
    return Ok(coursesToReturn);
}

// repository
public async Task<IEnumerable<Author>> GetAuthorsAsync(string? searchQuery)
{
    // Check if the search parameter is null or whitespace, return all authors without the search parameter.
    if (string.IsNullOrWhiteSpace(searchQuery)) return await GetAuthorsAsync();

    searchQuery = searchQuery.Trim();
    return await _context.Where(a => a.MainCategory.Contains(searchQuery)
            || a.FirstName.Contains(searchQuery)
            || a.LastName.Contains(searchQuery));
}
```

</details>

### Deferred Execution  

When working with Entity Framework Core, we use LINQ to build our queries. With deferred execution, the query variable itself doesn't hold the query results. 
It only stores the query commands. Execution of the query is deferred until the query variable is iterated over. So, deferred execution means that query execution occurs sometime after the query is constructed. We can get this behavior by working with `IQueryable` implementing collections. `IQueryable` of `T` allows us to execute a query against a specific data source. 
And while building upon it, it creates an expression tree. But the query itself isn't actually sent to the Datastore until iteration happens. 
Iteration can happen in different ways. 
* One way is by using an IQueryable in a loop. 
* Another way is by calling something like `ToList`, `ToArray`, or `ToDictionary` on it because that means converting the expression tree to an actual list of items. 
* And another way is by calling singleton queries. Singleton queries are queries like `average`, `count`, and `first`. Because to get to the `count` or the `first` item of an `IQueryable`, the list has to be iterated over. But as long as we can avoid that, we can build our query by, for example, adding different Where statements after each other, and we can ensure that it's only executed after that. And that is exactly what we did when combining searching with filtering.

<details><summary> 
<b>Filtering and searching combined - 2 different fashion</b>  </summary>

**First Fashion - Direct Query Parameters**
```csharp
    public async Task<ActionResult<IEnumerable<AuthorDto>>> GetAuthors(
        [FromQuery] string? mainCategory = "", string? searchQuery = "")
    {
        // throw new Exception("Test exception");

        // get authors from repo
        var authorsFromRepo = await _courseLibraryRepository
            .GetAuthorsAsync(mainCategory, searchQuery); 

        // return them
        return Ok(_mapper.Map<IEnumerable<AuthorDto>>(authorsFromRepo));
    }


//repository
public async Task<IEnumerable<Author>> GetAuthorsAsync(string? mainCategory, string? searchQuery)
    {
        if (string.IsNullOrWhiteSpace(mainCategory) 
            && string.IsNullOrWhiteSpace(searchQuery)) return await GetAuthorsAsync();

        // collection to start from
        var collection = _context.Authors as IQueryable<Author>;

        if (!string.IsNullOrWhiteSpace(mainCategory))
        {
            mainCategory = mainCategory.Trim();
            collection = collection.Where(a => a.MainCategory == mainCategory);
        }

        if (!string.IsNullOrEmpty(searchQuery))
        {
            searchQuery = searchQuery.Trim();
            collection = collection.Where(a => a.MainCategory.Contains(searchQuery)
                || a.FirstName.Contains(searchQuery)
                || a.LastName.Contains(searchQuery));
        }

        return await collection.ToListAsync();
}
```

**Second Fashion - Serialized Complex Type**

```csharp
// The URI used to request this kind of implementation. Mind the route and the query string parameters.
// http://localhost:5001/api/authors/GetAuthorsWithResourceParameters?mainCategory=Singing&searchQuery=a


// A complex type to serialize the query parameters of the URI
namespace CourseLibrary.API.ResourceParameters
{
    public class AuthorResourceParameters
    {
        public string? MainCategory { get; set; }
        public string? SearchQuery { get; set; }
    }
}

// Controller - Mind the parameter of the Action and the bind to deserialize the parameter - [FromQuery]
    [HttpGet("GetAuthorsWithResourceParameters")]
    public async Task<ActionResult<IEnumerable<AuthorDto>>> GetAuthors([FromQuery]
        AuthorResourceParameters authorResourceParameters)
    {
        // get authors from repo
        var authorsFromRepo = await _courseLibraryRepository
            .GetAuthorsAsync(authorResourceParameters);
        
        // return them
        return Ok(authorsFromRepo);
    }

// Repository - As Usual
    public async Task<IEnumerable<Author>> GetAuthorsAsync(AuthorResourceParameters authorResourceParameters)
    {
        if (authorResourceParameters == null) throw new ArgumentNullException(nameof(authorResourceParameters));

        if (string.IsNullOrWhiteSpace(authorResourceParameters.MainCategory)
            && string.IsNullOrWhiteSpace(authorResourceParameters.SearchQuery))
        {
            return await GetAuthorsAsync();
        }

        // collection to start from
        var collection = _context.Authors as IQueryable<Author>;

        if (!string.IsNullOrWhiteSpace(authorResourceParameters.MainCategory))
        {
            var mainCategory = authorResourceParameters.MainCategory.Trim();
            collection = collection.Where(a => a.MainCategory == mainCategory);
        }
        if (!string.IsNullOrEmpty(authorResourceParameters.SearchQuery))
        {
            var searchQuery = authorResourceParameters.SearchQuery.Trim();
            collection = collection.Where(a => a.MainCategory.Contains(searchQuery)
                || a.FirstName.Contains(searchQuery)
                || a.LastName.Contains(searchQuery));
        }
        return await collection.ToListAsync();
    }

```



</details>


---

## Chapter 7 - Paging

### Pagination
It's considered best practice to always implement paging on each collection resource, or at least on those that can also be created.   
The consumer is responsable to set the Pagination parameters like the page size and the page number. But the provider must set a max limit though.    
The pagination mechanism must be implemented after fetching the data from the database.  
If no paging parameters are provided, you should only return the first page by default.   
Deferred execution allows us to build up our query in the repository and only execute it when we need to. So, we can add the Skip and Take methods to the IQueryable before executing it.  


### Returning Pagination Metadata
The metadata should be returned in the response headers. The [RFC 5988](https://tools.ietf.org/html/rfc5988) defines a way to provide links to related resources in the HTTP headers.  
**If the metadata is returned along side with the result, it's not considered RESTFull API, because the message is not self-explanatory by itself**  
Mind the route and the action's name. These informations influences the creation of the next and previous page links by the URI.  

<details><summary><b>Steps used to implement pagination</b></summary>

1. In the repository class, the last instruction before the return is to execute the pagination using the generic util class PagedList.  

```csharp
    
    public async Task<PagedList<Author>> GetAuthorsAsync(AuthorResourceParameters authorResourceParameters)
    {
        if (authorResourceParameters == null) throw new ArgumentNullException(nameof(authorResourceParameters));

        // collection to start from
        var collection = _context.Authors as IQueryable<Author>;

        if (!string.IsNullOrWhiteSpace(authorResourceParameters.MainCategory))
        {
            var mainCategory = authorResourceParameters.MainCategory.Trim();
            collection = collection.Where(a => a.MainCategory == mainCategory);
        }

        if (!string.IsNullOrEmpty(authorResourceParameters.SearchQuery))
        {
            var searchQuery = authorResourceParameters.SearchQuery.Trim();
            collection = collection.Where(a => a.MainCategory.Contains(searchQuery)
                || a.FirstName.Contains(searchQuery)
                || a.LastName.Contains(searchQuery));
        }

        return await PagedList<Author>.CreateAsync(collection,
            authorResourceParameters.PageNumber,
            authorResourceParameters.PageSize);
    }
```

2. The generic util class PagedList
```csharp
namespace CourseLibrary.API.Helpers
{
    public class PagedList<T> : List<T>
    {
        public int CurrentPage { get; private set; }
        public int TotalPages { get; private set; }
        public int PageSize { get; private set; }
        public int TotalCount { get; private set; }
        public bool HasPrevious => CurrentPage > 1;
        public bool HasNext => CurrentPage < TotalPages;
        public PagedList(List<T> items, int count, int pageNumber, int pageSize)
        {
            TotalCount = count;
            PageSize = pageSize;
            CurrentPage = pageNumber;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            AddRange(items);
        }
        public static async Task<PagedList<T>> CreateAsync(
            IQueryable<T> source, int pageNumber, int pageSize)
        {
            var count = source.Count();
            var items = await source.Skip((pageNumber - 1) * pageSize)
                .Take(pageSize).ToListAsync();
            return new PagedList<T>(items, count, pageNumber, pageSize);
        }
    }
}
```

3. Helper class to be used as parameter to serialize the query string parameters of the URI, the maxPageSize, the Filter criteria (MainCategory), the Search Criteria (SearchQuery), PageSize and PageNumber  
```csharp
namespace CourseLibrary.API.ResourceParameters
{
    public class AuthorResourceParameters
    {
        const int maxPageSize = 20;
        public string? MainCategory { get; set; }
        public string? SearchQuery { get; set; }

        public int PageNumber { get; set; } = 1;
        
        private int _pageSize = 10;

        public int PageSize
        { 
            get => _pageSize; 
            
            set => _pageSize = (value > maxPageSize) ? maxPageSize : value; }
            //set => _pageSize = Math.Min(maxPageSize, value); // This algorithm has a problem when the value is 0, the page size will be 0.
        }
    }
}
```

4. Enumeration to help creating the previous and next page links
```csharp
namespace CourseLibrary.API.Helpers
{
    public enum ResourceUriType
    {
        PreviousPage,
        NextPage
    }
}
```

5. Method with the switch to create the previous and next page links and passing the mainCategory and searchQuery parameters as well.  
```csharp
    private string? CreateAuthorsResourceUri(
        AuthorResourceParameters authorResourceParameters,
        ResourceUriType type)
    {

        switch (type)
        {
            case ResourceUriType.PreviousPage:
                    return Url.Link("GetAuthorsWithResourceParameters",
                    new
                    {
                        mainCategory = authorResourceParameters.MainCategory,
                        searchQuery = authorResourceParameters.SearchQuery,
                        pageNumber = authorResourceParameters.PageNumber - 1,
                        pageSize = authorResourceParameters.PageSize
                    });
            case ResourceUriType.NextPage:
                    return Url.Link("GetAuthorsWithResourceParameters",
                    new
                    {
                        mainCategory = authorResourceParameters.MainCategory,
                        searchQuery = authorResourceParameters.SearchQuery,
                        pageNumber = authorResourceParameters.PageNumber + 1,
                        pageSize = authorResourceParameters.PageSize
                    });
            default:
                    return Url.Link("GetAuthorsWithResourceParameters",
                    new
                    {
                        mainCategory = authorResourceParameters.MainCategory,
                        searchQuery = authorResourceParameters.SearchQuery,
                        pageNumber = authorResourceParameters.PageNumber,
                        pageSize = authorResourceParameters.PageSize
                    });
        }
    }

```

6. The action creates the previous and next page links, if applicable, and adds them to the pagination metadata object. The pagination metadata object is then serialized to JSON and added to the response headers under the X-Pagination key. Finally, the method returns the authors from the repository as usual.
After retrieving the data from the repository, it creates the previous and next page links using the CreateAuthorsResourceUri method. This method constructs the appropriate URI based on the current page number, page size, and any filtering or searching criteria provided in the AuthorResourceParameters object.
Before returning the authors, it adds the pagination metadata to the response headers. This metadata includes information such as total count, page size, current page, total pages, and the previous and next page links if applicable. The metadata is serialized to JSON format and added to the X-Pagination header.
```csharp
    [HttpGet("GetAuthorsWithResourceParameters", Name = "GetAuthorsWithResourceParameters")]
    public async Task<ActionResult<IEnumerable<AuthorDto>>> GetAuthorsWithResourceParameters([FromQuery]
        AuthorResourceParameters authorResourceParameters)
    {
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
        return Ok(authorsFromRepo);
    }
```
</details>

---

## Chapter 8 - Suporting Sorting
The sorting in the this algorythm is happens in the repository layer but it is checked in the service layer though.  
In the service layer happens the mapping between the *DTO* and the *Entity*. The clients requires a sorting by name for example, the entity doesn't know anything about *name*, the entity knows about *first* and *last* name.  

<details><summary><b>Sorting Implementation Steps</b></summary>

1. Class used by the controller action to serialize the query string parameters of the URI. It contains the Filter criteria (MainCategory), the Search Criteria (SearchQuery), PageSize, PageNumber and **OrderBy** for sorting.  
The **orderBy** parameter is optional, and if not provided, the default sorting is by Name.  
This is the same class used in the pagination, searching, filtering implementations.
```csharp
public class AuthorResourceParameters
    {
        const int maxPageSize = 20;
        public string? MainCategory { get; set; }
        public string? SearchQuery { get; set; }

        public int PageNumber { get; set; } = 1;
        
        private int _pageSize = 10;

        public int PageSize
        { 
            get => _pageSize; 
            
            //set => _pageSize = (value > maxPageSize) ? maxPageSize : value; }
            set => _pageSize = Math.Min(maxPageSize, value); 
        }
        
        // Property used for sorting
        public string OrderBy { get; set; } = "Name";
    }
```

2. In the controller, the first *if* checks if the sorting parameter exists, it not, it returns to the client a 400 status coding stating that the sortby must be a valid parameter.  
```csharp
    [HttpGet(Name ="GetAuthors")]
    public async Task<ActionResult<IEnumerable<AuthorDto>>> GetAuthors([FromQuery]
        AuthorResourceParameters authorResourceParameters)
    {
        // Check for a valid sorting parameter
        if (!_propertyMappingService
            .ValidMappingExistsFor<AuthorDto, Entities.Author>(
            authorResourceParameters.OrderBy))
        {
            return BadRequest();
        }

        // get authors from repo
        var authorsFromRepo = await _courseLibraryRepository
            .GetAuthorsAsync(authorResourceParameters);

        // Same code as the example above for pagination, searching and filtering creations.
        return Ok(_mapper.Map<IEnumerable<AuthorDto>>(authorsFromRepo));
    }
```

3. In the repository, after the **filtering** and **searching**, but before the **pagination**, the **sorting** is applied.
```csharp
    public async Task<PagedList<Author>> GetAuthorsAsync(AuthorResourceParameters authorResourceParameters)
    {
        if (authorResourceParameters == null) throw new ArgumentNullException(nameof(authorResourceParameters));
        
        // collection to start from
        var collection = _context.Authors as IQueryable<Author>;

        // Same as the example above for filtering, pagination and searching creations.

        if (!string.IsNullOrWhiteSpace(authorResourceParameters.OrderBy))
        {
            // get property mapping dictionary
            var authorPropertyMappingDictionary =
                _propertyMappingService.GetPropertyMapping<AuthorDto, Author>();
            
            // apply sorting call
            collection = collection.ApplySort(authorResourceParameters.OrderBy,
                authorPropertyMappingDictionary);            
        }

        return await PagedList<Author>.CreateAsync(collection,
            authorResourceParameters.PageNumber,
            authorResourceParameters.PageSize);
    }
```

4. The **ApplySort** extension method used to apply the sorting to the IQueryable collection.   

The method uses the **System.Linq.Dynamic.Core** package to apply the sorting.  

```csharp
using System.Linq.Dynamic.Core;

namespace CourseLibrary.API.Helpers;

public static class IQueryableExtensions
{
    public static IQueryable<T> ApplySort<T>(
        this IQueryable<T> source,
        string orderBy,
        Dictionary<string, PropertyMappingValue> mappingDictionary)
    {
    // ******************* Begining of the first part of the algorythm *******************
    // Checks if the source is null, if the mappingDictionary is null and if the orderBy parameter is null or whitespace.
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        if (mappingDictionary == null)
        {
            throw new ArgumentNullException(nameof(mappingDictionary));
        }

        if (string.IsNullOrWhiteSpace(orderBy))
        {
            return source;
        }
    // ******************* End of the first part of the algorythm *******************

    // ******************* Begining of the second part of the algorythm *******************        

        var orderByString = string.Empty;

        // the orderBy string is separated by ",", so we split it.
        var orderByAfterSplit = orderBy.Split(',');
        
        //apply each orderby clause
        foreach (var orderByClause in orderByAfterSplit)
        {
            // trim the orderBy clause, as it might contain leading
            // or trailing spaces. Can't trim the var in the foreach,
            // so we use a new var.
            var trimmedOrderByClause = orderByClause.Trim();

            // if the sort option ends with " desc", we order
            // descending, otherwise ascending
            var orderDescending = trimmedOrderByClause.EndsWith(" desc");

            // remove " asc" or " desc" from the orderBy clause, so we
            // get the property name to look for in the mapping dictionary
            var indexOfFirstSpace = trimmedOrderByClause.IndexOf(" ");
            var propertyName = indexOfFirstSpace == -1 ?
                trimmedOrderByClause : trimmedOrderByClause
                .Remove(indexOfFirstSpace);

            // find the matching property
            if (!mappingDictionary.ContainsKey(propertyName))
            {
                throw new ArgumentException($"Key mapping for {propertyName} is missing");
            }

            // get the PropertyMappingValue
            var propertyMappingValue = mappingDictionary[propertyName];
            
            if (propertyMappingValue == null)
            {
                throw new ArgumentNullException(nameof(propertyMappingValue));
            }

            // revert sort order if necessary
            if (propertyMappingValue.Revert)
            {
                orderDescending = !orderDescending;
            }

            // Run through the properties names
            foreach (var destinationProperty in 
                propertyMappingValue.DestinationProperties)
            {
                orderByString = orderByString +
                    (string.IsNullOrWhiteSpace(orderByString) ? string.Empty : ", ")
                    + destinationProperty
                    + (orderDescending ? " descending" : " ascending");
            }
        }
    // ******************* End of the second part of the algorythm *******************

    // ******************* Begining of the third part of the algorythm *******************
        
        // apply the orderby string to the source        
        return source.OrderBy(orderByString);
        
    // ******************* End of the third part of the algorythm *******************
    }
}

```

5. The Interface IPropertyMapping used as **Mark Interface** in the PropertyMapping class.
```csharp
namespace CourseLibrary.API.Services;

public interface IPropertyMapping
{
}
```

6. The **PropertyMapping** class that inherits from the IPropertyMapping interface. It holds the mapping dictionary between the DTO and the Entity.
```csharp
namespace CourseLibrary.API.Services;

public class PropertyMapping<TSource, TDestination> : IPropertyMapping
{
    public Dictionary<string, PropertyMappingValue> MappingDictionary { get; private set; }
    public PropertyMapping(Dictionary<string, PropertyMappingValue> mappingDictionary)
    {
        MappingDictionary = mappingDictionary ??
            throw new ArgumentNullException(nameof(mappingDictionary));
    }
}

```

7. The **PropertyMappingValue**   
```csharp
public class PropertyMappingValue
{
    public IEnumerable<string> DestinationProperties { get; private set; }
    public bool Revert { get; private set; }

    public PropertyMappingValue(IEnumerable<string> destinationProperties, bool revert = false)
    {
        DestinationProperties = destinationProperties ??
            throw new ArgumentNullException(nameof(destinationProperties));
        Revert = revert;
    }
}
```

8. The **PropertyMappingService** class that holds the mapping dictionary and the methods to get the mapping dictionary and to check if the sorting parameter exists in the mapping dictionary.  
8. The service is this case is used to validate if it's applicable to sort by the parameter provided by the client or not.  
```csharp

namespace CourseLibrary.API.Services;

public class PropertyMappingService : IPropertyMappingService
{
    private readonly Dictionary<string, PropertyMappingValue> _authorPropertyMapping =
        new (StringComparer.OrdinalIgnoreCase)
        {
            { "Id", new (new [] { "Id" }) },
            { "MainCategory", new (new [] { "MainCategory" }) },
            { "Age", new (new [] { "DateOfBirth" }, true) },
            { "Name", new (new [] { "FirstName", "LastName" }) }
        };

    private readonly IList<IPropertyMapping> _propertyMappings = new List<IPropertyMapping>();
    public PropertyMappingService()
    {
        _propertyMappings.Add(new PropertyMapping<AuthorDto, Author>(
            _authorPropertyMapping));
    }

    public Dictionary<string, PropertyMappingValue> GetPropertyMapping<TSource, TDestination>()
    {
        // get matching mapping
        var matchingMapping = _propertyMappings
            .OfType<PropertyMapping<TSource, TDestination>>();

        if (matchingMapping.Count() ==1)
        {
            return matchingMapping.First().MappingDictionary;
        }

        throw new Exception($"Cannot find exact property mapping instance " +
            $"for <{typeof(TSource)},{typeof(TDestination)}>");        
    }

    public bool ValidMappingExistsFor<TSource, TDestination>(string fields)
    {
        var propertyMapping = GetPropertyMapping<TSource, TDestination>();

        if (string.IsNullOrWhiteSpace(fields))
        {
            return true;
        }

        // the string is separated by ",", so we split it.
        var fieldsAfterSplit = fields.Split(',');

        // run through the fields clauses
        foreach (var field in fieldsAfterSplit)
        {
            // trim
            var trimmedField = field.Trim();

            // remove everything after the first " " - if the fields
            // are coming from an orderBy string, this part must be
            // ignored

            var indexOfFirstSpace = trimmedField.IndexOf(" ");
            var propertyName = indexOfFirstSpace == -1 ?
                trimmedField : trimmedField.Remove(indexOfFirstSpace);

            // find the matching property
            if (!propertyMapping.ContainsKey(propertyName))
            {
                return false;
            }
        }
        return true;
    }
}
```


</details>

--- 

<details>
<summary>

## Other</summary>

<details><summary>

### Version(s) </summary>

20251002 - 1st Version

</details>

<details><summary>
### Libraries</summary>

1. AutoMapper.Extensions.Microsoft.DependencyInjection - v12.0.1  
2. Microsoft.AspNetCore.JsonPatch - v9.0.9  
3. Microsoft.AspNetCore.Mvc.NewtonsoftJson - v8.0.0  
4. System.Linq.Dynamic.Core - v1.3.7  

</details>

</details>


```csharp
```