module TPLActorExample.Extensions

open System.Collections.Generic
open System.Runtime.CompilerServices
open System.Threading.Tasks.Dataflow

module Dict =
    
    let inline set (key) (value) (dict: Dictionary<_,_>) =
        dict[key] <- value
        
    let inline remove (key) (dict: Dictionary<_,_>) =
        dict.Remove(key) |> ignore
        
[<Extension>]        
type ActionBlockExtensions() =
    
    [<Extension>]
    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    static member Publish(block: ActionBlock<'T>, msg: 'T) =
        block.Post(msg) |> ignore