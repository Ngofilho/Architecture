
# [ASP.Net Core Web API Fundamentals](https://app.pluralsight.com/library/courses/asp-dot-net-core-6-web-api-fundamentals)

## Annotations

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

---

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

---

</details>

- **Middleware** are software components that are assembled into an application pipeline to handle requests and responses.


- AddControllersWithView internally calls into AddControllers and then register some additional services for view support, which means support for HTML Razor views

- `Controller` and `ControllerBase` could be used to create controllers. The difference is that `Controller` inherits from `ControllerBase` and adds support for views, which is not needed in web APIs.
- [`Routing`](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/routing?view=aspnetcore-8.0) matches a request URI to an action on a controller.  To set up endpoint routing, two pieces of middleware must be injected in the request pipeline. `UseRouting` and `UseEndpoints`
- UseRouting marks the position in the middlweare pipeline where the routing decision is made.
- UseEndpoints marks the position in the middleware pipeline where the selected endpoint is executed.

**Most Used Status Code on WebApis**

|Level 200|Level 400|Level 500|
|-|-|-|
|200 - Ok `Ok()`|400 - Bad Request | 500 - Internal Server Error `StatusCode(500,"A error message");` - ***Careful with the use***|
|201 - Created `Created(uri, object)`| 401 - Unauthorized||
|204 - No Content `NoContent()`| 403 - Forbidden||
||404 - Not Found||
||405 - Method not allowed||
||406 - Not acceptable||
||409 - Conflict `Conflict("...")` or `ConflictObjectResult()` ||
||415 - Unsupported media type||
||422 - Unprocessable entity||


