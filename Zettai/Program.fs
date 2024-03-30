module Program

open Microsoft.Extensions.Hosting
open Domain

type Database = Map<User, Map<ListName, ToDoList>> ref
type InMemoryEventStore = Event<ListName, ListEvent> seq ref
let store: InMemoryEventStore = ref []

let streamLookup (store: InMemoryEventStore) : ListLookup =
    fun u ln ->
        store.Value
        |> Seq.filter (fun e -> e.AggregateId = ln)
        |> Seq.map (fun e -> e.Data)
        |> ListEvent.fold (ToDoList.initial ln)

let writeEvent (store: InMemoryEventStore) : EventWriter<ListName, ListEvent> =
    fun e -> store.Value <- Seq.append store.Value [ e ]

[<EntryPoint>]
let main _ =
    Host
        .CreateDefaultBuilder()
        .ConfigureWebHostDefaults(
            ZettaiHost.configure (streamLookup store) (writeEvent store)
            >> ignore
        )
        .Build()
        .Run()

    0
