# [Distributed Systems](https://app.pluralsight.com/library/courses/distributed-systems-fundamentals)  
##### Properties of a Reliable Application  
1. Idempotence  - Help to tolerate unreliable networks. When the client tries to repeat some failed operation due the network flaw, the application won't duplicate the effect if it receives duplicate message.    
2. Immutability - Keeps a record of everything that happens within the system, the data won't be overridden or deleted.  
3. Location Independence - Deployed at different locations.   
4. Versioning - Tolerance to changes onto the system.  

##### Fallacies of Distributed Computing  
1. _The network is reliable_  
2. Latency is zero  
3. Bandwidth id infinite  
4. The network is secure  
5. _Topology doesn't change_  
6. There is one administrator  
7. Transport cost is zero  
8. _The network is homogeneous_  


Idempotence operation is one that has no effect when it's received twice or more. To implement idempotence one approach could be using *Clinet-Side ID*. For example, if the user know the ID of the object before it's created, then the server can tell the difference between the creation of a new object and a duplicate message. Duplicate messages will carry the same IDs as the original.  
Another approuch can be implementing *Client-side ID in database*, either using a different ID than auto-increment key, *GUID* is a good choice as an alternate key, or before inserting the record, searching for it using the same parameters as being used on the insert, if it returns a record, then just returns it to the client otherwise, insert and then returns the outcome to the client.  
