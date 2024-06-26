﻿namespace Zettai.Tests

open Xunit
open Swensen.Unquote
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.TestHost
open System.Text.Json

open Infrastructure
open Domain

module AcceptanceTests =
    let deserialize<'a> (stream: System.IO.Stream) =
        JsonSerializer.Deserialize<'a>(stream, ZettaiHost.serializationOptions)

    let serialize (thing: 'a) =
        JsonSerializer.Serialize<'a>(thing, ZettaiHost.serializationOptions)

    let buildApp store =
        let store: Program.InMemoryEventStore = store |> Seq.ofList |> ref

        new Microsoft.AspNetCore.TestHost.TestServer(
            new WebHostBuilder()
            |> ZettaiHost.configure (Program.lookupByName store) (Program.writeEvent store)
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
        use client = buildClient []

        let response = client |> get "/"

        test <@ response.IsSuccessStatusCode @>
        test <@ response.Content.ReadAsStringAsync().Result = "Hello world!" @>

    [<Fact>]
    let ``404 on unknown url's`` () =
        use client = buildClient []

        let response = client |> get "/invalid-url"

        test <@ response.StatusCode = System.Net.HttpStatusCode.NotFound @>

    [<Fact>]
    let ``Renders lists for a user`` () =
        let listId = ListId.mint ()
        let listName = ListName.fromUntrusted "pets"

        let events =
            [ { AggregateId = listId
                Data =
                  ListCreated
                      { User = User "jo"
                        ListName = listName } }
              { AggregateId = listId
                Data = ItemAddedToList { Item = { Description = "nestor" } } } ]

        use client = buildClient events

        let parsed = client |> getJson<ToDoList> "/todo/jo/pets"

        let petList =
            { Id = listId
              Name = ListName.fromTrusted "pets"
              Items = [ { Description = "nestor" } ] }

        test <@ parsed = petList @>

    [<Fact>]
    let ``Add item to existing list`` () =
        let listId = ListId.mint ()

        use client =
            buildClient [ { AggregateId = listId
                            Data =
                              ListCreated
                                  { User = User "jo"
                                    ListName = ListName.fromTrusted "pets" } } ]

        let response =
            client
            |> postJson "/todo/jo/pets/item" { Description = "nestor" }

        test <@ response.IsSuccessStatusCode @>

        let response = client |> getJson<ToDoList> "/todo/jo/pets"

        test
            <@ response = { Id = listId
                            Name = ListName.fromTrusted "pets"
                            Items = [ { Description = "nestor" } ] } @>

    [<Fact>]
    let ``Add a new empty list`` () =
        use client = buildClient []

        let response = client |> post "/todo/jo/pets"

        test <@ response.StatusCode = System.Net.HttpStatusCode.Created @>

        let response = client |> getJson<ToDoList> "/todo/jo/pets"

        test <@ response.Name = ListName.fromTrusted "pets" @>
        test <@ response.Items = [] @>

    [<Fact>]
    let ``Rejects invalid list names`` () =
        use client = buildClient []

        let response =
            client
            |> post "/todo/jo/this-list-name-is-too-long-1234567890123456789012345678901234567890"

        test <@ response.StatusCode = System.Net.HttpStatusCode.BadRequest @>
        test <@ response.Content.ReadAsStringAsync().Result = "list name should not be more than 40 chars" @>
