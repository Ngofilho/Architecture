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
- **Error Handling** -  
- **Caching** -
- **Functional** -  
</details>

</details>
