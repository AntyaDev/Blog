module TPLActorExample.Program

open System
open System.Threading.Tasks
open BenchmarkDotNet.Running
open FsToolkit.ErrorHandling

[<EntryPoint>]
let main argv =
    
//    BenchmarkRunner.Run<TPLActorExample.MyActorBenchmark.TPLDataflowBenchmark>()
//    |> ignore
    
    let actor = MyActor()
    
    actor.Publish(Add("key1", 1))
    actor.Publish(Add("key2", 2))
        
    let v = actor.GetValue("key1").Result        
        
    // GetValue: string -> Task<Option<int>>
    let t = task {
        match! actor.GetValue("key1") with
        | Some v -> Console.WriteLine v
        | None   -> Console.WriteLine "key not found"
    }
    
    let t = taskOption {
        let! value = actor.GetValue("key1")
        Console.WriteLine value        
    }
    
    t.Wait()
    
    0