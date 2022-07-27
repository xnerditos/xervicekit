## Satisfying service dependencies for integration tests ##

Satisfying dependencies in an integration test can be done in one of two ways.  

1) You can include the dependency services in the test project in their entirety and let
   the test call the "real" services.  Note that you have to include dependencies all the way
   down the tree.  Obviously, this is more complex, but a truer
   resprentation of the real environment. 

2) You can mock the dependencies by creating mock services to stand in for the real ones.

### How to include a real service dependency ###

1) Include the Common library for your target service in the test project.  

2) Include the real service in the `AddServices` method for the HostEnvironment of the test project.

### How to mock a service dependency ###

If you mock the dependencies, you can follow the example given in the `Example` folder.  

1) Include the Common library for your target service in the test project.  

2) Create a mock operation that implements the "Api" interface from the target service

3) Include the mock in the `AddServices` method for the HostEnvironment of the test project.

### Notes on a hybrid approach ###

If you choose to use "real" services in the test, at some point it is quite likely that as you get to
the bottom of the dependency tree, you will have to mock at least one of the services.  This is entirely
possible to do.  You can mix and match between these two methods to produce as realistic a test environment as you like. Mocks for service dependencies even way down the tree will be found just fine
and substituted for the real thing if you follow these directions. 
