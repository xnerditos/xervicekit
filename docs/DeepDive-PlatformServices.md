# Platform Services

Certain features offered by the XerviceKit framework require special services to make them work, especially when in a multi-host environment.  These are only a few at the moment, but may grow to include more in the future. 

## Registry service

The Registry service has the job of keeping track of what services are available in the service fabric and where they are located.  When a host starts up, it has the address of just one instance of the Registry.  When it registers itself by a call to the Registry, it provides a list of what services it has running and what dependencies those services have.  The response from the Registry includes a list of all services instances it might need which includes other instances of the Registry if they exist.  

In the event that an instance of the Registry is not available, if the host has the Direct protocol enabled, the framework will "see" only local services.  This allows services within the same host to communicate with each other.  

In the future, the Registry service might start "pushing" new updates to hosts that have the capability to receive them. 

## Message Broker service

When a service starts up, it indicates what Events and Commands it subscribes to.  This allows it to be informed when one of these occurs.  In a multi-host environment, the subscriptions are related to a service called the Message Broker.  When an Event or Command is emitted. it is also relayed to the Message Broker.  The Message Broker then relays the Event or Command (together referred to as a "Message") to all subscribers. 

## Config service

Services can load their configuration from a local folder, but they can also have configuration centrally managed.  That is the job of the Config service.  At the time of the writing of this wiki, the Config service is not implemented, although the basic functionality is present in the framework.  


