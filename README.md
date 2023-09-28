# Annotations
<details>  
<summary>
  
## [Distributed Systems](https://app.pluralsight.com/library/courses/distributed-systems-fundamentals)  </summary>


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
**Queries** are messages that have no side effects and return data. Usually have a name of especification.     
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
![](https://github.com/Ngofilho/Architecture/blob/images/images/ERD.jpg) ![](https://github.com/Ngofilho/Architecture/blob/images/images/DbRelationDiagram.jpg)  ![](https://github.com/Ngofilho/Architecture/blob/images/images/ERDHierarchy.jpg)  
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


</details>

</details>

<details><summary>

#### Setting up Asynchronous Communication between ASP.NET Core Microservices</summary>

</details>

<details><summary>

#### Making Microservices More Resilient</summary>

</details>

<details><summary>

#### Accessing a Microservices Infrastructure</summary>

</details>
</details>

------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
