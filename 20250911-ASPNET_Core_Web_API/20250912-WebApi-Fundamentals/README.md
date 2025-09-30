
# [ASP.Net Core Web API Fundamentals](https://app.pluralsight.com/library/courses/asp-dot-net-core-6-web-api-fundamentals)

## Annotations

---
### Chapter 3 : Creating the API and Returning Resources

- Microsoft.NET.Sdk.Web implicitly includes the Microsoft.AspNetCore.App framework reference, which includes all supported packages by ASP.NET Core and Entity Framework Core.
- `dotnet run --launch-profile <name of the profile to run during the startup of the project>`


<details><summary>
Sample of basic web api project using the minimal hosting model</summary>

```csharp
var builder = WebApplication.CreateBuilder(args); // The webhost to build the web application

// The minimal hosting model for webapis in .NET 6+. Bellow are the set of services to be added to the DI container
builder.Services.AddControllers(); //  This method registers the necessary services for supporting controllers on our container
builder.Services.AddEnpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
if(app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
```
</details>

<details><summary>Example of terminal middleware that will short-circuit the request pipeline:

</summary>

```csharp
var builder = WebApplication.CreateBuilder(args); // The webhost to build the web application

// The minimal hosting model for webapis in .NET 6+. Bellow are the set of services to be added to the DI container
builder.Services.AddControllers();
builder.Services.AddEnpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.Run(async context => {
	await context.Response.WriteAsync("Palmeiras Campeão");// Will write, no matter what, Palmeiras Campeão and it will no process anything else.
	
});

app.Run();

```

</details>

- - **Middleware** are software components that are assembled into an application pipeline to handle requests and responses.


- AddControllersWithView internally calls into AddControllers and then register some additional services for view support, which means support for HTML Razor views

- `Controller` and `ControllerBase` could be used to create controllers. The difference is that `Controller` inherits from `ControllerBase` and adds support for views, which is not needed in web APIs.  
- [`Routing`](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/routing?view=aspnetcore-8.0) matches a request URI to an action on a controller.  To set up endpoint routing, two pieces of middleware must be injected in the request pipeline. `UseRouting` and `UseEndpoints`  
- UseRouting marks the position in the middlweare pipeline where the routing decision is made.  
- UseEndpoints marks the position in the middleware pipeline where the selected endpoint is executed.  

**Most Used Status Code on WebApis**

|Level 200|Level 400|Level 500|
|-|-|-|
|200 - Ok `Ok()`|400 - Bad Request | 500 - Internal Server Error|
|201 - Created `Created(uri, object)`| 401 - Unauthorized||
|204 - No Content `NoContent()`| 403 - Forbidden||
||404 - Not Found||
||409 - Conflict `Conflict("...")` or `ConflictObjectResult()` ||


