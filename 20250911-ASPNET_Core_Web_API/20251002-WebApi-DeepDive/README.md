# [ASP.NET Core Web Api Deep Dive](https://app.pluralsight.com/library/courses/asp-dot-net-core-6-web-api-deep-dive)

* [Chapter 3 - Desiging The Outer Facing Contract]([#chapter-3---designing-the-outer-facing-contract)   
* [Chapter 4 - Manipulating Resources](#chapter-4---manipulating-resources)     
* [Chapter 5 - Validating Data and Reporting Validation Errors](#chapter-5---validating-data-and-reporting-validation-errors)   
* [Chapter 6 - Supporting Filtering and Searching](#chapter-6---supporting-filtering-and-searching)   
* [Chapter 7 - Paging](#chapter-7---paging)   
* [Chapter 8 - Supporting Sorting](#chapter-8---supporting-sorting)   
* [Chapter 9 - Supporting Data Shaping](chapter-9---supporting-data-shaping)     
* [Chapter 10 - Learning and Implenting HATEOAS](#chapter-10---learning-and-implementing-hateoas)   
* [Chapter 11 - Combining HATEOAS with Semantic Media Types](#chapter-11---combining-hateoas-with-semantic-media-types)   
* [Chapter 12 - Caching](#chapter-12---caching)
* [Chapter 13 - Supporting Http Cache for AspNet Core APIs](#chapter-13---supporting-http-cache-for-aspnet-core-apis)
* [Chapter 14 - Supporting Concurrency](#chapter-14---supporting-concurrency)  

## Chapter 3 - Designing the Outer Facing Contract
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

<details><summary>

### Creating a set of Father items along side with its children on one go.
</summary>

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

The model binder example
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
</details>

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


<details><summary>Sample of `OPTIONS` implementation  </summary>

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
</details>

### HTTP method overview by use case
|Resource|Route|Responses|Possible Statuses|Obs|
|-|-|-|-|-|
|Reading Resources|GET `api/authors`|`[{author},{author}]` |200, 404|-|
|Reading Resources|GET `api/authors/{authorId}`|`{author}` |200, 404|-|
|Deleting Resources|DELETE `api/authors/{authorId}`|-|204, 404|-|
|Deleting Resources|DELETE `api/authors/`|-|204, 404|Rarely implemented|
|Creating Resources (server)|POST `api/authors` - Body: `{author}`|`{author}`|201, 404|-|
|Creating Resources (server)|POST `api/authors/{authorId}`|-|404 or 409|can never be successful|
|Creating Resources (server)|POST `api/authorcollections` - Body: `{authorCollection}`|`{authorCollection}`|201, 404|Create a resource for adding a collection in one go|
|Creating Resources (consumer)|PUT `api/authors/{authorId}` - Body: `{author}`|`{author}`|201|-|
|Creating Resources (consumer)|PATCH `api/authors/{authorId}` - Body: `{JsonPatchDocument on author}`|`{author}`|201|-|
|Updating Resources (full)|PUT `api/authors/{authorId}` - Body: `{author}`|`{author}`|200, 204, 404|-|
|Updating Resources (full)|PUT `api/authors/{authorId}` - Body: `[{author},{author}]`|`[{author},{author}]`|200, 204, 404|Rarely implemented|
|Updating Resources (partial)|PATCH `api/authors/{authorId}` - Body: `{JsonPatchDocument on author}`|`{author}`|200, 204, 404|-|
|Updating Resources (partial)|PATCH `api/authors` - Body: `{JsonPatchDocument on authors}`|`[{author},{author}]`|200, 204, 404|Rarely implemented|





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

 <details><summary><b>Validations Sample Codes</b></summary>

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

 </details>

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

## Chapter 8 - Supporting Sorting
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

2. In the controller, the first *if* checks if the sorting parameter exists, if not, it returns to the client a 400 status coding stating that the sortby must be a valid parameter.
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

## Chapter 9 - Supporting Data Shaping
**Data shaping allows the consumer of the API to choose the fields of the resource that have to be returned.**
This principle allows the consumer of the API to choose the fields of the resource representation that have to be returned.

So, rather than returning all properties of an author, a consumer of an API might only want to know the ID and the name.
Data shaping allows for this by looking at a fields query string parameter of which the value is a comma‑separated list of field names.

The field names passed in as value of the field's query string parameter should exist on the resource. So, for our author, a field‑level selection on age is valid, as the authors resource has an age, but one on date of birth is not valid, as an authors resource does not have that. The date of birth is defined on the entity and not at level of the outer‑facing contract.

When shaping data to return, not always we will be able to use strongly typed objects. To handle this scenario, we need a way to dynamically create an object at runtime. That's where the `ExpandoObject` comes in handy. It's defined in `System.Dynamic`. Its members can be added and removed at runtime.

When returning a collection of resources, we can use strongly typed objects, but when returning a single resource, we might not be able to do that.

**Caveats**
When implementing Data Shaping, keep in mind that it might violate the sub-constraint of REST: "Manipulation of Resources Through Representations".
To avoid this, make sure that the representation returned to the client contains all the necessary information to manipulate the resource, such as its URI.
Another approach is to implement HATEOAS, which provides links to related resources and actions, ensuring that the client has enough context to interact with the resource effectively.
And finally, makes sure to create a mechanism to avoid 500 status code errors when the client requests fields that do not exist on the resource.

<details><summary><b>Data Shaping Implementation Steps</b></summary>

1. The Resource Parameters class to carry the fields selected by the user in the query parameters of the URI. The last property is the `Fields`

```csharp
namespace CourseLibrary.API.ResourceParameters;

public class AuthorsResourceParameters
{
    const int maxPageSize = 20;
    public string? MainCategory { get; set; }
    public string? SearchQuery { get; set; }
    public int PageNumber { get; set; } = 1;

    private int _pageSize = 10;
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = (value > maxPageSize) ? maxPageSize : value;
    }
    public string OrderBy { get; set; } = "Name";

    public string? Fields { get; set; }
}
```

2. The extension helper class to shape the data using ExpandoObject

```csharp
using System.Dynamic;
using System.Reflection;

namespace CourseLibrary.API.Helpers;

public static class IEnumerableExtensions
{
    public static IEnumerable<ExpandoObject> ShapeData<TSource>(
            this IEnumerable<TSource> source,
            string? fields)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        // create a list to hold our ExpandoObjects
        var expandoObjectList = new List<ExpandoObject>();

        // create a list with PropertyInfo objects on TSource.  Reflection is
        // expensive, so rather than doing it for each object in the list, we do
        // it once and reuse the results.  After all, part of the reflection is on the
        // type of the object (TSource), not on the instance
        var propertyInfoList = new List<PropertyInfo>();

        if (string.IsNullOrWhiteSpace(fields))
        {
            // all public properties should be in the ExpandoObject
            var propertyInfos = typeof(TSource)
                    .GetProperties(BindingFlags.IgnoreCase
                    | BindingFlags.Public | BindingFlags.Instance);

            propertyInfoList.AddRange(propertyInfos);
        }
        else
        {       // the field are separated by ",", so we split it.
            var fieldsAfterSplit = fields.Split(',');

            foreach (var field in fieldsAfterSplit)
            {
                // trim each field, as it might contain leading
                // or trailing spaces. Can't trim the var in foreach,
                // so use another var.
                var propertyName = field.Trim();

                // use reflection to get the property on the source object
                // we need to include public and instance, b/c specifying a binding
                // flag overwrites the already-existing binding flags.
                var propertyInfo = typeof(TSource)
                    .GetProperty(propertyName, BindingFlags.IgnoreCase |
                    BindingFlags.Public | BindingFlags.Instance);

                if (propertyInfo == null)
                {
                    throw new Exception($"Property {propertyName} wasn't found on" +
                        $" {typeof(TSource)}");
                }

                // add propertyInfo to list
                propertyInfoList.Add(propertyInfo);
            }
        }

        // run through the source objects
        foreach (TSource sourceObject in source)
        {
            // create an ExpandoObject that will hold the
            // selected properties & values
            var dataShapedObject = new ExpandoObject();

            // Get the value of each property we have to return.  For that,
            // we run through the list
            foreach (var propertyInfo in propertyInfoList)
            {
                // GetValue returns the value of the property on the source object
                var propertyValue = propertyInfo.GetValue(sourceObject);

                // add the field to the ExpandoObject
                ((IDictionary<string, object?>)dataShapedObject)
                    .Add(propertyInfo.Name, propertyValue);
            }

            // add the ExpandoObject to the list
            expandoObjectList.Add(dataShapedObject);
        }

        // return the list
        return expandoObjectList;
    }
}
```

3. Implementation for an object. For sake of perfomance, we implement a different method for single objects. Since using `Reflection` is costly in terms of performance.
```csharp
using System.Dynamic;
using System.Reflection;

namespace CourseLibrary.API.Helpers;

public static class ObjectExtensions
{
    public static ExpandoObject ShapeData<TSource>(this TSource source,
     string? fields)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        var dataShapedObject = new ExpandoObject();

        if (string.IsNullOrWhiteSpace(fields))
        {
            // all public properties should be in the ExpandoObject
            var propertyInfos = typeof(TSource)
                    .GetProperties(BindingFlags.IgnoreCase |
                    BindingFlags.Public | BindingFlags.Instance);

            foreach (var propertyInfo in propertyInfos)
            {
                // get the value of the property on the source object
                var propertyValue = propertyInfo.GetValue(source);

                // add the field to the ExpandoObject
                ((IDictionary<string, object?>)dataShapedObject)
                    .Add(propertyInfo.Name, propertyValue);
            }

            return dataShapedObject;
        }

        // the field are separated by ",", so we split it.
        var fieldsAfterSplit = fields.Split(',');

        foreach (var field in fieldsAfterSplit)
        {
            // trim each field, as it might contain leading
            // or trailing spaces. Can't trim the var in foreach,
            // so use another var.
            var propertyName = field.Trim();

            // use reflection to get the property on the source object
            // we need to include public and instance, b/c specifying a
            // binding flag overwrites the already-existing binding flags.
            var propertyInfo = typeof(TSource)
                .GetProperty(propertyName,
                BindingFlags.IgnoreCase | BindingFlags.Public |
                BindingFlags.Instance);

            if (propertyInfo == null)
            {
                throw new Exception($"Property {propertyName} wasn't found " +
                    $"on {typeof(TSource)}");
            }

            // get the value of the property on the source object
            var propertyValue = propertyInfo.GetValue(source);

            // add the field to the ExpandoObject
            ((IDictionary<string, object?>)dataShapedObject)
                .Add(propertyInfo.Name, propertyValue);
        }

        // return the shaped object
        return dataShapedObject;
    }

}

```

4. Change the controller to return `IActionResult`, this is to allow returning different types of objects, thereby accepting the `ExpandoObject` returned by the `ShapeData` method.
The first instruction is to check if the fields provided by the client exists on the resource. If not, is called the factory method to create a `ProblemDetails` object and return a 400 status code to the client.
```csharp
    [HttpGet("{authorId}", Name = "GetAuthor")]
    public async Task<IActionResult> GetAuthor(Guid authorId,
        string? fields)
    {
        if (!_propertyCheckerService.TypeHasProperties<AuthorDto>
           (fields))
        {
            return BadRequest(
              _problemDetailsFactory.CreateProblemDetails(HttpContext,
                  statusCode: 400,
                  detail: $"Not all requested data shaping fields exist on " +
                  $"the resource: {fields}"));
        }

        // get author from repo
        var authorFromRepo = await _courseLibraryRepository
            .GetAuthorAsync(authorId);

        if (authorFromRepo == null)
        {
            return NotFound();
        }

        // return author
        return Ok(_mapper.Map<AuthorDto>(authorFromRepo)
            .ShapeData(fields));
    }
```

5. Class to validate if the fields provided by the client exists on the resource. Create a interface and register in the DI container to it could be injected in the controller.
```csharp
using System.Reflection;

namespace CourseLibrary.API.Services;

public class PropertyCheckerService : IPropertyCheckerService
{
    public bool TypeHasProperties<T>(string? fields)
    {
        if (string.IsNullOrWhiteSpace(fields))
        {
            return true;
        }

        // the field are separated by ",", so we split it.
        var fieldsAfterSplit = fields.Split(',');

        // check if the requested fields exist on source
        foreach (var field in fieldsAfterSplit)
        {
            // trim each field, as it might contain leading
            // or trailing spaces. Can't trim the var in foreach,
            // so use another var.
            var propertyName = field.Trim();

            // use reflection to check if the property can be
            // found on T.
            var propertyInfo = typeof(T)
                .GetProperty(propertyName,
                BindingFlags.IgnoreCase | BindingFlags.Public |
                BindingFlags.Instance);

            // it can't be found, return false
            if (propertyInfo == null)
            {
                return false;
            }
        }

        // all checks out, return true
        return true;

    }
}
```

6. Controller with the injected `IPropertyCheckerService` and the `ProblemDetailsFactory` from the `Microsoft.AspNetCore.Mvc.Infrastructure` namespace to create the `ProblemDetails` object to return to the client in case of error. In the code snippet #4, of this very section, there is a example to use it.
```csharp
    public AuthorsController(ICourseLibraryRepository courseLibraryRepository,
        IMapper mapper, IPropertyMappingService propertyMappingService,
        IPropertyCheckerService propertyCheckerService,
        ProblemDetailsFactory problemDetailsFactory)
    {
        _courseLibraryRepository = courseLibraryRepository ??
            throw new ArgumentNullException(nameof(courseLibraryRepository));
        _mapper = mapper ??
            throw new ArgumentNullException(nameof(mapper));
        _propertyMappingService = propertyMappingService ??
            throw new ArgumentNullException(nameof(propertyMappingService));
        _propertyCheckerService = propertyCheckerService ??
            throw new ArgumentNullException(nameof(propertyCheckerService));
        _problemDetailsFactory = problemDetailsFactory ??
            throw new ArgumentNullException(nameof(problemDetailsFactory));
    }
```

7. Request passing the required fields and the pagenumber.
```json
http://localhost:5000/api/authors?fields=id,name&pageSize=2&pageNumber=1
```

</details>


<details><summary>Pending</summary>

- Others implementation of the Data Shaping
- Implementing Data Shaping for children objects

</details>

---

## Chapter 10 - Learning and implementing HATEOAS
*You can't have evolvability if clients have their controls baked into their design at deployment. Controls have to be learned on the fly. That's what hypermedia enables.*
[Roy Fielding](https://www.infoq.com/articles/roy-fielding-on-versioning/)

**Supporting HATEOAS**
`<a href="uri" rel="type" type="media type">`
HTML represents links with the anchor element
- href: contains the URI
- rel: describes how the link relates to the resource
- type: describes the media type

```json
{
    "links":
    [{
        "href": "http://localhost:5000/api/authors/1",
        "rel":"reserve-course",
        "method": "POST"
        }]
}
```

For complex responses with collections, we need a kind of envelop to hold a list of resources
```json
{
    "value":[{"author":"A"},{"author":"B"}],
    "links":[{...},{...}]
}
```

|Statically typed approach|Dynamically typed approach|
|-|-|
|Base class (with links) and wrapper class|Anonymous types & ExpandoObject|
|Inherit base class for single resources|Add links to ExpandoObject for single resources|
|Use wrapper class for collection resources|Use anonymous type for collection resources|

<details><summary><b>Sample code to implement HATEOAS</b></summary>

Simple case, with only one object on the response.

1. Model class for LinkDto
```csharp
namespace CourseLibrary.API.Models;

public class LinkDto
{
    public LinkDto(string? href, string? rel, string method)
    {
        Href = href;
        Rel = rel;
        Method = method;
    }

    public string? Href { get; }
    public string? Rel { get; }
    public string Method { get; }
}
```

2. Create a method to create the links to be used as the response.
The first one is `_self`. It's the link to the resource itself, the rest of the links are related to the possible actions on the resource, like retrieving the courses or create a course for the author.
**It's the place to define which links should return to the client based on business rules.**.
```csharp
private IEnumerable<LinkDto> CreateLinksForResponse(Guid authorId, string? fields)
{
    var links = new List<LinkDto>();
    if (string.IsNullOrWhiteSpace(fields))
    {
        links.Add(
            new LinkDto(Url.Link("GetAuthor", new { authorId }),
            "self",
            "GET"));
    }
    else
    {
        // Fields here could be verified by inspecting the fields parameter
        links.Add(
            new LinkDto(Url.Link("GetAuthor", new { authorId, fields }),
            "self",
            "GET"));
    }

    // Link to create a course for the author. The second parameter is the route name in the Course Controller for the method to get the courses of the author.
    links.Add(
    new (Url.Link("CreateCourseForAuthor", new { authorId }),
    "create_course_for_author",
    "POST"
    ));

    // Link to get all courses for the author. Same think for the route name as the rel value "courses".
    links.Add(
    new (Url.Link("GetCoursesForAuthor", new { authorId }),
    "courses",
    "GET"));

    return links;
}
```

3. The action code to create the links to return to the client.
After the creation of the resource to be returned for the client, is created a variable called `links` that calls the method to create the links.
Then, the resource to be returned is mapped to a DTO and shaped using the `ShapeData` extension method. The result is casted to an `IDictionary<string, object?>` to allow adding the links to it.
Finally, the links are added to the resource and returned to the client.
```csharp
[HttpGet("{authorId}", Name = "GetAuthor")]
    public async Task<IActionResult> GetAuthor(Guid authorId,
        string? fields)
    {
        ....

        // create links
        var links = CreateLinksForAuthor(authorId, fields);

        //add
        var linkedResourceToReturn = _mapper.Map<AuthorDto>(authorFromRepo)
            .ShapeData(fields) as IDictionary<string, object?>;

        linkedResourceToReturn.Add("links", links);

        // return
        return Ok(linkedResourceToReturn);
    }
```
</details>

<details><summary><b>Example of HATEOAS with Complex Type</b></summary>

1. Method to create links with the self using the default logic. The *ResourceUriType* is a enumerator type.
With this routine, there is no need to send pagination on response header.
```csharp
private string? CreateAuthorsResourceUri(
        AuthorsResourceParameters authorsResourceParameters,
        ResourceUriType type)
    {
        switch (type)
        {
            case ResourceUriType.PreviousPage:
                return Url.Link("GetAuthors",
                    new
                    {
                        fields = authorsResourceParameters.Fields,
                        orderBy = authorsResourceParameters.OrderBy,
                        pageNumber = authorsResourceParameters.PageNumber - 1,
                        pageSize = authorsResourceParameters.PageSize,
                        mainCategory = authorsResourceParameters.MainCategory,
                        searchQuery = authorsResourceParameters.SearchQuery
                    });
            case ResourceUriType.NextPage:
                return Url.Link("GetAuthors",
                    new
                    {
                        fields = authorsResourceParameters.Fields,
                        orderBy = authorsResourceParameters.OrderBy,
                        pageNumber = authorsResourceParameters.PageNumber + 1,
                        pageSize = authorsResourceParameters.PageSize,
                        mainCategory = authorsResourceParameters.MainCategory,
                        searchQuery = authorsResourceParameters.SearchQuery
                    });
            case ResourceUriType.Current:
            default:
                return Url.Link("GetAuthors",
                    new
                    {
                        fields = authorsResourceParameters.Fields,
                        orderBy = authorsResourceParameters.OrderBy,
                        pageNumber = authorsResourceParameters.PageNumber,
                        pageSize = authorsResourceParameters.PageSize,
                        mainCategory = authorsResourceParameters.MainCategory,
                        searchQuery = authorsResourceParameters.SearchQuery
                    });
        }
    }
```

2. Method to create the self, next and previous links. The method itself calls the previous method shown above on section #1 of this examples.
```csharp
    private IEnumerable<LinkDto> CreateLinksForAuthors(AuthorsResourceParameters authorsResourceParameters, bool hasNext, bool hasPrevious)
    {
        var links = new List<LinkDto>();
        // self
        links.Add(new (CreateAuthorsResourceUri(authorsResourceParameters, ResourceUriType.Current), "self", "GET"));

        if (hasNext)
        {
            links.Add(new LinkDto(CreateAuthorsResourceUri(authorsResourceParameters, ResourceUriType.NextPage), "nextPage", "GET"));
        }

        if (hasPrevious)
        {
            links.Add(new LinkDto(CreateAuthorsResourceUri(authorsResourceParameters, ResourceUriType.PreviousPage), "previousPage", "GET"));
        }
        return links;
    }
```

3. Action with the calling to the HATEOAS routine.
```csharp
[HttpGet(Name = "GetAuthors")]
    [HttpHead]
    public async Task<IActionResult> GetAuthors(
        [FromQuery] AuthorsResourceParameters authorsResourceParameters)
    {
        // after the retrieval of the data from the layers beneath the controller, is called the
        var authorsFromRepo = await _courseLibraryRepository
            .GetAuthorsAsync(authorsResourceParameters);

        // pagination meta data used in the response's header
        var paginationMetadata = new
        {
            totalCount = authorsFromRepo.TotalCount,
            pageSize = authorsFromRepo.PageSize,
            currentPage = authorsFromRepo.CurrentPage,
            totalPages = authorsFromRepo.TotalPages,
        };

        // serialization of the metadata in the header
        Response.Headers.Add("X-Pagination",
               JsonSerializer.Serialize(paginationMetadata));

        // create links to be used in the response roots to indicate the next possible requests to the API
        var links = CreateLinksForAuthors(authorsResourceParameters, authorsFromRepo.HasNext, authorsFromRepo.HasPrevious);

        // The data from the layers beneath the controller to be shaped onto the response.
        var shapedAuthors = _mapper.Map<IEnumerable<AuthorDto>>(authorsFromRepo)
                    .ShapeData(authorsResourceParameters.Fields);

        // building of the shaped authors to be returned with the links.
        var shapedAuthorsWithLinks = shapedAuthors.Select(author =>
        {
            var authorAsDictionary = author as IDictionary<string, object?>;
            var authorLinks = CreateLinksForAuthor((Guid)authorAsDictionary["Id"], null);
            authorAsDictionary.Add("links", authorLinks);
            return authorAsDictionary;
        });

        // final building before the response to be sent to the client.
        var linkedCollectionResource = new
        {
            value = shapedAuthorsWithLinks,
            links = links
        };

        // return them
        return Ok(linkedCollectionResource);
    }
```


</details>

### Root Document
For this kind of document, the client can learn how to interact with the rest of the API. This document will live at the API root, so host/api.
It's an empty API controller, generally named RootController.
It should be executed on a GET request to /api and contains links to the document itself and links to actions that can happen on URIs at root level or that are not accessible otherwise.
From this root document, consumers of the API can start interacting with the API.

<details><summary><b>Example of `Root` implementation</b></summary>

```csharp
using CourseLibrary.API.Models;
using Microsoft.AspNetCore.Mvc;

namespace CourseLibrary.API.Controllers;

[Route("api")]
[ApiController]
public class RootController : ControllerBase
{
    [HttpGet(Name = "GetRoot")]
    public IActionResult GetRoot()
    {
        // create links for root
        var links = new List<LinkDto>();

        links.Add(
          new(Url.Link("GetRoot", new { }),
          "self",
          "GET"));

        links.Add(
          new(Url.Link("GetAuthors", new { }),
          "authors",
          "GET"));

        links.Add(
          new(Url.Link("CreateAuthor", new { }),
          "create_author",
          "POST"));

        return Ok(links);
    }
}
```

</details>

###  Other options to implement HATEOAS or attempts to standardization of API responses.
[HAL - Hyperlink As Language](datatracker.ietf.org/doc/html/draft-kelly-json-hal-11)   
[Siren - Hypermedia specification for representing entities](github.com/kevinwiber/siren)   
[NHateoas - Copilot Suggestion](github.com/JeremySkinner/NHateoas)   
[NHateoas](github.com/yuri-sannikov/NHateoas)   
[JSON for Linking Data](https://json-ld.org)  
[JSON api](https://jsonapi.org)  
[OData - OASIS](www.odata.org)  

---

## Chapter 11 - Combining HATEOAS with Semantic Media Types

### Semantic Media Types
“A REST API should spend almost all of its descriptive effort in defining the media type(s) used for representing resources and driving application state, or in defining extended relation names and/or hypertext-enabled mark-up for existing standard media types.”
[Roy Fielding,](https://roy.gbiv.com/untangled/2008/rest-apis-must-be-hypertext-driven)

*application/json* tells us something about the *format* of the data, but not about the *type*

Media types that thell something about the semantics of the data, in other words: *what the data means*.

**Vendor-specific Media Types**
`application/vnd.nilo.hateoas+json`
1. application => Top-level type
2. vnd => Vendor-specific
3. nilo => Vendor identifier
4. hateoas => Media type name
5. json => Suffix

<details><summary><b>Setup a specific media type to be processed</b></summary>

Action with header parameter to process the header's value `Accept`. If the header doesn't pass the first check the client will receive a bad request reponse.
At the end of the action, if the client passed the header it will receive the hateoas links otherwise it will receive only the values.
```csharp
    [HttpGet("{authorId}", Name = "GetAuthor")]
    public async Task<IActionResult> GetAuthor(Guid authorId,
        string? fields,
        [FromHeader(Name = "Accept")] string? mediaType)
    {

        if (!MediaTypeHeaderValue.TryParse(mediaType, out var parsedMediaType))
        {
            return BadRequest(_problemDetailsFactory.CreateProblemDetails(HttpContext, statusCode: 400,
                detail: $"Accept header media type is not a valid media type."));
        }

        if(parsedMediaType.MediaType == "application/vnd.nilo.hateoas+json")
        {
            // create links
            var links = CreateLinksForAuthor(authorId, fields);

            //add
            var linkedResourceToReturn = _mapper.Map<AuthorDto>(authorFromRepo)
                .ShapeData(fields) as IDictionary<string, object?>;

            linkedResourceToReturn.Add("links", links);

            // return 
            return Ok(linkedResourceToReturn);
        }
        return Ok(_mapper.Map<AuthorDto>(authorFromRepo));

    }
```

2. Set the right content negotiation type on the response. After the configuration of the controller adds the middleware configuration below

```csharp
builder.Services.Configure<MvcOptions>(config =>
        {
            var newtonsoftJsonOutputFormatter = config.OutputFormatters.OfType<NewtonsoftJsonOutputFormatter>()?.FirstOrDefault();

            if(newtonsoftJsonOutputFormatter != null)
            {
                newtonsoftJsonOutputFormatter.SupportedMediaTypes.Add("application/vnd.nilo.hateoas+json");
                newtonsoftJsonOutputFormatter.SupportedMediaTypes.Add("application/vnd.marvin.hateoas+json");
            }
        });

```
</details>

Clip 5 - Tightening the Contract Between Client and Server with Vendor Media Types
Combining Semantic Media Types with HATEOAS

**There should be only one suffix per media type, and only officially registered suffixes should be used. Creating different content negotion options requires a default `application/json` though.** 

`application/vnd.nilo.author.friendly+json`
- Friendly representation without links. It doesn't break the principle of one suffix per media type. It's right.

`application/vnd.nilo.author.friendly+hateoas+json`
-Friendly representation with links. This does break the principle of one suffix per media type.  
`application/vnd.nilo.author.friendly+hateoas+json`  
Instead using the prior use the later example above. It replaced the `+hateoas` to `.hateoas`  

`application/vnd.nilo.author.full+json`
- Full representation without links. It's right, because there is only one suffix.

`application/vnd.nilo.author.full+hateoas+json`
- Full representation with links. It's not right, because there are two suffixes. The hateoas and json. The correct way to use it should be as follow. Making `+hateoas+json` to `.hateoas+json`
`application/vnd.nilo.author.full.hateoas+json`

Example of different types of representation of data.
Friendly representation of data
```json
{
   "name": "Xablaw Palmeirense"
}
```

Full representation of data
```json
{
    "firstName": "Xablaw",
    "lastName": "Palmeirense"
}
```


 There is a way to couple media types to specific resources. By applying the `Producers` attribute, we can restrict the media types an action will produce.
<details><summary><b>Instruction to Handle Different Content Types for Resources Representations Responses</b></summary>

The `Produces` will filter the header with respectivelly values
- `Accept:application/vnd.marvin.hateoas+json`  
- `Accept:application/vnd.marvin.author.full+json`  
- `Accept:application/vnd.marvin.author.full.hateoas+json`  
- `Accept:application/vnd.marvin.author.friendly+json`  
- `Accept:application/vnd.marvin.author.friendly.hateoas+json`  
and if none is passed in the request header, it will match the filter `application/json`. 
Each of the accept above will produce different resource representation on the response. The code below is sample how to process it.  

```csharp
    [Produces("application/json",
        "application/vnd.marvin.hateoas+json",
        "application/vnd.marvin.author.full+json",
        "application/vnd.marvin.author.full.hateoas+json",
        "application/vnd.marvin.author.friendly+json",
        "application/vnd.marvin.author.friendly.hateoas+json")]
    [HttpGet("{authorId}", Name = "GetAuthor")]
    public async Task<IActionResult> GetAuthor(Guid authorId,
        string? fields,
        [FromHeader(Name = "Accept")] string? mediaType)
    {

        if (!MediaTypeHeaderValue.TryParse(mediaType, out var parsedMediaType))
        {
            return BadRequest(_problemDetailsFactory.CreateProblemDetails(HttpContext, statusCode: 400,
                detail: $"Accept header media type is not a valid media type."));
        }

        if (!_propertyCheckerService.TypeHasProperties<AuthorDto>
           (fields))
        {
            return BadRequest(
              _problemDetailsFactory.CreateProblemDetails(HttpContext,
                  statusCode: 400,
                  detail: $"Not all requested data shaping fields exist on " +
                  $"the resource: {fields}"));
        }

        // get author from repo
        var authorFromRepo = await _courseLibraryRepository
            .GetAuthorAsync(authorId);

        if (authorFromRepo == null)
        {
            return NotFound();
        }

        //var includeLinks = parsedMediaType.SubTypeWithoutSuffix
        var includeLinks = parsedMediaType.SubTypeWithoutSuffix
            .EndsWith("hateoas", StringComparison.InvariantCultureIgnoreCase);
        
        IEnumerable<LinkDto> links = new List<LinkDto>();
        
        if (includeLinks)
        {
            links = CreateLinksForAuthor(authorId, fields);
        }

        var primaryMediaType = includeLinks ?
            parsedMediaType.SubTypeWithoutSuffix
            .Substring(0, parsedMediaType.SubTypeWithoutSuffix.Length - 8) 
            : parsedMediaType.SubTypeWithoutSuffix;

        if(primaryMediaType == "vnd.marvin.hateoas+json")
        {
            var fullResourceToReturn = _mapper.Map<AuthorFullDto>(authorFromRepo)
                .ShapeData(fields) as IDictionary<string, object?>;

            if (includeLinks)
            {
                fullResourceToReturn.Add("links", links);
            }

            return Ok(fullResourceToReturn);            
        }

        // friendly author
        var friendlyResourceToReturn = _mapper.Map<AuthorDto>(authorFromRepo)
            .ShapeData(fields) as IDictionary<string, object?>;
        if (includeLinks)
        {
            friendlyResourceToReturn.Add("links", links);
        }
        
        return Ok(friendlyResourceToReturn);
        
    }
```
</details>

<details><summary><b>Handling different types of `Contenty-Type` for input requests</b></summary>

1. Creates a class that inherits from `Attribute` and implements `IActionConstraint`. This class will be used on the actions as attribute to filter different type of content-type for different requests.
```csharp
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;

namespace CourseLibrary.API.ActionConstraints;

[AttributeUsage(AttributeTargets.All, Inherited = true, AllowMultiple = true)]
public class RequestHeaderMatchesMediaTypeAttribute : Attribute, IActionConstraint
{
    private readonly string _requestHeaderToMatch;
    private readonly string mediaType;
    private readonly string[] otherMediaTypes;
    private readonly MediaTypeCollection _mediaTypes = new();

    public RequestHeaderMatchesMediaTypeAttribute(string requestHeaderToMatch,
        string mediaType, params string[] otherMediaTypes)
    {
        this._requestHeaderToMatch = requestHeaderToMatch 
            ?? throw new ArgumentNullException(nameof(requestHeaderToMatch));
        this.mediaType = mediaType 
            ?? throw new ArgumentNullException(nameof(requestHeaderToMatch));
        this.otherMediaTypes = otherMediaTypes
            ?? throw new ArgumentNullException(nameof(requestHeaderToMatch));

        // check if the inputted media types are valid media types
        // and add them to the _mediaTypes collection
        if (MediaTypeHeaderValue.TryParse(mediaType, out var parsedMediaType))
        {
            _mediaTypes.Add(parsedMediaType);
        }
        else
        {
            throw new ArgumentException(nameof(mediaType));
        }


        foreach (var otherMediaType in otherMediaTypes)
        {
            if(MediaTypeHeaderValue.TryParse(otherMediaType, 
                out var parsedOtherMediaType))
            {
                _mediaTypes.Add(parsedOtherMediaType);
            }
            else
            {
                throw new ArgumentException(nameof(otherMediaType));
            }
        }
    }

    public int Order { get; }

    public bool Accept(ActionConstraintContext context)
    {
        var requestHeaders = context.RouteContext.HttpContext.Request.Headers;
        if (!requestHeaders.ContainsKey(_requestHeaderToMatch))
        {
            return false;
        }
        var parsedRequestMediaTypes = new MediaType(requestHeaders[_requestHeaderToMatch]);
        // if one of the media types matches, return true
        foreach (var mediaType in _mediaTypes)
        {
            var parsedMediaType = new MediaType(mediaType);
            if (parsedRequestMediaTypes.Equals(parsedMediaType))
            { 
                return true; 
            }
        }
        return false;
    }
}

```

2. Decorate each action with different parameters to filter its respectivelly `Content-type` 
```csharp

    [HttpPost(Name = "CreateAuthorWithDateOfDeath")]
    [RequestHeaderMatchesMediaType("Content-Type",          
        "application/vnd.marvin.authorforcreationwithdateofdeath+json")]
    [Consumes("application/vnd.marvin.authorforcreationwithdateofdeath+json")]
    public async Task<ActionResult<AuthorDto>> CreateAuthorWithDateOfDeath(
        AuthorForCreationDto author)
        {...}

    [HttpPost]
    [RequestHeaderMatchesMediaType("Content-Type",
        "application/json",
        "application/vnd.marvin.authorforcreation+json")]
    [Consumes("application/vnd.marvin.authorforcreation+json")]
    public async Task<ActionResult<AuthorDto>> CreateAuthor(
        AuthorForCreationDto author)
        {...}
```
To specify which media types or actions can consume. Just like there's a `Produces` attribute to restrict what an action can produce, there's a `Consumes` attribute to constrict what an action can consume. So this has to do with the mediaTypes for the input formatter. 
That is different from our `RequestHeaderMatchesMediaType` constraint because that ensures routing to an action is allowed or blocked. It doesn't have anything to do with an input or output formatter. So we do need both of these attributes. 

The first request we're going to send is one to create an author with application/json as Content‑Type header. So that is one without a DateOfDeath. Let's click Send, and we indeed end up in the CreateAuthor action. So far, so good. Then let's try creating an author with Content‑Type vnd.marvin.authorforcreation+json. This again is an author without a DateOfDeath, and that means we should again end up in the CreateAuthor action. And that is indeed the case. Lastly, let's try creating an author with a DateOfDeath. So the request body contains a DateOfDeath, and as Content‑Type header value, we have our custom vendor‑specific AuthorForCreationWithDateOfDeath mediaType. Let's send this, and there we go, this time, we end up in our CreateAuthorWithDateOfDeath action. The author has been created, and have a look at the age of the author. Apparently it's 60. Let's have a look at what we sent. If we take the DateOfBirth and DateOfDeath into account, this is indeed an author who reached the age of 60. So here we go. This works as expected. And now we've got an ActionConstraint, we can also improve our GetAuthor method. Let's do that next.

3. Different types of request to call the actions above based on the request header
```json
// Example of the request to call the action for creation of author with date of death
Content-Type:application/vnd.marvin.authorforcreationwithdateofdeath+json
Accept:application/json

// Example of the request to call the action for creation of author
Content-Type:application/vnd.marvin.authorforcreation+json
Accept:application/json
```

</details>




### Versioning
**Through the URI**  
 - `api/v1/authors`  

**Through query string parameters**  
 - `api/authors?api-version=v1`  

**Through a custom header**  
 - `api-version=v1`  

**Version Media types to handle change in representations**
 - `application/vnd.nilo.author.friendly.v1+json` or use friendly names
---

## Chapter 12 - Caching
Each response should define itself as cacheable or not.  
Caching would be useless if it did not significantly improve performance. The goal of caching is to eliminate the need to send requests in many cases, and to eliminate the need to send full responses in many other cases.

**Caching Specifications**
[W3 - Obsolete](https://www.w3.org/Protocols/rfc2616/rfc2616-sec13.html)
[Data Tracker RFC7234 - Obsolete](https://datatracker.ietf.org/doc/html/rfc7234)
[Data Tracker RFC9111 - Actual](https://datatracker.ietf.org/doc/html/rfc9111)


<details><summary><b></b></summary>

```csharp
```

```csharp
```
</details>

### The Purpose of Caching

**Eliminate the number of requests**  
Reduces network-roundtrips  
*Expiration* mechanism  

**Eliminate the need to send full responses**  
Reduces network bandwidth  
*Validation* mechanism  

**The cache is a separate component**
- Accepcts requests from consumer to the API
- Receives responses from the API and stores them if they are deemed cacheable
It's the middle-man of request-response communication


**Cache Types**
1. Client Cache or Browser Cache - Private cache, only the client has access to it. Lives on the client.
2. Gateway Cache - Shared across different applications. Lives on server side. Reverse proxy caches or HTTP accelerators.
3. Proxy Cache - Also shared cache, but does not live at the consuming-side nor at the side of the API. It lives on the network.

Response Cache Attribute and Middleware

To support caching, we essentially need two things.
1. The first thing we need is a way to state for each resource whether or not it's cacheable.
That is done via a response header. There are various headers to consider, but the one most often used is the `Cache‑Control` header. A `Cache‑Control` header with maximum age set to 120. This states that a response must only be cached for 120 seconds.
To achieve that, the `ResponseCache` attribute is used.

2. A cache store. either at client level, server level or proxy level. The middleware is responsible for storing cacheable responses and serving them up from its store.


State for each resource whether or not it's cacheable
- Cache-Control:max-age=120
- [ResponseCache] attribute
- This does not actually cache anything

Cache store
- Response caching middleware


The response header will have a `Cache-Control` header with a `max-age` directive set to 120 seconds `public,max-age=120`. This indicates that the response can be cached for up to 120 seconds, and it could be stored publicly and privatelly

**Adding a cache store with the `ResponseCaching` middleware**
Just adding the max-age in the response header wouldn't be enough to cache the response. It's also required to provide a store mechanism though
```csharp
builder.Services.AddResponseCaching();
```
and adds the middleware to the HTTP request pipeline. Make sure to add it before the `app.MapControllers()`
```csharp
app.UseResponseCaching();
```
An indicative that the response was served from the cache is the presence of the `Age` header with the integer value representing the seconds the resource was added in the cache.

**Using cache  profiles to apply the same rules to different resources**

```csharp
builder.Services.AddControllers(options =>
{
    options.CacheProfiles.Add("240SecondsCacheProfile",
        new CacheProfile()
        {
            Duration = 240,
            Location = ResponseCacheLocation.Any
        });
});
```
It's possible to apply the cache profile to an action or to a controller.
```csharp
    [ResponseCache(CacheProfileName = "240SecondsCacheProfile")]
    public async Task<ActionResult<IEnumerable<AuthorDto>>> GetAuthors(
        [FromQuery] AuthorResourceParameters authorResourceParameters)
    {
        // get authors from repo
        var authorsFromRepo = await _courseLibraryRepository
            .GetAuthorsAsync(authorResourceParameters);
        // return them
        return Ok(_mapper.Map<IEnumerable<AuthorDto>>(authorsFromRepo));
    }
```



### Expiration Model
Allows the server to state how long a response is considered fresh.

|Expires header|Cache-Control header|
|-|-|
|Expires: Wed, 21 Oct 2015 07:28:00 GMT|Cache-Control: public,max-age=3600|
|Clocks must be synchronized|**Preferred header for expiration**|
|Offers little control|[Directives](https://datatracker.ietf.org/doc/html/rfc9111)|

<details><summary><b>How the Expiration Model Works</b></summary>
If the private cache hasn't expired, it will be used by the Angular Application or Mobile Application to use the resource. If has been expired it will access the Web Api. But if another application request the data from the Api and the its private cache doesn't have the resouce, it will access the Api  
With a public cache happens the same thing but with the difference that it drastically decrease the access to the Api but not to the public shared cache. Because every application will access the public shared cache.  

![](https://github.com/Ngofilho/Architecture/blob/images/images/20251002WebApiDeepDive/ExpirationModel.png)
</details>

### Validation Model
Used to validate the freshness of a cached response that's been cached.
Validation is used to validate the freshness of a response that has been cached. When a cache has a stale entry that it would like to use as a response to a client's request, it first has to check with the origin server, or possibly an intermediate cache with a fresh response to see if its cached entry is still usable. But to be able to validate, we need something to, well, validate against, and that's a validator.

|Strong Validators|Weak Validators|
|-|-|
|**Change if the body or headers of a response change**|**Don't always change when the response changes (eg: only on significante changes)**|
|**ETag (Entity Tag) response header**|Last-Modified: Wed, 27 Nov 2021 18:00:00 GMT|
|ETag:"123456789"|ETag: "w/123456789"|
|**Can be used in any context (equality is guaranteed)**|**Equivalence, but not equality**|

<details><summary><b>Validation Model</b></summary>

![](https://github.com/Ngofilho/Architecture/blob/images/images/20251002WebApiDeepDive/ValidationModel.png)</details>

**Expiration and Validation Combined**

|Private Cache|Shared (public) cache|
|-|-|
|**As long as the response hasn’t expired (isn’t stale), that response can be returned from the cache**|**As long as the response hasn’t expired (isn’t stale), that response can be returned from the cache**|
|Reduces communication with the API (including response generation), reduces bandwidth requirements|Reduces bandwidth requirements between cache and API, dramatically reduces request to the API|
|**If it has expired, the API is hit**|**If it has expired, the API is hit**|
|Bandwidth usage and response generation is potentially reduced even more|Bandwidth usage between cache and API and response generation is potentially reduced|

### Exploring the Cache-control Directives

**Response Directives**

|Category/Header|Purpose|
|-|-|
|**Freshness**|Have to do with how long a response can be considered fresh. So, a response can expire differently in a private cache versus in a shared cash.|   
|max‑age| Defines the maximum age after which a response expires in seconds.|  
|s‑maxage| Overrides the max value for shared caches.| 
|||
|**Cache type**| Related to the cache type or cache location.| 
|public |Indicates that a response may be cached by any cache|
|private or Shared. |Private indicates that all or parts of the response message are intended for a single user and thus must not be cached by a shared cache.|
|||
|**Validation**| Related to validation.| 
|no‑cache |Indicates that a response should not be used for subsequent requests without successful revalidation with the origin server.| 
|must‑revalidate |The server can state that if a response becomes stale, then revalidation has to happen. This is to allow the server to force revalidation by the cache, even if a client has decided that stale responses are okay.|
|proxy‑revalidate |Is exactly the same as must‑revalidate, but it doesn't apply to private user agent caches, like a browser cache.| 
|||
|**Other**|| 
|no‑store| Is available at client level as well, and it states that the cache must not store any part of the message. It's mostly used for confidentiality reasons.| 
|no‑transform| States that the cache shouldn't convert the media type of the response body.| 

**Request Directives**

|Category/Header|Purpose|
|-|-|
|**Freshness**||
|max‑age| Indicates that the client is willing to accept a response whose age is no greater than the specified time in seconds.| 
|min‑fresh| Indicates that the client is willing to accept a response whose freshness lifetime is no less than its current age plus the specified time in seconds. That is, the client wants a response that will still be fresh for at least the specified number of seconds.| 
|max‑stale| Indicates that the client is willing to accept a response that has exceeded its expiration time. This is the one must‑revalidate in the response reacts against. By the way, when I say client here, I am always talking about the component that sends the request. 
|||
|**Validation**||
|no‑cache| Stating that the response to this request should not be used for subsequent requests without successful revalidation with the origin server. In other words, with the API. 
|||
|**Other**||
|no‑store| The same as for response headers.|
|no‑transform|The same as for response headers.|
|only‑if‑cached| This states that a client wants the cache to return only those responses that it currently has stored and not reload or revalidate with the origin server. This one is typically used when there's a very poor network connection.|

These are already pretty advanced directives and options. In a lot of cases, we're quite okay with just using **max‑age** and **public** or **private**. 


---

## Chapter 13 - Supporting HTTP Cache for ASP.NET Core APIs

### Supporting ETags
ETags are preferred over dates as they are strong validators.  
This middleware allows many customizations like custom date parser, a custom validator value store, a custom ETag generator, if you want to provide a store to use with Redis it allows.  
In the example below, is set the cache control to expire in 60 seconds and the cache location to be private.  
```csharp
    builder.Services.AddHttpCacheHeaders((expirationModelOptions) =>
        {
            expirationModelOptions.MaxAge = 60;
            expirationModelOptions.CacheLocation =
            Marvin.Cache.Headers.CacheLocation.Private;
        },
        (validationModelOptions) =>
        {
            validationModelOptions.MustRevalidate = true;
        });
```
and then. Mind the order the `UseHttpCacheHeaders()` must be before `app.MapControllers()`.
```csharp
    app.UseHttpCacheHeaders();
```

If is set in controller the attributes of the Cache, it overrides the middleware configuration. for example, the code below states that the maxage is 1000 seconds, which in fact is greater than the 60 seconds set in the middleware customization. It also sets the mustrevalidate to true.  
 Setting MustRevalidate to true, it will tells a cache that if a response becomes stale, revalidation has to happen. By adding this directive, we can force revalidation by the cache, even if the client has decided that stale responses are for a specified amount of time.
```csharp
[HttpCacheExpiration(CacheLocation = CacheLocation.Public, MaxAge = 1000)]
[HttpCacheValidation(MustRevalidate = true)]
public class CoursesController : ControllerBase
```

The response header with the `age` in seconds indicating the amount of time had expent.
```json
Content-Length: 301
Content-Type: application/json; charset=utf-8
Date: Thu, 23 Oct 2025 19:01:16 GMT
Server: Kestrel
Age: 819
Cache-Control: public,max-age=1000
ETag: "5B8E42EADFA171BE5D11050461533458"
Expires: Thu, 23 Oct 2025 19:17:56 GMT
Last-Modified: Thu, 23 Oct 2025 19:01:16 GMT
Vary: Accept, Accept-Language, Accept-Encoding
```

### Demo: Dealing with Varying Response Representations
It's also possible to cache variation of the resource representation. For example, cache the json representation and if the client request a xml representation, the later one will hit the api while the json is kept in the cache.  

### ETags and the Validation Model
Sending a request with the `If-None-Match` with the previous ETag value from a response, we will get a `304 - Not Modified` response. It's sufficient with expiration model, but must be checked and validated with the validation in the cache scenario.  
The ETag value will be generated by the checking the response generated, if the response is the same as the first one that the generated the ETag, the ETag value will remains the same.  

### Cache Stores and Content Delivery Networks  
Examples of private caches   
For UWP or WPF applications is [CacheCow.Client](https://github.com/aliostad/CacheCow)   

Shared Caches are gateway or proxies:  
- [Varnish](https://varnish-cache.org)  
- [Apache Traffic Server](http://trafficserver.apache.org)  
- [Squi](http://www.squid-cache.org/)  


CDN   
- [Azure CDN](https://azure.microsoft.com/services/cdn)  
- [Cloudflare](https://www.cloudflare.com)  
- [Akamai](https://www.akamai.com)

### Cache Invalidation  
Wiping a response from the cache because you know it isn't the correct version anymore.  
A lot of this process happens automatically. A response is invalidated when it becomes stale, for example. 
When a resource is updated, the ETag changes, which means the validation model ensures that the cache will return the correct new version.  
Take the courses resource, for example, a list of courses. 
If a PUT statement is sent to one course resource, that one course will get a new ETag, but the courses resource doesn't automatically change.   
If the course you just updated is one of the courses in the returned courses when fetching the courses resource, then the courses resource is out of date.   
Same goes for deleting a course. That, too, might have an effect on the courses resource. 
Automating something like that is not trivial.   It is often a manual process, or a matter of implementing rules for related resources.   
This, too, is something CDNs can help with.  
In fact, if you read through their documentation, you'll notice they often pride themselves on allowing you to wipe something from their cache instantly, often via a web API call, sometimes even by providing an easy‑to‑use SDK.   
So, if you need functionality like that, CDNs are a good option. Most cache servers also offer this option.   
And if you ever have the need to do this with the Marvin.Cache.Headers middleware, you can do so by calling marking for invalidation on an IValidator value invalidator implementing instance.   
Simply injecting IValidator value invalidator via a constructor injection will get you the default implementation.  


---

## Chapter 14 - Supporting Concurrency

|Pessimistic concurrency|Optimistic Concurrency|
|-|-|
|Resource is locked|Token is returned together with the resource|
|While it’s locked, it cannot be modified by another client|The update can happen as long as the token is still valid|
|This is not possible in REST|ETags are used as validation tokens|

<details><summary><b>Example of Optimistic Concurrency</b></summary>

1. Kevin gets an author, and the author is returned with an ETag value.   
2. Sven gets that same author, and that's returned with the same ETag.   
3. Sven updates the author, passing in the ETag in the If‑Match header.  
4. The API checks this header and compares it with the ETag it saved for that response.   
5. If they match, which they currently do, the update can be applied.  
6. At this moment, a new ETag is generated for the response.  
7. The response then includes that new ETag as well.  
8. But then Kevin sends the update with an If‑Match header containing the ETag Kevin currently has.  
9. It reaches the API, and there the API sees that this does not match with the most current ETag for that resource.  
10. So the API returns a `412 Precondition Failed` status code.  

Kevin's update isn't applied because he was working on an older version of the author.  
The same applies for `PATCH`.  
This also drives the case for separating the cache store from the component that just generates the Cache‑Control and ETag headers.  
Those ETag headers serve different purposes. We don't need a cache to handle concurrent updates.  
So even without putting a cache server in front of the API or without using a Content Delivery Network, we can support concurrency simply by having a component that generates ETags for us by sending them from the client, as we just learned.  

![](https://github.com/Ngofilho/Architecture/blob/images/images/20251002WebApiDeepDive/SupportingConcurrency.png)


</details>

---

<details>
<summary>

## Other</summary>

<details><summary><b>

### How-To</b></summary>

Run the Visual Studio and use the Postman collection (in the folder src) in this repo. Check, if applicable, the header of the request is the same as expected in the middleware or any validation on the controllers/actions.  
Each chapter of the course has the its final implementation of the code separated in this folder (samples) by its respectivelly chapter number. From one chapter to another the code changes. The last chapter code (14) contains the final code implemented during the course by the instructor.

</details>

<details><summary>

### Version(s) </summary>

20251002 - 20251018 - 1st Version

</details>

<details><summary>

### Libraries</summary>

1. .Net Core 8    
2. AutoMapper.Extensions.Microsoft.DependencyInjection - v12.0.1  
3. Microsoft.AspNetCore.JsonPatch - v9.0.9  
4. Microsoft.AspNetCore.Mvc.NewtonsoftJson - v8.0.0  
5. System.Linq.Dynamic.Core - v1.3.7  
6. [Marvin.Cache.Headers - v7.0.0](https://github.com/KevinDockx/HttpCacheHeaders)  

</details>

</details>
