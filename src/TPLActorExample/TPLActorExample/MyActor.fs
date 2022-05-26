namespace TPLActorExample

open System.Collections.Generic
open System.Threading.Tasks
open System.Threading.Tasks.Dataflow
open TPLActorExample.Extensions

type ActorMessage =
    | Add    of key:string * value:int
    | Remove of key:string
    | Get    of key:string * reply:TaskCompletionSource<Option<int>>    

type MyActor() =    
    
    let dict = Dictionary()
    
    let actor = ActionBlock(fun msg ->
        match msg with
        | Add (key, value) -> Dict.set key value dict
        | Remove key       -> Dict.remove key dict
        
        | Get (key, reply) ->
            match dict.TryGetValue key with
            | true, value -> reply.SetResult(Some value)
            | false, _    -> reply.SetResult(None)
    )
    
    member _.Publish(msg) = actor.Post(msg) |> ignore    
    
    member _.GetValue(key) =
        let reply = TaskCompletionSource<Option<int>>()
        let msg = Get(key, reply)
        actor.Publish msg        
        reply.Task