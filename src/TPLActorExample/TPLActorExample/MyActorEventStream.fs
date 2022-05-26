namespace TPLActorExample.EventStreamExample

open System
open System.Collections.Generic
open System.Threading.Tasks.Dataflow
open FSharp.Control.Reactive
open TPLActorExample.Extensions

type ActorMessage =
    | Add    of key:string * value:int
    | Remove of key:string        

type ActorEvent =
    | Added   of key:string * value:int
    | Removed of key:string

type MyActorEventStream() =    
    
    let dict = Dictionary()
    let eventStream = Subject.broadcast
        
    let publishEvent (event) =
        eventStream.OnNext event
    
    let actor = ActionBlock(fun msg ->
        match msg with
        | Add (key, value) ->            
            Dict.set key value dict
            publishEvent (Added (key, value))
        
        | Remove key ->
            Dict.remove key dict
            publishEvent (Removed key)
    )
    
    member _.EventStream = eventStream :> IObservable<ActorEvent>
    
    member _.Publish(msg) = actor.Post(msg) |> ignore