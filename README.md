# T.Pipes

A wrapper for [H.Pipes](https://github.com/HavenDV/H.Pipes)
For Surrogate implementation and other things too.

## Features

- Incremental Code Generator for `interface to command` and `command to interface` boilerplate using Attributes
  (Generated `Functions` can be only overriden staticly, but be warned to apply that correcly as there is one Collection per Unique generic type)
- A semi prepared [Newtonsoft.Json](https://www.newtonsoft.com/) serialzer for remoting
  (using deprecated binary serializer when needed)
- A simple `SurrogateProcessWrapper` to start/stop a surrogate safely
  (it only `kills` it after timeout, if possible use disconnect or other packet as a termination signal, the natural exit will be awaited for the timeout duration)
- Pipe ConnectionBase/Client/Server/CallbackBase as wrappers for `H.Pipes`
- Delegating Pipe Client/Server/Callback implementation for Interface delegation
- Spawning Pipe Client/Server/Callback implementation to Produce Delegating Pipes on demand
- Pipe/Callback objects derive from `BaseClass` which allow to also dispose on `LifetimeCancellation` token and listen to cancellation events
  (usefull in no IoC scenarios to handle cascading disposing)
- Generic Classes for the most part allowing to create targeted non-generic sealed classes for optimal performace
- Simple example application in `T.Pipes.Test.Client/Server`
- A bunch of unit tests `T.Pipes.Test` to assert some of the issues which might arise are handled

More indepth info can be gathered by strolling trough the Code or by reading the XML docs.

## Code generator

If you add the Code generator to project, the `PipeUse` and `PipeServe` attributes will be generating boilerplate for your `Delegating Pipe Callback` implementations. 

The interface to generate code for must be specified using typeof operator in the attribute parameter. 
Multiple instances of this attribute are allowed. All Generic types must be Closed.

- `PipeUse` Will Use the actual `Target` implmentation and make the callback act like an adapter for properties/methods/events
- `PipeServer` Will explicityly implement the interface on the callback to avoid collisions and make the callback act like an adapter for properties/methods/events

From the Pipe perspective all calls go trough collected static `Functions` property to lookup correct action.
Aliased implementations are visible from the Callback itself.

There is an edge case for any method of Disposing - ideally you just want to dispose the entire Callback/Server/Client/Callback/Target stack nad not just the `Target` object itself.
The dispose can be delegated to the target only, but then it is up to the user to handle reuse of pipes and etc., it should be possible thanks to protected Setter on `Target`.

Usage example can be found in `T.Pipes.Test.Client/Server`.

## Delegating Pipe Server/Client/Callback

Abstract classes to build the Interface Delegation upon. Should be ideally implemented by user using a sealed class.
All actual logic lands in `Callback`, the `Server`/`Client` are just managing the pipe.

## Spawning Pipe Server/Client/Callback

Abstract classes to create more Delegating Pipes. Should be ideally implemented by user using a sealed class.
All actual logic lands in `Callback`, the `Server`/`Client` are just managing the pipe.

## Surrogate Process Wrapper

Is a simple class with start and stop methods.

- `Starting` first ensures the previous instance of the process is `Stopped` then calls `Start` on the Process.
- `Stopping` the process tries to send the `CloseMainWindow`, and then awaits graceful close, if the timeout will expire `Kill` is called on the process.
