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

    let serialize (thing: 'a) =
        JsonSerializer.Serialize<'a>(thing, ZettaiHost.serializationOptions)

    let buildApp db =
        new Microsoft.AspNetCore.TestHost.TestServer(
            new WebHostBuilder()
            |> ZettaiHost.configure (Program.mapLookup db) (Program.mapWrite db)
        )

    let createClient (ts: TestServer) = ts.CreateClient()

    [<Fact>]
    let ``App bootstraps`` () =
        use client = Map.empty |> ref |> buildApp |> createClient

        let response = (client.GetAsync "/").Result

        test <@ response.IsSuccessStatusCode @>
        test <@ response.Content.ReadAsStringAsync().Result = "Hello world!" @>

    [<Fact>]
    let ``404 on unknown url's`` () =
        use client = Map.empty |> ref |> buildApp |> createClient

        let response = (client.GetAsync "/invalid-url").Result

        test <@ response.StatusCode = System.Net.HttpStatusCode.NotFound @>

    [<Fact>]
    let ``Renders lists for a user`` () =
        let petList =
            { Name = ListName "pets"
              Items = [ { Description = "nestor" } ] }

        let data = [ User "jo", [ petList ] ] |> Map.ofList |> ref

        use client = data |> buildApp |> createClient

        let response = (client.GetAsync "/todo/jo/pets").Result

        test <@ response.IsSuccessStatusCode @>
        let parsed = deserialize<ToDoList> (response.Content.ReadAsStream())
        test <@ parsed = petList @>

    [<Fact>]
    let ``Add item to existing list`` () =

        let data =
            [ User "jo", [ { Name = ListName "pets"; Items = [] } ] ]
            |> Map.ofList
            |> ref

        use client = data |> buildApp |> createClient

        let body = new System.Net.Http.StringContent(serialize { Description = "nestor" })

        let response = (client.PostAsync("/todo/jo/pets", body)).Result

        test <@ response.IsSuccessStatusCode @>

        let response = (client.GetAsync "/todo/jo/pets").Result

        test <@ response.IsSuccessStatusCode @>
        let parsed = deserialize<ToDoList> (response.Content.ReadAsStream())

        test
            <@ parsed = { Name = ListName "pets"
                          Items = [ { Description = "nestor" } ] } @>
