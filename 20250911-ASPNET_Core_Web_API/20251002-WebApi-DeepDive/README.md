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

Method is considered safe when it does not change the resource representation. GETS and HEADS are safe methods.  

Method is considered idempotent when the same request can be made multiple times with the same effect as making it once. PUTS, DELETES and HEADS are idempotent methods.   

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
