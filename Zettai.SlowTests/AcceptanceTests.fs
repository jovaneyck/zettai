namespace Zettai.SlowTests

open Xunit
open Swensen.Unquote
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.TestHost
open System.Text.Json

open Types

type Database = Map<User, ToDoList list>

module AcceptanceTests =
    let deserialize<'a> (stream: System.IO.Stream) =
        JsonSerializer.Deserialize<'a>(stream, ZettaiHost.serializationOptions)

    let lookupList db u l =
        db
        |> Map.find u
        |> List.find (fun ll -> ll.Name = l)

    let buildApp db =
        new Microsoft.AspNetCore.TestHost.TestServer(
            new WebHostBuilder()
            |> ZettaiHost.configure (lookupList db)
        )

    let createClient (ts: TestServer) = ts.CreateClient()

    [<Fact>]
    let ``App bootstraps`` () =
        use client = Map.empty |> buildApp |> createClient

        let response = (client.GetAsync "/").Result

        test <@ response.IsSuccessStatusCode @>
        test <@ response.Content.ReadAsStringAsync().Result = "Hello world!" @>

    [<Fact>]
    let ``404 on unknown url's`` () =
        use client = Map.empty |> buildApp |> createClient

        let response = (client.GetAsync "/invalid-url").Result

        test <@ response.StatusCode = System.Net.HttpStatusCode.NotFound @>

    [<Fact>]
    let ``Renders lists for a user`` () =
        let petList =
            { Name = ListName "pets"
              Items = [ { Description = "nestor" } ] }

        let data = [ User "jo", [ petList ] ] |> Map.ofList

        use client = data |> buildApp |> createClient

        let response = (client.GetAsync "/todo/jo/pets").Result

        test <@ response.IsSuccessStatusCode @>
        let parsed = deserialize<ToDoList> (response.Content.ReadAsStream())
        test <@ parsed = petList @>
