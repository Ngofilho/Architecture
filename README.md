# [Distributed Systems](https://app.pluralsight.com/library/courses/distributed-systems-fundamentals)  
##### Properties of a Reliable Application  
1. Idempotence  - Help to tolerate unreliable networks. When the client tries to repeat some failed operation due the network flaw, the application won't duplicate the effect if it receives duplicate message.    
2. Immutability - Keeps a record of everything that happens within the system, the data won't be overridden or deleted.  
3. Location Independence - Deployed at different locations.   
4. Versioning - Tolerance to changes onto the system.  

##### Fallacies of Distributed Computing  
1. __The network is reliable__  
2. Latency is zero  
3. Bandwidth id infinite  
4. The network is secure  
5. __Topology doesn't change__  
6. There is one administrator  
7. Transport cost is zero  
8. __The network is homogeneous__  