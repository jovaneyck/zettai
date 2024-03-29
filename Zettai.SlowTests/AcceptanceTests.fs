namespace Zettai.SlowTests

open Xunit
open Swensen.Unquote
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.TestHost
open System.Text.Json

open Types

module AcceptanceTests =
    let deserialize<'a> (stream: System.IO.Stream) =
        JsonSerializer.Deserialize<'a>(stream, ZettaiHost.serializationOptions)

    let serialize (thing: 'a) =
        JsonSerializer.Serialize<'a>(thing, ZettaiHost.serializationOptions)

    let buildApp db =
        let rdb = db |> Map.ofList |> ref

        new Microsoft.AspNetCore.TestHost.TestServer(
            new WebHostBuilder()
            |> ZettaiHost.configure (Program.mapLookup rdb) (Program.mapWrite rdb)
        )

    let createClient (ts: TestServer) = ts.CreateClient()

    let get (path: string) (client: System.Net.Http.HttpClient) =
        let response = (client.GetAsync path).Result
        response

    let getJson<'a> (path: string) (client: System.Net.Http.HttpClient) =
        let response = (client.GetAsync path).Result
        let parsed = deserialize<'a> (response.Content.ReadAsStream())
        parsed

    let post (path: string) (payload: 'a) (client: System.Net.Http.HttpClient) =
        let body = new System.Net.Http.StringContent(serialize payload)
        let response = (client.PostAsync(path, body)).Result
        response

    [<Fact>]
    let ``App bootstraps`` () =
        use client = [] |> buildApp |> createClient

        let response = client |> get "/"

        test <@ response.IsSuccessStatusCode @>
        test <@ response.Content.ReadAsStringAsync().Result = "Hello world!" @>

    [<Fact>]
    let ``404 on unknown url's`` () =
        use client = [] |> buildApp |> createClient

        let response = client |> get "/invalid-url"

        test <@ response.StatusCode = System.Net.HttpStatusCode.NotFound @>

    [<Fact>]
    let ``Renders lists for a user`` () =
        let petList =
            { Name = ListName "pets"
              Items = [ { Description = "nestor" } ] }

        use client =
            [ User "jo", [ petList ] ]
            |> buildApp
            |> createClient

        let parsed = client |> getJson<ToDoList> "/todo/jo/pets"

        test <@ parsed = petList @>

    [<Fact>]
    let ``Add item to existing list`` () =
        use client =
            [ User "jo", [ { Name = ListName "pets"; Items = [] } ] ]
            |> buildApp
            |> createClient

        let response =
            client
            |> post "/todo/jo/pets" { Description = "nestor" }

        test <@ response.IsSuccessStatusCode @>

        let response = client |> getJson<ToDoList> "/todo/jo/pets"

        test
            <@ response = { Name = ListName "pets"
                            Items = [ { Description = "nestor" } ] } @>
