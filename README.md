# [Distributed Systems](https://app.pluralsight.com/library/courses/distributed-systems-fundamentals)  
#### Properties of a Reliable Application  

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
Deployed at different locations.  
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


