namespace Zettai.Tests

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
        let rdb =
            db
            |> Map.ofList
            |> Map.map (fun _ v -> v |> List.map (fun l -> l.Name, l) |> Map.ofList)
            |> ref

        new Microsoft.AspNetCore.TestHost.TestServer(
            new WebHostBuilder()
            |> ZettaiHost.configure (Program.mapLookup rdb) (Program.mapWrite rdb)
        )

    let createClient (ts: TestServer) = ts.CreateClient()
    let buildClient = buildApp >> createClient

    let get (path: string) (client: System.Net.Http.HttpClient) =
        let response = (client.GetAsync path).Result
        response

    let getJson<'a> (path: string) (client: System.Net.Http.HttpClient) =
        let response = (client.GetAsync path).Result
        let parsed = deserialize<'a> (response.Content.ReadAsStream())
        parsed

    let post (path: string) (client: System.Net.Http.HttpClient) =
        let body = new System.Net.Http.StringContent("{}")
        let response = (client.PostAsync(path, body)).Result
        response

    let postJson (path: string) (payload: 'a) (client: System.Net.Http.HttpClient) =
        let body = new System.Net.Http.StringContent(serialize payload)
        let response = (client.PostAsync(path, body)).Result
        response

    [<Fact>]
    let ``App bootstraps`` () =
        use client = [] |> buildClient

        let response = client |> get "/"

        test <@ response.IsSuccessStatusCode @>
        test <@ response.Content.ReadAsStringAsync().Result = "Hello world!" @>

    [<Fact>]
    let ``404 on unknown url's`` () =
        use client = [] |> buildClient

        let response = client |> get "/invalid-url"

        test <@ response.StatusCode = System.Net.HttpStatusCode.NotFound @>

    [<Fact>]
    let ``Renders lists for a user`` () =
        let petList =
            { Name = ListName.fromTrusted "pets"
              Items = [ { Description = "nestor" } ] }

        use client = [ User "jo", [ petList ] ] |> buildClient

        let parsed = client |> getJson<ToDoList> "/todo/jo/pets"

        test <@ parsed = petList @>

    [<Fact>]
    let ``Add item to existing list`` () =
        use client =
            [ User "jo",
              [ { Name = ListName.fromTrusted "pets"
                  Items = [] } ] ]
            |> buildClient

        let response =
            client
            |> postJson "/todo/jo/pets/item" { Description = "nestor" }

        test <@ response.IsSuccessStatusCode @>

        let response = client |> getJson<ToDoList> "/todo/jo/pets"

        test
            <@ response = { Name = ListName.fromTrusted "pets"
                            Items = [ { Description = "nestor" } ] } @>

    [<Fact>]
    let ``Add a new empty list`` () =
        use client = [ User "jo", [] ] |> buildClient

        let response = client |> post "/todo/jo/pets"

        test <@ response.StatusCode = System.Net.HttpStatusCode.Created @>

        let response = client |> getJson<ToDoList> "/todo/jo/pets"

        test
            <@ response = { Name = ListName.fromTrusted "pets"
                            Items = [] } @>


    [<Fact>]
    let ``Rejects invalid list names`` () =
        use client = [ User "jo", [] ] |> buildClient

        let response =
            client
            |> post "/todo/jo/this-list-name-is-too-long-1234567890123456789012345678901234567890"

        test <@ response.StatusCode = System.Net.HttpStatusCode.BadRequest @>
        test <@ response.Content.ReadAsStringAsync().Result = "list name should not be more than 40 chars" @>