[**The Problem Details for HTTP APIs RFC**](https://datatracker.ietf.org/doc/html/rfc7807)
```json
"type":"",
"title":"",
"status": 404,
"traceId": ""
```

<details><summary>
To manipulate the default ProblemDetails response, one way is passing an action to manipulate the ProblemDetails object using the `AddProblemDetails` extension method on the Service collection.
</summary>

```csharp
builder.Services.AddProblemDetails(options =>
{
	options.CustomizeProblemDetails = ctx =>
	{
		ctx.ProblemDetails.Extensions.Add("additionalInfo", "Additional info example");
	}
});

// will produce the response bellow.
{
"type":"",
"title":"",
"status": 404,
"traceId": "",
"additionlaInfo":"Additional info example"

}
```
</details>

- [**Content Negotiation**](https://learn.microsoft.com/en-us/aspnet/core/web-api/advanced/formatting?view=aspnetcore-8.0) The process of selecting the best representation for a given response when there are multiple representations available.   
Output formatter Deals with output. Media type: Accepct header   
Input formatter deals with input Media type: Content-type header   
Support is implemented by `ObjectResult`   
The rule is that the first Input formatter in the list inside the customization is the default.  

Http Header
```
Accept: application/xml
Accept: application/json
Accept: text/plain
```

In example bellow is a sample of code to handle the unacceptted format and the response provided by it
```csharp
builder.Services.AddController (option => {
	option.ReturnHttpNotAcceptable = true;
});

//Status code 406 - Not Acceptable
```


In the example bellow there is a sample of the customization of the service to response xml
```csharp
app.Services.AddControllers().AddXmlDataContractSerializerFormatters();

```

- **File Transfer** Example to implement the file transfer   
**FileContentResult**, which accepts the file bytes and a content type for the file.    
**FileStreamResult**. This accepts a stream to read from and the contentType.  
**PhysicalFileResult** and **VirtualFileResult**. These, too, allow you to pass through a file name and a content type.  
All of these also derive from the same **FileResult** class.   
It's more convenient to call into return **File**. This method is defined on the ControllerBase and it acts as a wrapper around the aforementioned **FileResult** subclasses  

<details><summary>
File Download Sample
</summary>

```csharp
// Adds the instruction bellow to accept the transfer the of any type of file based on the file's extension
builder.Services.AddSingleton<FileExtensionContentTypeProvider>();


using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace CityInfo.API.Controllers
{
    [Route("api/files")]
    [ApiController]
    public class FilesController : ControllerBase
    {

        private readonly FileExtensionContentTypeProvider _fileExtensionContentTypeProvider;

        public FilesController(
            FileExtensionContentTypeProvider fileExtensionContentTypeProvider)
        {
            _fileExtensionContentTypeProvider = fileExtensionContentTypeProvider
                ?? throw new System.ArgumentNullException(
                    nameof(fileExtensionContentTypeProvider));
        }

// Action to handle the file request
        [HttpGet("{fileId}")]
        public ActionResult GetFile(string fileId)
        {
            // look up the actual file, depending on the fileId...
            // demo code
            var pathToFile = "getting-started-with-rest-slides.pdf";

            // check whether the file exists
            if (!System.IO.File.Exists(pathToFile))
            {
                return NotFound();
            }

            if (!_fileExtensionContentTypeProvider.TryGetContentType(
                pathToFile, out var contentType))
            {
                contentType = "application/octet-stream";
            }

            var bytes = System.IO.File.ReadAllBytes(pathToFile);
            return File(bytes, contentType, Path.GetFileName(pathToFile));
        }
    }
}


```

</details>


-

---

### Chapter 4 : Manipulating Resources and Validating Input

**By default, ASP.NET Core attempts to use the complex object model binder**
-  [FromBody]: Request Body. Inferred for complex types
-  [FromForm]: From data in the request body. Inferred for action parameters of type `IFormFile` and `IFormFileCollection`
-  [FromHeader]: Request Header
-  [FromQuery]: Query string parameters. Inferred for any other action parameters.
-  [FromRoute]: Route data from the current request. Inferred for any action parameter name matching a parameter in the route template.
-  [FromServices]: The service(s) injected as action parameter
-  [AsParameters]: Method Parameters

Return of the type `CreatedAtRoute` will response with the route of the newly created item. Useful as alternative for HATEOAS

- **Model State (Validation Input)** It represents a collection of name‑value pairs that were submitted to our API, one for each property. It also contains a collection of error messages for each value submitted. Whenever a request comes in, the rules we just apply to our model are checked automatically. If one of them doesn't check out, the ModelStates.IsValid property will be false. This property will also be false if an invalid value for a property type is passed in. But this is not necessary. The API Controller   

- **Patch - Partially Updating a Resource** [Json Patch (RFC6902)](https://tools.ietf.org/html/rfc6902)  is the standard of ***Patch Update***. The support from Microsoft comes from the library [Microsoft.AspNetCore.JsonPatch](https://www.nuget.org/packages/Microsoft.AspNetCore.JsonPatch/#readme-body-tab) - It requires the *NewtonSoft.Json*  

Array of Operations with the set of instruction to patch the resource
```json
[
    {
        "op":"replace", // "replace" operation
        "path":"/name", // the path of the resource to be replaced
        "value":"new name" // the new value of the resrouce
    },
    {
        "op":"replace",             // "replace" operation
        "path":"/description",      // the path of the resource to be replaced
        "value":"new descritption"  // the new value of the resrouce
    }
]
```
Allowed operations:  
1. add  
2. remove  
3. replace  
4. move  
5. copy  
6. test  

---
<details>

<summary>  

## Other(s)</summary>

<details><summary>

### Version(s) </summary>

- 20250912 - 20250926 - First Time

</details>
</details>
