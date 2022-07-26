# Sample Registry Service

The sample registry service implements the bare minimum needed for a Registry service.  This is not intended to be an example of how to write a service, but rather to function as a minimal Registry if you are using a multi-host architecture. 

Note: 

* Because all of the "common" stuff is in XKit.Lib.Common, we don't have a Common project.  For the same reason, we don't need a Client project.  The XerviceKit framework itself talks to the Registry.
* There is no Test project because of the simplistic nature of this service, as well as pure laziness at the time of writing this document. 
* This sample is not "production ready".  For one thing, if the Registry service goes down, it has no way of re-acquiring registration information.  Optimally, it should save this information in a temporary database of some kind so that upon restart it could return with the same registrations.