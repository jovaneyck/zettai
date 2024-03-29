module Program

open Microsoft.Extensions.Hosting
open Types

let db =
    [ User "jo",
      [ { Name = ListName "books"
          Items =
            [ { Description = "Tidy First?" }
              { Description = "Team Topologies" } ] } ] ]
    |> Map.ofList

let mapLookup: ListLookup =
    fun u l ->
        let listsForUser = db |> Map.find u

        let list =
            listsForUser
            |> List.find (fun lfu -> lfu.Name = l)

        list


[<EntryPoint>]
let main _ =
    Host
        .CreateDefaultBuilder()
        .ConfigureWebHostDefaults(ZettaiHost.configure mapLookup >> ignore)
        .Build()
        .Run()

    0