[**The Problem Details for HTTP APIs RFC**](https://datatracker.ietf.org/doc/html/rfc7807)
```json
"type":"",
"title":"",
"status": 404,
"traceId": ""
```

<details><summary>

#### **Middleware Customization**

To manipulate the default ProblemDetails response, one way is passing an action to manipulate the ProblemDetails object using the `AddProblemDetails` extension method on the Service collection.
</summary>

```csharp
builder.Services.AddProblemDetails(options =>
{
	options.CustomizeProblemDetails = ctx =>
	{
		ctx.ProblemDetails.Extensions.Add("additionalInfo", "Additional info example");

        ctx.ProblemDetails.Extensions.Add("server",
            Environment.MachineName);
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

---

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

- **Middleware Customization**
In example bellow is a sample of code to handle the unacceptted format and the response provided by it

```csharp
builder.Services.AddController (option => {
	option.ReturnHttpNotAcceptable = true;
});

//Status code 406 - Not Acceptable
```

- **Middleware Customization**
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

---

</details>

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
The usually used are [FromBody], [FromHeader], [FromQuery] and [FromRoute]

Return of the type `CreatedAtRoute` will response with the route of the newly created item. Useful as alternative for HATEOAS

- **Model State (Validation Input)** It represents a collection of name‑value pairs that were submitted to our API, one for each property. It also contains a collection of error messages for each value submitted. Whenever a request comes in, the rules we just apply to our model are checked automatically. If one of them doesn't check out, the ModelStates.IsValid property will be false. This property will also be false if an invalid value for a property type is passed in. But this is not necessary. The API Controller

- **Patch - Partially Updating a Resource** [Json Patch (RFC6902)](https://tools.ietf.org/html/rfc6902)  is the standard of ***Patch Update***. The support from Microsoft comes from the library [Microsoft.AspNetCore.JsonPatch](https://www.nuget.org/packages/Microsoft.AspNetCore.JsonPatch/#readme-body-tab) - It requires the *NewtonSoft.Json* and the *Microsoft.AspNetCore.Mvc.NewtonsoftJson*

- **Middleware Customization**
`builder.Services.AddControllers().AddNewtonsoftJson();`

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

- **Middleware Customization**

---

### Chapter 5 : Working with Services and Dependency Injection

- **Middleware Customization**
`builder.Logging` allows customization of the out of the box logging. `builder.Logging.ClearProviders()` clear all the previously configured providers.
`builder.Logging.AddConsole()` will add the console for the output of the logs.

- DeveloperException: ASP.NET Core apps enable the `DeveloperException` page, by default, when two things are true, one, you must be running in the Development environment, and two, the app must have been created using WebApplication.CreateBuilder

- **Middleware Customization**
It's important to place the `ExceptionHandler` in the begining of the request pipeline code to globally catches all the exceptions.

```csharp
if(!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler();
    //app.UseDeveloperExceptionPage(); // This option allow to view a detailed error page. This is security treat option because it shows the stack trace.
}

builder.Services.AddProblemDetails();

```

There are lots of logging [providers](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/logging?view=aspnetcore-8.0#third-party-logging-providers) like:
elmah.io
Gelf
JSNLog
KissLog.net
Log4Net
NLog
PLogger
Sentry
Serilog
Stackdriver

A sink is a location to save the logs.

Configuring the Serilog

```csharp

//Serilog configuration in the Program.cs
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("logs/logfile.txt",rollingInterval: RollingInterval.Day)
    //.WriteTo.ApplicationInsights(new TelemetryConfiguration(){ InstrumentationKey = "Azure Application Insights Instrumentation Key"}, TelemetryConverter.Traces) // To use Azure Application Insights.
    .CreateLogger();

// This instruction tells AspNet to use the log configurated above
buider.Host.UseSerilog();
```

---

### Chapter 6 : Getting Acquainted with Entity Framework Core

|Safe approaches|Potentially unsafe approaches|
|-|-|
|Linq queries|`.FromSqlRaw()`|
|`.FromSql()` (when passing user input as parameter data)|Manually sanitizing the inputted values is required|
|`.FromSqlInterpolated()` (when passing user input as parameter data)||

---

### Chapter 09 : Securing Your API

<details><summary>
Sample Class Used Generate Authentication Tokens</summary>


The `SymmetricSecurityKey` requires the `System.IdentityModel.Tokens` library.
`Claim` class is defined in `System.Security.Claims`
`JwtSecurityToken` is defined in `System.IdentityModel.Tokens.Jwt`

```csharp

namespace CityInfo.API.Controllers
{
    [Route("api/authentication")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        // we won't use this outside of this class, so we can scope it to this namespace
        // This class has the only purpose to carry the basic user identification to validate he is whoever he says
        public class AuthenticationRequestBody
        {
            public string? UserName { get; set; }
            public string? Password { get; set; }
        }

        // This class could be classified as claim class, because it's been used by the validation to load the claims
        // This could be replaced by the "Identity Claims Class".
        private class CityInfoUser
        {
            public int UserId { get; set; }
            public string UserName { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string City { get; set; }

            public CityInfoUser(
                int userId,
                string userName,
                string firstName,
                string lastName,
                string city)
            {
                UserId = userId;
                UserName = userName;
                FirstName = firstName;
                LastName = lastName;
                City = city;
            }

        }

        //Constructor to require the IConfiguration to load informations from the appsettings.json
        public AuthenticationController(IConfiguration configuration)
        {
            _configuration = configuration ??
                throw new ArgumentNullException(nameof(configuration));
        }

        //The Action to be called passing the basic user's validation.
        // This Action returns the token if the user is valid.
        // The whole authentication process happens here.
        [HttpPost("authenticate")]
        public ActionResult<string> Authenticate(
            AuthenticationRequestBody authenticationRequestBody)
        {
            // Step 1: validate the username/password
            var user = ValidateUserCredentials(
                authenticationRequestBody.UserName,
                authenticationRequestBody.Password);

                //If the user and password is not valid, it returns an Unauthorized response.
            if (user == null)
            {
                return Unauthorized();
            }

            // Step 2: create a token
            // First retrieve a key from the appsettings then it decrypt the key to generate another SymmetricSecurityKey
            var securityKey = new SymmetricSecurityKey(
                Convert.FromBase64String(_configuration["Authentication:SecretForKey"]));

            // This signingCretentials will be used to sign the Jwt Token using the above security key and informing te Hash mechanism. In this case SHA256
            var signingCredentials = new SigningCredentials(
                securityKey, SecurityAlgorithms.HmacSha256);

            // Fill in the other claims
            var claimsForToken = new List<Claim>();
            claimsForToken.Add(new Claim("sub", user.UserId.ToString()));
            claimsForToken.Add(new Claim("given_name", user.FirstName));
            claimsForToken.Add(new Claim("family_name", user.LastName));
            claimsForToken.Add(new Claim("city", user.City));

             // Generate the Jwt Token
            var jwtSecurityToken = new JwtSecurityToken(
                _configuration["Authentication:Issuer"],
                _configuration["Authentication:Audience"],
                claimsForToken,
                DateTime.UtcNow,
                DateTime.UtcNow.AddHours(1),
                signingCredentials);

            var tokenToReturn = new JwtSecurityTokenHandler()
               .WriteToken(jwtSecurityToken);

               //Returns the newly Generated Token
            return Ok(tokenToReturn);
        }

        // This method could be substituted by any other validation of the ISP to retrieve the user's claim.
        private CityInfoUser ValidateUserCredentials(string? userName, string? password)
        {
            // we don't have a user DB or table.  If you have, check the passed-through
            // username/password against what's stored in the database.
            //
            // For demo purposes, we assume the credentials are valid

            // return a new CityInfoUser (values would normally come from your user DB/table)
            return new CityInfoUser(
                1,
                userName ?? "",
                "Kevin",
                "Dockx",
                "Antwerp");

        }
    }
}


```

</details>

`Microsoft.AspNetCore.Authentication.jwtbearer` contains the middleware to validate the token in the consumed API.

```csharp
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new()
        {
            ValidateIssuer = true, // validates the token issued
            ValidateAudience = true, // validates the token audience
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Authentication:Issuer"], // Fills the issuer to validate the token's issuer.
            ValidAudience = builder.Configuration["Authentication:Audience"], // Fills the audience to validate the token's audience.
            IssuerSigningKey = new SymmetricSecurityKey(
               Convert.FromBase64String(builder.Configuration["Authentication:SecretForKey"])) // To validate the token's signature
        };
    }
    );


