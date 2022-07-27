# Xervicekit

Xervicekit is a framework intended to provide a backbone for implementation of microservices. It tries to provide certain common functionality, allowing a developer to concentrate on the logic of the service itself without having to worry about the mechanics.  Service calls are abstracted to method calls, so that the logic does not need to concern itself with how data is moved or where the code is actually executing. 

If you are interested in a boring "salesy" presentation about what XerviceKit is trying to accomplish, feel free to [take a look at this slide presentation](https://docs.google.com/presentation/d/e/2PACX-1vQbOuhXQfXIGNlS4XHKKq51As8-iKhA-J2us-SeW4vkWX_dnTmDOGcUYHjc5Cw7WTY3800SVK3DrTcC/pub?start=false&loop=false&delayms=3000). 

## Getting started

If you are looking to get some code down quickly, we recommend reading over the [Overview of basic concepts](docs/BasicConcepts.md) and then running through [the first tutorial](./tutorials/Tutorial001_Create_A_Basic_Service) which will run you through setting up a very simple basic service and making a service call from the command line.

Note that the first tutorial is by necessity a very simplified approach to setting up a service.  If you have the time for it and want to go a bit deeper, the [second tutorial](./tutorials/Tutorial002_Organize_And_Add_Client_And_Tests/) reorganizes the code and follows the patterns recommended in the documentation. 

## Basics

These pages try to take you through the basic things you will need to accomplish specific tasks.  If you are looking for advanced information, check out the "deep dives" instead. 

* [Overview of basic concepts](docs/BasicConcepts.md)
* [Setting up a Host](docs/GettingStarted-Hosts.md)
* [Setting up a Service](docs/GettingStarted-Services.md)
* [Testing a Service Api](docs/GettingStarted-TestingServiceApi.md)
* [Testing Services with dependencies](docs/GettingStarted-TestingServiceDependencies.md)
* [Working with Clients and Consumers](docs/GettingStarted-Clients.md)
* [Working with Events and Commands](docs/GettingStarted-EventsAndCommands.md)
* [Working with Daemons](docs/GettingStarted-Daemons.md)

## Deep dives

These pages look at aspects of the XerviceKit framework with a deeper perspective.  These are generally not necessary unless you are trying to do something a bit more advanced.

* [TBD: Deep dive:  Hosts](docs/DeepDive-Hosts.md)
* [TBD: Deep dive:  Services](docs/DeepDive-Services.md)
* [TBD: Deep dive:  Operations](docs/DeepDive-Operations.md)
* [TBD: Deep dive:  Clients and Consumers](docs/DeepDive-ClientsAndConsumers.md)
* [TBD: Deep dive:  Platform services](docs/DeepDive-PlatformServices.md)
* [TBD: Deep dive:  Services calls](docs/DeepDive-ServiceCalls.md)
* [TBD: Deep dive:  Events and Commands](docs/DeepDive-EventsAndCommands.md)
* [TBD: Deep dive:  Daemons](docs/DeepDive-Daemons.md)
* [TBD: Deep dive:  Meta Services](docs/DeepDive-MetaServices.md)

## Learning

* [Tutorials](./tutorials)
* [Samples](./samples)
* [Slide presentation:  Basics](https://docs.google.com/presentation/d/e/2PACX-1vQbOuhXQfXIGNlS4XHKKq51As8-iKhA-J2us-SeW4vkWX_dnTmDOGcUYHjc5Cw7WTY3800SVK3DrTcC/pub?start=false&loop=false&delayms=3000)
* [Slide presentation:  Hosts, Services, and Operations](https://docs.google.com/presentation/d/e/2PACX-1vRNh9SUTmjQL7RU53XTisA0ALbF9kSj6GUhU04D6qbHmSEUJ2j7YV-vN3hvTRoHijy2Q0zf57w43knW/pub?start=false&loop=false&delayms=3000)

## Other

* TBD: Noodling about application architecture 
* TBD: Noodling about service categories
* TBD: Noodling about service rings
* TBD: How does XerviceKit relate to / compare to Service Mesh?

----------------------

