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
1- When we looked into routing, we learned that route templates should be applied with attributes when building APIs   
2- what is returned in case of an error follows a certain format, the ProblemDetails format.   
3- Bind Inferred Source   
	1- `FromBody` is inferred for complex type parameters thanks to the `[ApiController]`, ASP.Net by default try to bind the complex model to the body of the request. 
	2- `FromForm` is inferred for action parameters of type `IFormFile` and `IFormFileCollection`. 
	3- `FromRoute` is inferred for any action parameter name matching a parameter in the route template. When more than one route matches an action parameter, any route value is considered `FromRoute`. 
	4- `FromQuery` is inferred for any other action parameters.    

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

---

<details>
<summary>

## Other</summary>

<details><summary>

### Version(s) </summary>

20251002 - 1st Version

</details>

</details>
