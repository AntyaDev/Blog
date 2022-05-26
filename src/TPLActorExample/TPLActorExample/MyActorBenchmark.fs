namespace TPLActorExample.MyActorBenchmark

open System.Collections.Concurrent
open System.Collections.Generic
open System.Runtime.CompilerServices
open System.Threading.Tasks
open System.Threading.Tasks.Dataflow
open BenchmarkDotNet.Attributes
open TPLActorExample.Extensions

type ActorMessage =
    | Add    of key:int * value:int
    | Remove of key:int
    | Get    of key:int * reply:TaskCompletionSource<Option<int>>    

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
    
    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    member _.Publish(msg) = actor.Post(msg) |> ignore    
    
    member _.GetValue(key) =
        let reply = TaskCompletionSource<Option<int>>()
        let msg = Get(key, reply)
        actor.Publish msg        
        reply.Task
 
[<MemoryDiagnoser>]
type TPLDataflowBenchmark() as this =
 
    [<Params(100_000, 1_000_000)>]
    member val MsgCount = 0 with get, set
 
    [<Benchmark>]
    member _.TPLDataFlow() =
        let actor = MyActor()
 
        [| 0..this.MsgCount |]        
        //|> Array.Parallel.iter (fun i ->
        |> Array.iter (fun i ->
            let isEven = i % 2 = 0
            if isEven then
                actor.Publish(Add(i, i))
            else
                actor.Publish(Remove i)
        )
 
        actor.GetValue(42).Result |> ignore
        
    [<Benchmark>]
    member _.ConcurrentDictionary() =
        let dict = ConcurrentDictionary<int,int>()
 
        [| 0..this.MsgCount |]
        //|> Array.Parallel.iter (fun i ->
        |> Array.iter (fun i ->
            let isEven = i % 2 = 0
            if isEven then
                dict[i] <- i
            else
                dict.Remove(i) |> ignore
        )