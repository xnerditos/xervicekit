# Sample MessageBroker Service

The Message Broker service is an example of a fairly complex service.  It has logic to retry failed messages and act accordingly. 

Note that the approach to testing is to 

1) Separate out the core logic and test is separately 
2) Test each of the overall units; the methods, the daemon functionality, etc.

This is service is not "production ready" in that all the subscriptions and queue states are held in memory and will be lost when the service goes down.  However it should provide a decent guide for building your own.  Additionally, it should serve as a good example of other things like use of daemons.
