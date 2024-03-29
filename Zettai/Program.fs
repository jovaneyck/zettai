module Program

open Microsoft.Extensions.Hosting
open Types

type Database = Map<User, ToDoList list> ref

let db: Database =
    [ User "jo",
      [ { Name = ListName "books"
          Items =
            [ { Description = "Tidy First?" }
              { Description = "Team Topologies" } ] } ] ]
    |> Map.ofList
    |> ref

let mapLookup (db: Database) : ListLookup =
    fun u l ->
        let listsForUser = db.Value |> Map.find u

        let list =
            listsForUser
            |> List.find (fun lfu -> lfu.Name = l)

        list

let mapWrite (db: Database) : ListWrite =
    fun l ->
        let updated =
            db.Value
            |> Map.map (fun _ ls ->
                ls
                |> List.map (fun ll -> if ll.Name = l.Name then l else ll))

        db.Value <- updated

[<EntryPoint>]
let main _ =
    Host
        .CreateDefaultBuilder()
        .ConfigureWebHostDefaults(
            ZettaiHost.configure (mapLookup db) (mapWrite db)
            >> ignore
        )
        .Build()
        .Run()

    0
