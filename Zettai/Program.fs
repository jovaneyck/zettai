module Program

open Microsoft.Extensions.Hosting
open Infrastructure
open Domain

type InMemoryEventStore = Event<ListId, ListEvent> seq ref

let store: InMemoryEventStore =
    let list = ListId.mint ()

    [ { AggregateId = list
        Data =
          ListCreated
              { User = User "jo"
                ListName = ListName.fromTrusted "books" } }
      { AggregateId = list
        Data = ItemAddedToList { Item = { Description = "Tidy First?" } } } ]
    |> Seq.ofList
    |> ref

let lookupIdByName (store: InMemoryEventStore) =
    fun u ln ->
        store.Value
        |> Seq.pick (fun e ->
            match e.Data with
            | ListCreated c ->
                if c.User = u && c.ListName = ln then
                    Some e.AggregateId
                else
                    None
            | _ -> None)

let lookupByName (store: InMemoryEventStore) u ln =
    let id = lookupIdByName store u ln

    store.Value
    |> Seq.filter (fun e -> e.AggregateId = id)
    |> ListEvent.fold (ToDoList.initial id ln)

let writeEvent (store: InMemoryEventStore) e =
    store.Value <- Seq.append store.Value [ e ]

[<EntryPoint>]
let main _ =
    Host
        .CreateDefaultBuilder()
        .ConfigureWebHostDefaults(
            ZettaiHost.configure (lookupByName store) (writeEvent store)
            >> ignore
        )
        .Build()
        .Run()

    0
