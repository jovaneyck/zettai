module Program

open Microsoft.Extensions.Hosting
open Types

type Database = Map<User, Map<ListName, ToDoList>> ref
let store = ref []

let db: Database =
    [ User "jo",
      [ (ListName.fromTrusted "books",
         { Name = ListName.fromTrusted "books"
           Items =
             [ { Description = "Tidy First?" }
               { Description = "Team Topologies" } ] }) ]
      |> Map.ofList ]
    |> Map.ofList
    |> ref

let mapLookup (db: Database) : ListLookup =
    fun u l ->
        let listsForUser = db.Value |> Map.find u
        let list = listsForUser |> Map.find l

        list

let mapWrite (db: Database) : ListWrite =
    fun u l ->
        let updatedLists = db.Value |> Map.find u |> Map.add l.Name l
        let updatedDb = db.Value |> Map.add u updatedLists
        db.Value <- updatedDb

let writeEvent (store: ListEvent list ref) : EventWriter =
    fun e -> store.Value <- e :: store.Value

[<EntryPoint>]
let main _ =
    Host
        .CreateDefaultBuilder()
        .ConfigureWebHostDefaults(
            ZettaiHost.configure (mapLookup db) (mapWrite db) (writeEvent store)
            >> ignore
        )
        .Build()
        .Run()

    0
