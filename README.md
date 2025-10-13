# Annotations

<details><summary> 

## Styles</summary>

[**REST**](https://www.ics.uci.edu/~fielding/pubs/dissertation/top.htm), or Representational State Transfer, is intended to evoke an image of how a well‑designed web application behaves: a network of web pages (a virtual state-machine) where the user progresses through an application by selecting links (state transitions) resulting in the next page (representing the next state of the application) being transferred to the user and rendered for their use.

- REST is an architectural style  
- REST is not a standard in its own right  
- Standards are used to implement the REST architectural style  
- REST is, in principle, protocol agnostic  
- Rest is defined by 6 constraints (A design decision that can have positive and negative impacts)   
  1. ***Uniform Interface*** - API and consumers share one single technical interface: URI, Method, Media Type (payload)  (Obligatory)  
  2. ***Client-Server*** - client and server are separeted (client and server can evolve separately) (Obligatory)  
  3. ***Statelessness*** - state is contained within the request (Obligatory)  
  4. ***Layered System*** - client cannot tell what layer it's connected to (Obligatory)  
  5. ***Cacheable*** - each response message must explicitly state if it can be cached or not (Obligatory)  
  6. ***Code on Demand*** - server can extend client functionality (Optional)   
A system is only considered RESTful when it adheres to all the required constraints. Most "RESTful" APIs aren't really RESTful, but that doesn't make them bad APIs, as long as you understand the potential trade-offs   

The Ricardson Maturity Model
|Level|Topic|Description|Considerations|Example|
|-|-|-|-|-|
|0|The Swamp of POX - *P*lain *O*ld *X*ML|HTTP protocol is used for remote interaction, the rest of the protocol isn't used as it should be. |RPC-style implementations (SOAP, often seen when using WCF).|POST (info on data) `http://host/api`
POST (author to create) `http://host/api`|
|1|Resources|Each resource is mapped to a URI, HTTP methods aren't used as they should be.| Results in reduced complexity.|POST (info on data) `http://host/api/authors`
POST (author to create) `http://host/api/authors/{id}`|
|2|Verbs|Correct HTTP verbs are used, correct status codes are used|Removes unnecessary variation|GET `http://host/api/authors` 200 Ok (authors)
POST (author representation) `http://host/api/authors` 201 Created (author)|
|3|Hypermedia|The API supports Hypermedia as the Engine of Application State (HATEOAS). Introduces discoverability.|It's a precondition for a RESTful API|GET `http://host/api/authors` 200 Ok (authors + links that drive application state)|

- Level 0 (The Swamp of POX - *P*lain *O*ld *X*ML) - HTTP protocol is used for remote interaction, the rest of the protocol isn't used as it should be. RPC-style implementations (SOAP, often seen when using WCF).  
- Level 1 (Resources) - Each resource is mapped to a URI, HTTP methods aren't used as they should be. Results in reduced complexity.  
- Level 2 (Verbs) - Correct HTTP verbs are used, correct status codes are used. Removes unnecessary variation.  
- Level 3 (Hypermedia) - The API supports Hypermedia as the Engine of Application State (HATEOAS). Introduces discoverability. It's a precondition for a RESTful API.    

</details>


<details>
<summary>

## [Fundamentals of Distributed Systems](https://app.pluralsight.com/library/courses/distributed-systems-fundamentals)  </summary>


<details>
<summary>

#### Properties of a Reliable Application  </summary>

<details>
<summary>

##### 1. Idempotence </summary>
Help to tolerate unreliable networks. When the client tries to repeat some failed operation due the network flaw, the application won't duplicate the effect if it receives duplicate message.
Idempotence operation is one that has no effect when it's received twice or more. To implement idempotence, one approach could be using *Clinet-Side ID*. For example, if the user know the ID of the object before it's created, then the server can tell the difference between the creation of a new object and a duplicate message. Duplicate messages will carry the same IDs as the original.
Another approach can be implementing *Client-side ID in database*, either using a different ID than auto-increment key, *GUID* is a good choice as an alternate key, or before inserting the record, searching for it using the same parameters as being used on the insert, if it returns a record, then just returns it to the client otherwise, insert and then returns the outcome to the client.

</details>

<details>
<summary>

##### 2. Immutability</summary>
From [Pat Helland paper](http://highscalability.com/blog/2015/1/26/paper-immutability-changes-everything-by-pat-helland.html), keeps a record of everything that happens within the system, the data won't be overridden or deleted.
Gives reliable audit log, do not destroy data, preserves metadata.
Two well known design patterns for immutability are:
1. **Snapshots** - Usually used for updates
2. **Tombstones** - Used for delete records.
</details>

<details>
<summary>

##### 3. **Location Independence**</summary>
Deployed at different locations. Implies that an application's behavior does not depend upon its location. Each instance produces the same behavior.
Location Independent Identifiers:
1. Alternate Key
2. Natural Key
3. Public Key
4. Hash - Known patterns as Content-address Storage
Uses the content itself to help to identify itself
Implementations of ***Content-address Storage***
- docker image stack hash
- git - sha1 hash system
- IPFS(Inter planetary file system)

Advantages of Content-address Storage:
Naturally immutable
Naturally Verifiable
Naturally Idempotent
Solves Cache Invalidation
</details>

<details>
<summary>

##### 4. **Versioning**</summary>
 Tolerance to changes onto the system.
</details>

##### Fallacies of Distributed Computing
1. _The network is reliable_
2. Latency is zero
3. Bandwidth id infinite
4. The network is secure
5. _Topology doesn't change_
6. There is one administrator
7. Transport cost is zero
8. _The network is homogeneous_

</details>

<details><summary>

#### Connectiong Services</summary>
Eventual consistency. When we have data in different places, when we ask the same for both, we want the anwser be consistent with between them. The *Eventual* comes from the falacy of the latency is zero.
**Commands** are messages that have side effects and return no data. Usually named with imperative verb.
**Queries** are messages that have no side effects and return data. Usually have a name of specification.
Commands and queries are sent to the system of record.
**Events**, otherwise, are published from the system the record. They are published after a event had occurred and they expose data from the system of record. Tend to be named using past-tense verbs.

**Publish-subscribe**
Publish doesn't know subscribers hence who's interested in those events ahead of time.

<details>

<summary>

##### Competing Consumer

</summary>

  The way that MassTransit implements publish‑subscribe is to set up an exchange. When a consumer subscribes for events, MassTransit adds a queue to the exchange. Each different kind of subscriber gets a new queue. When subscribing, the consumer specifies a receive endpoint. Different receive endpoints imply different kinds of consumers. It therefore creates a new queue for each receive endpoint. When the system of record publishes a message to the exchange, MassTransit will add that message to each of those queues. In that way, the same message is handled by each different kind of subscriber. On the other hand, when two instances of the same kind of subscribers start up, they will both specify the same receive endpoint. MassTransit will therefore connect them both to the same queue. The consumers reading from the same queue will compete for those messages. Only one of them will receive it. This is a pattern within publish‑subscribe known as the competing consumer pattern. And because MassTransit implements competing consumers, it's able to ensure that only one instance will receive the message.
</details>

**Commutativity** refers to when the system process different messages, and the order of processing doesn't matter for the system.


</details>

<details><summary>

#### Identifying Service Boundaries</summary>
*Any organization that designs a system will produce a design whose structure is a copy of the organization's communication structure*
Melvin E. Conway

**Distinghishing Aggregates**:
- Definition: What's the definition of an aggregate (which part/class can be an aggregate).
- Differentiation: What distinguishes one aggregate from another.
- Identity: What's is it identity.
- Events: What events leds to its existence.

<details><summary>

##### Historical Model</summary>
![](https://github.com/Ngofilho/Architecture/blob/images/images/HistoricalModel.jpg)

</details>

</details>

<details><summary>

#### Invoking Business Processes</summary>
**Temporal Coupling** The client timeout must encompass not only the Delivery Time but also the Processing Time.

|Protocol|Caracteristics|Examples|
|-|-|-|
|Synchronous|Response occurs after request is processed<br>Response conveys information about processing<br>Report on delivery and processing failures|HTTP - Standard Based<br>SOAP - Standard Based<br>gRPC - Standard Based<br>REST - Standard Based|
|Asynchronous|Reponse occurs after delivery<br>Reponse cannot convey information about processing<br>Response cannot report sucess<br>Report only on delivery failures|AMQP<br>Apache Kafka - Implementation Based<br>IBM MQ - Implementation Based<br>Amazon SQS - Implementation Based<br>HTTP (used judiciously e.g. 202 Accepted)|

The impact of protocols on types of messages
|Message Type|Asynchronous Protocol|Synchronous Protocol|
|-|-|-|
|Events|Tipically used as asynchronous, because there is no need to couple the emitter with the receiver, even for temporal coupling|-|
|Commands|If the client doesn't need to know any information about how the command was processed|If the client needs to know any information, then use synchronous protocol message|
|Queries|Usually synchronous processing|Use asynchronous when the query needs time to be processed, but you must inform the query's result drop off location|


</details>

<details><summary>

#### Keeping Things Running</summary>
**Runbook** consists of documentation about the components, dependencies, communication channels, behaviors, mitigations plans. It's a living document. e.g. (Wikipedia about the project).

<details><summary>

##### Database Runbook Diagrams</summary>
It's recommended keep the diagrams small.
![](https://github.com/Ngofilho/Architecture/blob/images/images/ERD.jpg)

***

![](https://github.com/Ngofilho/Architecture/blob/images/images/DbRelationDiagram.jpg)

***

![](https://github.com/Ngofilho/Architecture/blob/images/images/ERDHierarchy.jpg)
The ERD should contain the keys only, not the columns.
It's recomended that the father table be on top and the children tables beneath it, with the arrows pointing up. This makes the hierarchy clear.
</details>

**Application Dependency Map** responsible to describe every dependency the application has, like:
- API's
- Messages
- Machine Names
- Configurations Settings

**Problems and Mitigations**
|Problem|Mitigation|
|-|-|
|Endpoint /*acts* response time exceeds 250ms|In dtabase *promotion.sql*, exec sp_updates|
|Queue show.rabbotmq Indexer exceeds 20 messages|Ensure that elasticsearch.shows is responding|
|Failed messages in show.rabbitmq Emailer_Error|Ensure that smtp.customer_service is responding|
|...|...|

Log technical debt.
Runbook should contain the log queries examples, so do the sql queries too.
***No plan survices first contact with the enemy, and no runbook survives the first deployment to production.***

<details><summary>

##### Logging Standards</summary>
![](https://github.com/Ngofilho/Architecture/blob/images/images/LogsStandards.jpg)
First, log every entry point into the system. These are the API calls, controller actions, and incoming messages. Include all of the inputs before they are parsed. Log these as informational messages. If the input data contains a lot of unnecessary detail, then log this as a separate debug message at the same time. Log a debug message upon return so that you can capture rudimentary timing measurements. Second, log every point that leaves the system. This includes every outgoing API call or message sent. And here the same rules apply. These are informational messages with optional debug details. And log a debug message when the dependency returns. This allows the ops team to watch the system under normal operation and then crank it up to debug in order to see those extra details. Speaking of those extra details, that brings us to number three, log every database or file system access. If possible, include the SQL and the parameters in the log message. These should be debug messages because they're very detailed. Most ORMs include interceptors that will do this for you. Just make sure that you enable them. And now the fourth standard logging practice is to log any important decisions within any sufficiently complicated business logic. Be sure to include the parameters that went into making that decision. You won't have any of these with your standard CRUD operations, but when you have more complex business logic you'll definitely want to capture this information. And they should be captured at the information level. And then, finally, of course, log exceptions as errors. Make sure to catch and wrap those exceptions in order to add context. Your development organization may want to put more practices in place, but this is a great starting point that will give your operations team enough information to figure out what's going on.

</details>

</details>

<details><summary>

#### Managing Complex Scenarios</summary>
**Invariants**  are statements about application state that most of the time are true, like money is never created nor destroyed, it only moves from account to another. The inventory of a product can never be negative. Specific seats at a concert event, cannot be sold twice. There are invariantes that link 2 or more properties together, like if a customer is paid for a product, then they own that product, if they no, then they don't.
When an invariant is broken, it's said that the invariant lacks consistency. For example, when we transfer money from account to another, until the end of the transaction of the transfer to be considered as completed, the initial state of the invariant has broke the invariant that states that money is never created nor destroyed. When the later step of the process of transfer money is completed, it'll restore the earlier step that has broke the invariant.

Database Transactions breaks invariants on its start and restore it on its end, these steps, break and restore, are guaranted by **ACID**
- A: Atomicity- Guaranteed that no one outside the transaction will see the invariant broken after the transaction is completed.
- C: Consistency - Guaranteed that the invariantes has been restored after the transaction has been executed.
- I: Isolation - Guarantees that others outside of the transaction can't see the invariants are broken in real time.
- D: Durability - Guaranteed that changes persist after the transaction has been executed.

- **Tip(s)**
- The first step to understand the complexity of the system is to understand the system's invariants.

**Saga** Proposed by Hector Garcia-Mollina and Kenneth Salem published the pattern in a paper at Princeton University Department of Computer Science on 1987. It breaks down a long live transaction into several small transactions. Each transaction is limited in time and scope and takes advantage of the ACID guaranteed that DB offers. It's not guaranteed that external observers to see that the invariants have been violted and state of the system is not consistent. Instead, it guaranteed that the state will become consistent once the saga completes. The paper propose that DBMS manages the sagas, but this rarelly occurs, because generally modern distributed systems implement sagas as states machines.
Each message moves the machine from one state to the next, messages related to the same business process all share a common correlation ID.

E.g. Saga.
**Sales Service**
The paying for a ticket transaction could be broken into different steps bellow.
- Reserve funds - Easily reversible
- Lock inventory - Easily reversible
- Capture funds - Means transfer the value in to a merchant account.
- Allocate inventory - This step and the capture funds terminates the saga.
The whole process starts with the *PurchaseTicket* command.
<details><summary>

##### Saga State Machine and Compesating Transactions </summary>
**Saga State Machine**
![](https://github.com/Ngofilho/Architecture/blob/images/images/SagaStateMachine.jpg)

**Compesating Transactions**
![](https://github.com/Ngofilho/Architecture/blob/images/images/CompensatingTransactionsSagas.jpg)
</details>


<details><summary>

##### Tips for Building Value</summary>
Distributed systems are hard. They're hard to build, they're hard to run, and they don't always behave in nice, predictable ways.
If you don't need to build a distributed system, then don't. Focus on good, reliable architecture to make the best possible application for your customers.
Design an immutable database so that you capture all of the data and metadata required to understand what has happened.
Don't expose internal database IDs so that the application can be location independent and operations can be idempotent.
Use additive structure in order to version the application without breaking API consumers or invalidating existing data.
But if you really need to build a distributed system, then build on that reliable architecture to make the pieces work well together.
Use the metadata that you captured in the immutable database in order to make your operations communicative.
Rely on that location independent identity and idempotence and commutativity in order to make your systems reach eventual consistency. Lean on those additive structures in order to account for the fact that the network is not homogeneous and that applications will be upgraded at different times. If you find yourself building and managing a distributed system, it'll probably be because there's a business benefit to doing so. Tap into those business units.
Align your IT strategy with your business strategy, bringing people together across disciplines at workshops, over code reviews, and in war rooms.
Encourage a culture and a set of deliberate practices that provide each specialist with the information that they need to support the whole platform. I think that you'll find that the rewards of running a distributed system extend far beyond good architecture. You will build measurable value for your organization and have a seat at the table for innovative strategic conversations.
</details>
</details>

</details>


------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

<details>
<summary>

## [Designing RESTful Web APIs](https://app.pluralsight.com/courses/64ee3a82-fc9c-4bcf-b94b-5f01b6e64536/table-of-contents)</summary>

<details>
<summary>

#### What is REST?</summary>

<details>

<summary>

##### History of Distributed System</summary>

![](https://github.com/Ngofilho/Architecture/blob/images/images/DC_History.png)

</details>

**Useful commands**
```cmd
curl arest.me -I //show all of the headers in the response except for the body.

curl arest.me -i //show all of the headers and the body in the response.
```
</details>

<details>
<summary>

#### Designing a RESTful API</summary>
Use **query strings** for optional parameters or innofensive data. Use for non-resource properties.
**Nouns are good, verbs are bad**

|Avoid|Prefer|
|-|-|
|/getCustomers|/Customers with verb get|
|/getCustomersByName|/Customers with verb get|
|/getCustomersByPhone|/Customers with verb get|
|/verifyCredit|/Credit with verb get|
|/saveCustomer|/Customer with verb post|
|/updateCustomer|/Customer with verb patch or put|
|/deleteCustmer|/Customer with verb delete|


**Tip(s)**
`X-Total-Count` shows the total result
Another useful trick is use as **query parameter** the page size and send back in header content the previous and next page. ie. `X-NextPage:/api/Sites?page=5` and `X-NextPage:/api/Sites?page=5`

- **Associations** - Can have multiple associations
      ie. `/api/customers/123/invoices`  Retrieve all `invoices` (resource) from the `customer` (resource) `123`
      `/api/customers/123/payments` Retrieve all `payments` (resource) from `customer` (resource) `123`
  Search should use queries
ie.
```
/api/Customers?st=GA
/api/Customers?st=GA&salesid=144
/api/Customers?hasOpenOrders=true
```

- **Paging** -
  Lists should support paging. Commonly used with query strings
ie.  `/api/sites?page=1&page_size=25`
Use wrappers to imply paging:
```json
{
  "totalResults": 255,
  "nextPage": "/api/sites?page=5",
  "prevPage": "/api/sites?page=3",
  "results":[...]
}
```
- **Error Handling** -
  Not just status code, it could be also used to communicate errors, and to help the user to recover.
ie.
```json
{"error":"Failed to supply id"}
```
other example
```json
{
  "errors" : {
              "Name" : [
                          "The name field is required."
                        ]
            },
          "title": "One or more validation errors occurred",
          "status":400,
          "traceId":"2434hkjkjh1234"
}
```

- **Caching** -
Basic tenet of REST APIs. Server-side chaching is good. But isn't what they mean. Use HTTP for caching mechanism.

Request
```http
GET / HTTP/1.1
Version:last_xyz
...Content...
```
Response
```http
304 Not modified
```

Another way of technique is using the header option `If-Match=last_xyz` with an arbitrary identification to request to server to put these fields in my request in the right place if the identifications is the same of the server. If there is no match, if it has been updted on the server since I retrieved it, it will send back the http status 412 (Precondition failed).
Request
```http
GET / HTTP/1.1
If-Match:last_xyz
...Content...
```
A great way to handle this caching is something called entity tags (ETag). It support weak and strong caching support.

```http
HTTP/1.1 200 OK
Content-Type: text/xml;
Date: Thu, 23 May 2013 21:52:14 GMT
ETag W/"4893023942098"
Content-Length: 639


HTTP/1.1 304 Not Modified

HTTP/1.1 412 Precondition Failed
```
- **Functional** -
Generally used for some operation that might be used to do certain kinds of routines that might cause side-efects, like reset a database, calculate values, etc.

- **Async API Solutions to Consider**
  1. Comet
  2. gRPC
  3. SignalR
  4. Firebase
  5. Socket.IO
  6. etc.
</details>

<details>
<summary>

#### Versioning Your API</summary>
Strategies to version APIs
- Uri Path
    Pros: Very clear to clients where the version is handled
    Cons: Every version needs to change URIs, can be brittle
  ie
  `https://foo.org/api/v2/Customers`

- Query String
    Pros: Versioning is optionally included (can use default version)
    Cons: Too easy for clients to miss needing the version
  ie
  `https://foo.org/api/Customers?v=2.0`

-  Versioning with Headers
    Pros: Separates versioning from the rest of the API
    Cons: Requires more sophisticated developer to manipulate headers
   ie
  ```http
GET /api/camps HTTP/1.1
Host: localhost:44388
Content-Type: application/json
X-Version:2.0
  ```

- Versioning with Accept Header
    Pros: No need to create your own custom header
    Cons: Even less discoverable than query strings
ie
```http
GET /api/camps HTTP/1.1
Host: localhost:44388
Content-Type: application/json
Accept: application/json;version=2.0
```

- Versioning with Content Type
    Pros: Can version the payload as well as the API call itself
    Cons: Requires a lot more development maturity to create and maintain
  ie
```http
GET /api/camps HTTP/1.1
Host: localhost:44388
Content-Type: application/vnd.yourapp.camp.v1+json
Accept: application/vnd.yourapp.camp.v1+json
```
</details>

<details>
<summary>

#### Locking Downs Your API</summary>

Do You really need to secure your API ?
|Are you... | Secure?|
|-|-|
|...using private or personalized data?|Yes.|
|...sending sensitive data across the 'wire'|Yes.|
|...using credentials of any kind?|Yes.|
|...trying to protect against overuse of your servers|Yes.|

- Server Infrastructure Security
    Outside scope of API security
- Secure In-Transit
    SSL is almost always appropriate
    Cost of SSL is worth the expense
-  Secure the API itself
    Cross Origin Security
    Authorization/Authentication

**Cross Domain Security**
- By default not allowed
- Only applies for browsers
- Public API should allow cross domain
- Private API Consider for Partners

<details><summary>

##### How Does CORS Work?</summary>
![](https://github.com/Ngofilho/Architecture/blob/images/images/cors.jpg)

</details>

**Authentication Types for APIs**
- Cookies -Easiest and common. Subject to request forgery.
- Basic Auth - Easy to implement. But not secure, unless enforcing SSL, but still risky. Increase surface area of attacks, because sends credentials on every request.
- Token Auth - Most common, because it's a mixture of secure and simplicity. Should expire must faster than cookies, typically 5-20 minutes.
- OAuth - Use trusted third-party to identify. User authenticates with third party, use token to confirm identity. Safer for you and user.

</details>

<details><summary>

###### Credit(s)/Other(s)/Reference(s)/Source(s) </summary>

Done between: 09/2023  

</details>

</details>

------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

<details>
<summary>

## [ASP.NET Core 3 Microservices: Getting Started](https://app.pluralsight.com/courses/ea5ff9d5-c3d5-4d09-be4d-cc91d95b6c32/table-of-contents)</summary>
<details><summary>

#### How to Create a Microservice</summary>

**The microservice architectural style** applications consisting of small services. Each service can be considered as component, they are independent, each service can be can be maintained by multiple teams, their size make them manageable because they are small. Finally, each service must be responsible for exactly one task. The **UI** could be one big application communicating with the micro services.

**Evolving a Microservice** you must always ask if it still independent ? Does it still perform one task ? If not, it's time to add a microservice or refactor the architecture.
Microservices should not have dependencies on others micro services neither database, because it can break the process.

**Tip(s)**
Use NSwag can be used separately if you're not using Visual Studio, to create classes from openApi specification.
***gRPC*** pros: It's faster and has better performance than REST services. cons: The client must know upfront the behavior of the service consumed.
</details>

<details><summary>

#### Connecting Microservices Synchronously and Asynchronously </summary>


</details>

<details><summary>

#### Microservices Considerations and Design </summary>
|Architectural Style|Benefits|Downsides|
|-|-|-|
|Monolith|- Easier to deploy<br>- Easier to test<br>- Well known<br>- No calls over the wire<br>|- Hard to maintain modularity<br>- Bigger == more complex<br>- Bigger == Scaling out is expensive<br>- Fault tolerance is lower when you have problem on side of it<br>- Updates is hard to new techonologies<br>- Multiple teams get involved is hard to manage|
|Microservice|- Very large and/or complex applications are easier to develop<br>- Highly available<br>- Individual microservices are scalable<br>- Development experience better when many people collaborate<br>- Teams can use various technologies<br>- Fault isolation<br>  |- The solution as a whole is more complex<br>- How do all parts stick together?<br>- Deployment and monitoring<br>- Requires team skills beyond coding<br>- Properly sizing microservices<br>- Eventual consistency<br>|

Organization Readiness:
- Project-based approach doesn't work
- Devops
- Ownership
- Ground rules across teams
</details>

<details><summary>

###### Credit(s)/Other(s)/Reference(s)/Source(s) </summary>

[Course part of ASP.NET Microservices Path](https://app.pluralsight.com/paths/skills/net-microservices)

Done between: 10/2023

</details>

</details>

------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
<details><summary>

## [Microservices communication in ASP.Net Core 3](https://app.pluralsight.com/library/courses/microservices-communication-asp-dot-net-core) </summary>


<details><summary>

#### Introducing Microservice Communication in ASP.NET Core</summary>

</details>

<details><summary>

#### Creating Synchronous Communication between ASP.NET Core Microservices</summary>
This code, although uses async and await, it will wait for the response from the callee. This is an example of a synchronous communication.
```c#
public async Task<Coupon> GetCoupon(Guid couponId)
{
  var response = await
          client.GetAsync($"/api/discount/{couponId}");
  return await response.ReadContentAs<Coupon>();
}
```

<details><summary>

#### Inter-microservice communication</summary>
**Microservices inter-communication**
![](https://github.com/Ngofilho/Architecture/blob/images/images/MicroServicesLivingTogether.jpg)

**Synchronous communication**
![](https://github.com/Ngofilho/Architecture/blob/images/images/SynchronousCommunication.jpg)
</details>

<details><summary>

#### Working with gRPC</summary>
**gRPC** is contract base.
Proto file, which contains the protobuff or the protocol buffer, it's the contract that will be available from the service, base on this file the class will be generated. There are two main things, messages (are the definition of the data that will go over the gRPC service) and services (Are the definitions of the service capabilities so they contain the functionalities exposed over the service).
```javascript
syntax = "proto3";

option csharp_namespace = "GloboTicket.Grpc";

package API;

service Discounts {
	rpc GetCoupon (GetCouponByIdRequest) returns (GetCouponByIdResponse) {}
}

message GetCouponByIdRequest {
	string CouponId = 1;
}

message GetCouponByIdResponse {
	Coupon coupon = 1;
}


message Coupon {
	string CouponId = 1;
	string Code = 2;
	int32 Amount = 3;
	bool AlreadyUsed = 4;
}
```
First we define the message, each message contains the *name* and *type*, and the field contains an unique number which is used to identify each field. Next the proto file contains one or more services.

Based on the proto file above, the class is created like this bellow.
```c#
public class DiscountsService: Discounts.DiscountsBase
{
  public override async Task<GetCouponByIdResponse> GetCoupon(GetCouponByIdRequest request, ServerCallContext context)
  {
    ...
  }
}
```

And this is the calling gRPC service
```c#
private readonly Discounts.DiscountsClient discountService;

GetCouponByIdResponse getCouponByIdResponse =
  await discountsService.GetCouponAsync(getCouponByIdRequest);
```
</details>

</details>

<details><summary>

#### Setting up Asynchronous Communication between ASP.NET Core Microservices</summary>
**Communications Options**:
- Point-to-Point - We have a single receive that's going to the pipe, and that message will be processed only once
- Publish-Subscribe - One-to-many, where all the receivers can process the message.
</details>

<details><summary>

#### Making Microservices More Resilient</summary>

</details>

<details><summary>

#### Accessing a Microservices Infrastructure</summary>

</details>

<details><summary>

###### Credit(s)/Other(s)/Reference(s)/Source(s) </summary>

[Course part of ASP.NET Microservices Path](https://app.pluralsight.com/paths/skills/net-microservices)

Done between: 10/2023

</details>

</details>

------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
<details><summary>

## [Data Management Strategy](https://app.pluralsight.com/library/courses/implementing-data-management-strategy-asp-dot-net-core-microservices-architecture)</summary>


<details><summary>

###### Credit(s)/Other(s)/Reference(s)/Source(s) </summary>

[Course part of ASP.NET Microservices Path](https://app.pluralsight.com/paths/skills/net-microservices)

Done between: 10/2023

</details>

</details>

------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

<details><summary>

## [Securing Microservices in ASP.NET Core](https://app.pluralsight.com/library/courses/securing-microservices-asp-dot-net-core)</summary>

<details><summary>

### Securing Your First Microservice</summary>
**The easiest way to get started using identity service template is using the command `dotnet new --install identityserver4.templates`**. This command will install identity server 4 templates on to machine.
**SubjectId is UserId**

On Identity Server web page there is a document called **openid-configuration**, this document is used by clients and APIs services to learn how to interact with the identity server, for example to learn what key to use for validating token signatures.
</details>

Diminuir escopos das audiências
Token exchange

Audiência > Scope > Sub

Module 3 - Token Exchange Patterns

Module 4 - Implmenting Security with API Gateway and BFF Patterns (Example using Ocelot) and routes configurations

Module 5 - Improving the API Gateway Pattern - Using scoped-based microservices access authorization

Module 6 - Enabling long-lived access and token stores, how to use cached tokens

<details><summary>

###### Credit(s)/Other(s)/Reference(s)/Source(s) </summary>

[Course part of ASP.NET Microservices Path](https://app.pluralsight.com/paths/skills/net-microservices)

Done between: 10/2023

</details>

</details>

------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
<details><summary>

## [Versioning and Evolving Microservices in ASP.Net Core 3](https://app.pluralsight.com/library/courses/versioning-evolving-microservices-asp-dot-net-core)</summary>

Always increment or implement another feature creating new route, optional query string parameters and so instead of breaking change
Nuget package Microsoft.AspNet.Core.Versioning (supports versioning in >path, query string or header)

Version Interliving

It's possible to combine different strategies to use specific versions

<details><summary>

###### Credit(s)/Other(s)/Reference(s)/Source(s) </summary>

[Course part of ASP.NET Microservices Path](https://app.pluralsight.com/paths/skills/net-microservices)

Done between: 10/2023

</details>

</details>

------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
<details><summary>

## [Deploying ASP.Net Core 3 Microservices Using Kubernetes and AKS](https://app.pluralsight.com/library/courses/deploying-asp-dot-net-core-microservices-kubernetes-aks)</summary>


<details><summary>

###### Credit(s)/Other(s)/Reference(s)/Source(s) </summary>

[Course part of ASP.NET Microservices Path](https://app.pluralsight.com/paths/skills/net-microservices)

Done between: 10/2023

</details>

</details>

------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
<details><summary>

## [Implementing Cross-Cutting Concerns for ASP.Net Core 3 Microservices](https://app.pluralsight.com/library/courses/implementing-cross-cutting-concerns-asp-dot-net-core-microservices)</summary>

[Repository with the course's code](https://github.com/Ngofilho/ArchitectureCrossCutting)

<details><summary>

### 02 - Implementing Logging</summary>
Used to:
- Understand behaviour of a service
- Identify errors for investigation
- Diagnose bugs and failures
It's important to log information you may later depende on.
The first decision to taken about the log is **what** to log and after this decision is **when**
**Logging Requirements**
  What Information will be needed to diagnose a bug or runtime error ?
  Balance between logging too much or too little:
  - Log enough to be useful
  - Avoid introducing redundant noise

  Messages should contain enough detail and data to support proper analysis
  - For example, record request and/or resource IDs.

**Log Levels**
Tag messages with metadata about the importance of the event
Each message includes a log level
Log messages can be filtered based on their log level
**Filtering by Log Level**
Always log erros and exceptions and sometimes log conditional application flow.
**Microsoft Log Levels**
|Level|Usage|Use in Production ?|
|-|-|-|
|Trace|Log detailed messages during developement|never|
|Debug|Log verbose messages (occasionally in production|Yes, for a short period o time|
|Information|Log general flow of requests/operations|Sometimes, just to ensure behaviours|
|Warning|Log non-critical but abnormal events|Yes|
|Error|Log exceptions which cannot/are not gracefully handled|Yes|
|Critical|Log major failures which require immediate attention|Yes|

**Log filtering settings**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",  //Sets the default logging level accross the entire application. Means that Information and higher will be recorded by default
      "Microsoft": "Warning",    			//Overrides for other categories, for example this will log messages categories which start with Microsoft
      "Microsoft.Hosting.Lifetime": "Information",	//This override will log messages that starts with Microsoft.Hosting.Lifetime, starting as Information, this filter is more specific and appears aftewards the previous one, so it takes precedence.
      "System.Net.Http.HttpClient": "Warning"		//Will log messages with this category starting only with the log level of Warning.
    }
  }
}
```

**Sharing Common Logging Code**
1. Create a `common` project.
2. Create a Extension class with an extension method to handle the log.
3. Create another class that inherits the `DelegatingHandler` class.
4. Overrides the `SendAsync` method.
```c#
namespace GloboTicket.Common
{
//step 1 and step 2
public static class LoggerExtensions
{
    public static void LogHttpResponse(this ILogger logger, HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            logger.LogDebug("Received a success response from {Url}", response.RequestMessage.RequestUri);
        }
        else
        {
            logger.LogWarning("Received a non-success status code {StatusCode} from {Url}", (int)response.StatusCode, response.RequestMessage.RequestUri);
        }
    }
}

//step 3 and step 4
    public class LoggingDelegatingHandler : DelegatingHandler
    {
        private readonly ILogger<LoggingDelegatingHandler> logger;

        public LoggingDelegatingHandler(ILogger<LoggingDelegatingHandler> logger)
        {
            this.logger = logger;
        }


        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancelationToken)
        {
            try
            {
                var response = await base.SendAsync(request, cancelationToken);
                logger.LogHttpResponse(response);
                return response;
            }
            catch (HttpRequestException ex) when (ex.InnerException is SocketException se && se.SocketErrorCode == SocketError.ConnectionRefused)
            {
                var hostWithPort = request.RequestUri.IsDefaultPort ?
                       request.RequestUri.DnsSafeHost
                       : $"{request.RequestUri.DnsSafeHost}:{request.RequestUri.Port}";
                logger.LogCritical(ex, "Unable to connect to {Host}. Please check the " +
                    "configuration to ensure the correct URL for the service " +
                    "has been configured.", hostWithPort);
            }
            return new HttpResponseMessage(System.Net.HttpStatusCode.BadGateway)
            {
                RequestMessage = request
            };
        }
    }
}
```
</details>

<details><summary>

### 03 - Implementing Centralized Logging for Microservices</summary>

In this module, the teacher shows the following processes:
- How to configure applications to use Serilog.
- How to configure Kibana indexes.
- How to enrich log messages with environment, application name.
- How to format exception data.

Teaches how to config `ELK`
</details>

<details><summary>

### 04 - Implementing Health Checks in Microservices</summary>

[Source code containing the implementation with Azure Service Bus, it's missing the implementation of Healt check for RabbitMq](https://github.com/Ngofilho/ArchitectureCrossCutting)

Teachs how to implement Health Check

</details>

<details><summary>

###### Credit(s)/Other(s)/Reference(s)/Source(s) </summary>

[Course part of ASP.NET Microservices Path](https://app.pluralsight.com/paths/skills/net-microservices)

Done between: 10/2023

</details>

</details>

------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
<details><summary>

## [Strategies for Microservice Scalability and Availability in ASP.Net Core](https://app.pluralsight.com/library/courses/strategies-microservice-scalability-availability-asp-dot-net-core)</summary>

Re-watch this course and make the examples
**Load Balancer**
- **F**ront**E**nd IP (FE-IP)
- **B**ack**E**nd Pool (BE-Pool)
- **L**oad**B**alancer Rules (LB-Rules)
- Health Probe

[Repository with the course's code](https://github.com/Ngofilho/ArchitectureAvailabilityScalability)


<details><summary>

###### Credit(s)/Other(s)/Reference(s)/Source(s) </summary>

[Course part of ASP.NET Microservices Path](https://app.pluralsight.com/paths/skills/net-microservices)

Done between: 10/2023

</details>

</details>

------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

<details><summary>

## [Implementing Advanced RESTful Concerns with ASP.NET Core 3](https://app.pluralsight.com/library/courses/asp-dot-net-core-3-advanced-restful-concerns)</summary>

<details><summary>

### Paging through Collection Resources </summary>

It's considered best practice to always implement paging on each resource collection, or at least on those resources that can also be created. This is to avoid unintended negative effects on performance when resource collections grow. Not having paging on a list of 10 authors might be okay, but if your API allows creating authors, this list can grow, and we don't want to end up with accidentally returning thousands of authors in one response. Pagination parameters are typically passed through via the query string. As far as paging is concerned, the consumers should be able to choose the page number and page size, but that page size can cause problems as well. A consumer can pass through 100,000 as the page size. So the page size itself should be checked against a specific value to see if it isn't too large. If no paging parameters are provided, we should only return the first page by default. So, we are manipulating a collection resource. For things like paging to work correctly and have a positive impact on performance, we need to ensure that this goes all the way through to our data store. For example, **if we have thousands of orders in our database and we first return all those authors from a repository to the controller and then page them, we still fetch way too much data from the database**.
</details>


<details><summary>

### Sorting Resource Collections </summary>

Sorting requires to handle the parameters sent by the request to specify how to sort the response. A good practice is to have a default value of sort. Sorting also requires that if there is a fail with the sorting parameters like an unspecified sort parameter, the response must be 400 class (Client request error) not 500 class (Server Error).
Another good practice is to return the sort parameter in the response pagination header.  
One approach to implement Sort is using a helper class, as extension. This class below is example of implementation

```c#
public static class IQueryableExtensions
{
    public static IQueryable<T> ApplySort<T>(this IQueryable<T> source, string orderBy, Dictionary<string, PropertyMappingValue> mappingDictionary)
    {
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

        var orderByString = string.Empty;

        // the orderBy string is separated by ",", so we split it.
        var orderByAfterSplit = orderBy.Split(',');


        // apply each orderby clause in reverse order - otherwise, the 
        // IQueryable will be ordered in the wrong order
        foreach (var orderByClause in orderByAfterSplit.Reverse())
        {
            // trim the orderBy clause, as it might contain leading
            // or trailing spaces. Can't trim the var in foreach,
            // so use another var
            var trimmedOrderByClause = orderByClause.Trim();

            // if the sort option ends with "desc", we order
            // descending, otherwise ascending
            var orderDescending = trimmedOrderByClause.EndsWith(" desc");

            // remove " asc" or " desc" from the orderByClause, se we
            // get the property name to look for in the mapping dictionary
            var indexOfFirstSpace = trimmedOrderByClause.IndexOf(" ");
            var propertyName = indexOfFirstSpace == -1 ?
                trimmedOrderByClause : trimmedOrderByClause.Remove(indexOfFirstSpace);

            // find the matching property
            if(!mappingDictionary.ContainsKey(propertyName))
            {
                throw new ArgumentException($"Key mapping for {propertyName} is missing");
            }

            // get the PropertyMappingValue
            var propertyMappingValue = mappingDictionary[propertyName];

            if (propertyMappingValue == null)
            {
                throw new ArgumentNullException("propertyMappingValue");
            }

            // Run through the property names
            // so the orderby clauses are applied in the correct order
            foreach (var destinationProperty in propertyMappingValue.DestinationProperties)
            {
                // rever sort order if necessary
                if(propertyMappingValue.Revert)
                {
                    orderDescending = !orderDescending;
                }

                orderByString = orderByString +
                    (string.IsNullOrWhiteSpace(orderByString) ? string.Empty : ", ")
                    + destinationProperty
                    + (orderDescending ? " descending" : " ascending");
            }

        }

        return source.OrderBy(orderByString);
    }
}

```

</details>


<details><summary>

### Shaping Data</summary>



</details>

<details><summary>

### Learning and Implementing **HATEOAS** </summary>

</details>

<details><summary>

### Improving Reliability with Advanced **Content Negotiation** </summary>

</details>

<details><summary>

### Getting Started with Caching Resources</summary>

HTTP Caching  
http://bit.ly/2hJTTxD (RFC 2616)  
http://bit.ly/2in4uzh (RFC 7234)  


**`Cache-Control` header**  
[Directives](http://bit.ly/1Ups120)  

</details>


<details><summary>

### Supporting HTTP Cache for ASP.NET Core APIs </summary>



</details>


<details><summary>

### Concurrency </summary>

</details>

<details><summary>

###### Credit(s)/Other(s)/Reference(s)/Source(s) </summary>

[Course part of the API Development in ASP.NET Core](https://app.pluralsight.com/paths/skills/api-development-in-aspnet-core)

Done between: 01/15/2024 - 01/18/2024

</details>

</details>

------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

<details>
	
<summary>

###### Tool(s)</summary>

[Excalidraw](https://excalidraw.com/)  
[Diagrams](https://app.diagrams.net/)


<details><summary>

###### Event Storming
</summary>

[Event Storming Ziobrand's Lair](https://ziobrando.blogspot.com/)  

</details>


<details><summary>

###### Impact Mapping
</summary>

[Impact Mapping WebSite](https://www.impactmapping.org/)    
</details>

</details>

<details><summary>

###### Annotation(s)/Credit(s)/Example(s)/Other(s)/Reference(s)/Sample(s)/Studie(s)</summary>


[Documentação de Como Montar Ambientes e Suas Configurações CNJ 253 - Podcast Hipsters.Net #342](https://docs.pje.jus.br/)  
[Linkedin Article "Want to Become a Software Engineer"](https://www.linkedin.com/feed/update/urn:li:activity:7146810352159113216/)  

[High Scalibity](https://highscalability.com/)   


[25 Best Software Architecture Blogs and websites](https://developer.feedspot.com/software_architecture_blogs/)  
[High Scalability Web Site](http://highscalability.com/)  
[A Pattern Language for Microservices](https://microservices.io/patterns/index.html)  

[Post Exemplo de Ganhador de Hackton de Arquitetura de Software](https://www.linkedin.com/posts/dannevesdantas_vencemos-o-hackathon-de-software-architecture-ugcPost-7240451617194426368-6v1e?utm_source=share&utm_medium=member_desktop)   
[Git repository for this example](https://github.com/Grupo-G03-4SOAT-FIAP/Health-Med-api)   

<details><summary>

###### 7 popular GitHub repos on software architecture 
</summary>

[Post Link](https://www.linkedin.com/posts/kristijankralj_confession-i-cant-stop-collecting-github-activity-7242776149120978944-t9Cb?utm_source=share&utm_medium=member_desktop)   
[Evolutionary Architecture By Example, repo from Linkedin Article "Want to Become a Software Engineer" ](https://github.com/evolutionary-architecture/evolutionary-architecture-by-example?tab=readme-ov-file#problem)    
[Modular Monolith with DDD, another repo from Linkedin Article "Want to Become a Software Engineeer"](https://github.com/kgrzybek/modular-monolith-with-ddd)     
[.NET 8 starter kit with multitenancy support](https://github.com/fullstackhero/dotnet-starter-kit/)   
[eCommerce microservice .NET application](https://github.com/dotnet/eShop)      
[Vertical slice architecture example](https://github.com/jbogard/ContosoUniversityDotNetCore-Pages)     
[Clean architecture template for .NET apps](https://github.com/jasontaylordev/CleanArchitecture)    
[Hexagonal application example](https://github.com/ivanpaulovich/clean-architecture-manga)     

</details>

</details>


<hr>

# System Design  

<details><summary>

 ###### Annotation(s)/Credit(s)/Demo(s)/Example(s)/Other(s)/Reference(s)/Sample(s)/Source(s)/Stud(y)(ies)/Thank(s)       
</summary>

[15 articles to help you get better at System Design](https://www.linkedin.com/posts/saurabh-dashora_15-articles-to-help-you-get-better-at-system-activity-7242781540236099584-5vGS?utm_source=share&utm_medium=member_desktop)  
[3 Interview Questions on Event-Driven Patterns](https://newsletter.systemdesigncodex.com/p/3-interview-questions-on-event-driven)   
[Message Queues and Message Brokers](https://newsletter.systemdesigncodex.com/p/message-queues-and-message-brokers)   
[Database Sharding](https://newsletter.systemdesigncodex.com/p/database-sharding)   
[Polling vs Webhooks](https://newsletter.systemdesigncodex.com/p/polling-vs-webhooks)   
[Normalization vs Denormalization](https://newsletter.systemdesigncodex.com/p/normalization-vs-denormalization)   
[Airbnb’s Migration from Monolith to Microservices](https://newsletter.systemdesigncodex.com/p/airbnb-migration-from-monolith-to)   
[7 Techniques for Database Performance and Scaling](https://newsletter.systemdesigncodex.com/p/7-techniques-for-database-performance)   
[Must-Know Service Communication Patterns](https://newsletter.systemdesigncodex.com/p/service-communication-patterns)   
[8 Strategies for Reducing Latency](https://newsletter.systemdesigncodex.com/p/8-strategies-for-reducing-latency)   
[Microservices Patterns](https://newsletter.systemdesigncodex.com/p/microservices-patterns)   
[Change Data Capture & Microservices](https://newsletter.systemdesigncodex.com/p/change-data-capture-and-microservices)   
[3 Types of Event Patterns in EDA](https://newsletter.systemdesigncodex.com/p/3-types-of-event-patterns-in-eda)   
[Load Balancers vs API Gateways vs BFFs](https://newsletter.systemdesigncodex.com/p/load-balancers-vs-api-gateways-vs)   
[An Intro to LSM Trees](https://newsletter.systemdesigncodex.com/p/an-intro-to-lsm-trees)   
[SDC#1 - CAP Theorem with Tom the Prankster](https://newsletter.systemdesigncodex.com/p/cap-theorem)    

</details>