// This instruction below adds the middleware to the request pipeline. Pay attention to the middleware order.
app.UseAuthentication();
```

And in every controller that requires authentication and authorization it must be write down in the class name the `[Authorize]`.


- **Authorization Policy** A policy is made up of a set of requirements. When all requirements evaluate to true, the policy is met.

```csharp

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("MustBeFromAntwerp", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("city", "Antwerp");
    }
});


// Place the instruction below in the controller to use the Authorization Policy
[Authorize(Policy = "MustBeFromAntwerp")]
```


- [**User-Jwt Tool**](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/jwt-authn?view=aspnetcore-8.0&tabs=windows) The dotnet `user-jwts` command line tool can create and manage app specific local JSON Web Tokens (JWTs).

---

### Chapter 10 : Versioning and Documenting Your API
- **Versioning**

Version via custom request header
- X-version: "v1"

Version via Accept header
- Accept:
   "application/json;version=v1"

Version the media types
- Accept:
   "application/vnd.marvin.book.v1+json"

`Asp.Versioning.Mvc` is a package part of the Asp.Net to version API's

To use and configure it, register it in the builder.

```csharp
builder.Services.AddApiVersioning(setupAction =>
{
    setupAction.ReportApiVersions = true;
    setupAction.AssumeDefaultVersionWhenUnspecified = true; //To use the default version when no version is specified.
    setupAction.DefaultApiVersion = new Asp.Versioning.ApiVersion(1, 0);
}).AddMvc();
//The AddMvc() method enable support for ASP.Net Core MVC APIs.
```

To use the specified version, pass the api version through the query string `https://.....?api-version=2`

- **Documentation**
[Swashbuckle.AspNetCore](https://github.com/domaindrivendev/Swashbuckle.AspNetCore) Generates an OpenAPI specification from API and Wraps swagger-ui and provides an embedded version of it.

```csharp
builder.Services.AddEndpointsApiExplorer(); // It's a built‑in ASP.NET Core service that exposes information on your API, like the available endpoints and how to interact with them. It's used internally by Swashbuckle to generate the OpenAPI specification.
builder.Services.AddSwaggerGen(); //It's executed. This registers services that are used for effectively generating the spec.

app.UseSwagger(); //Ensures that the middleware for generating the OpenAPI specification is added.
app.UseSwaggerUI(); //Ensures that the middleware that uses that specification to generate the default Swagger UI documentation URI gets added.
```

For the documentaiton using the `ActionResult` is better than using `IActionResult` because the first gives more resources for the documentation.
It wouldn't be suffice to place the document the Actions and models classes to reflect in the Swagger documentation. It also requires go to the project properties and under the `Builde>Output` check the option `Generate a file containing API documentation` and set the file name for the xml generated.

<details><summary>
This middleware code bellow informs the swagger about the xml generated with the documentation of the classes.</summary>

```csharp
builder.Services.AddSwaggerGen(setupAction =>
{
    var xmlCommentsFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlCommentsFullPath = Path.Combine(AppContext.BaseDirectory, xmlCommentsFile);

    setupAction.IncludeXmlComments(xmlCommentsFullPath);
});
```
</details>

<details><summary>

###### `Asp.Versioning.Mvc.ApiExplorer` allows automatically fills the version in the swagger documentation.  </summary>

```csharp

builder.Services.AddApiVersioning(setupAction =>
{
    setupAction.ReportApiVersions = true;
    setupAction.AssumeDefaultVersionWhenUnspecified = true; //To use the default version when no version is specified.
    setupAction.DefaultApiVersion = new Asp.Versioning.ApiVersion(1, 0);
}).AddMvc()
.AddApiExplorer(setupAction =>
{
    setupAction.SubstituteApiVersionInUrl = true;
});

// This code must be executed after the that service (the code above) has been registered on the container.
var apiVersionDescriptionProvider = builder.Services.BuildServiceProvider().GetRequiredService<IApiVersionDescriptionProvider>();
builder.Services.AddSwaggerGen(setupAction =>
{

    foreach(var description in
        apiVersionDescriptionProvider.ApiVersionDescriptions)
    {
        setupAction.SwaggerDoc
        (
            $"{description.GroupName}",
            new()
            {
                Title = "City Info API",
                Version = description.ApiVersion.ToString(),
                Description = "Through this API you can access cities and their points of interest"
            }
        );
    }
    var xmlCommentsFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlCommentsFullPath = Path.Combine(AppContext.BaseDirectory, xmlCommentsFile);

    setupAction.IncludeXmlComments(xmlCommentsFullPath);


    // The instruction bellow is intended to adds security to the requests to the actions through swagger.
    setupAction.AddSecurityDefinition("CityInfoApiBearerAuth",new(){
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        Description = "Input a valid token to access this API"
    });


    // The token isn't automatically sent as a Bearer Token in the authorization header in a request by the documenation.
    // The instruction bellow marks the operation in the OpenAPI spec as one that requires authentication. To that avail, we call in to AddSecurityRequirement on our setupAction. This expects an OpenApiSecurityRequirement object. That's, in fact, the dictionary with an OpenAPI security scheme as key
    setupAction.AddSecurityRequirement(new()
    {
        {
            new()
            {
                Reference = new OpenApiReference{
                    Type = ReferenceType.SecurityScheme,
                    Id = "CityInfoApiBearerAuth"}
            },
            new List<string>()
        }
    }
    });
});


// and finally pass through a setupAction to get those versions,
app.AddSwaggerUI(setupAction =>
{
    var descriptions = app.DescribeApiVersions(); //This is an extension method coming from that Asp.Versioning.Mvc.ApiExplorer package
    //then create endpoints for each of them, passing through the GroupName. This should result in version‑aware specifications and Swagger UI responding to it.
    foreach (var description in descriptions)
    {
        setupAction.SwaggerEndpoint
        (
            $"/swagger/{description.GroupName}/swagger.json",
            description.GroupName.ToUpperInvariante()
        );
    }
});
```
</details>

---

### Chapter 11 : Testing and Deploying Your API

- [Http REPL](https://learn.microsoft.com/en-us/aspnet/core/web-api/http-repl/?view=aspnetcore-8.0&tabs=windows) is tool used to enhace the http testing
`dotnet install -g --prerelease microsoft.dotnet-httprel` is the command to install it globally.
The Http REPL uses the OpenAPI description
`connect https://localhost:7169 --openapi https://localhost:7169/swagger/2.0/swagger.json` is the command to find the OpenAPI description
`pref set editor.command.default C:/Windows/system32/notepad.exe`
`set header Authorizations "Bearer .........` is the command to set a token in HttpREPL.

- [Endpoints Explorer](#) is a Visual Studio window that allows the creation of `.http` files.

- Dealing with Proxies and Load Balancers
[X-Forward Header](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/proxy-load-balancer?view=aspnetcore-8.0) used by middlewares to securely process the requests behind the proxies.

```csharp
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XFowardedFor
    | ForwardedHeaders.XForwardedProto;
});
```
The middleware should and could run after is the diagnostics and error handling
```csharp
app.UseForwardedHeaders();
```

- **Using Azure Key Vault**
It requires the Azure Entra package and Azure Key Vault Package. It's necessary to create a rule in the Azure Key Vault to allow the Azure Web Service to access it.
```csharp
var secretClient = new SecretClient(
 new Uri("Uri address of the Azure Key Vault"),
 new DefaultAzureCredential());
  builder.Configuration.AddAzureKeyVault(secretClient,
    new KeyVaultSecretManager());
);
```
---

<details>

<summary>

## Other(s)</summary>

<details><summary>

### Tool(s)

</summary>

- HttpREPL
- Postman
- .http files
- Swagger
- Entity Framework
- Azure Key Vault
- Azure WebServices
- API Testing Explorer
- Azure Application Insights
- Serilog
</details>

<details><summary>

### Version(s) </summary>

- 20250912 - 20250926 - First Time

</details>
</details>
