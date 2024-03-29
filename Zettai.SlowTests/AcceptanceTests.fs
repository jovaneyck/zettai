namespace Zettai.SlowTests

open Xunit
open Swensen.Unquote
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection
open System.Text.Json
open System.Text.Json.Serialization

open Types

type Database = Map<User, ToDoList list>

module AcceptanceTests =
    let deserialize<'a> (stream: System.IO.Stream) =
        JsonSerializer.Deserialize<'a>(stream, ZettaiHost.serializationOptions)

    let configureTestServices (database: Database) (whb: IWebHostBuilder) =
        let lookupList: ListLookup =
            fun u l ->
                database
                |> Map.find u
                |> List.find (fun ll -> ll.Name = l)

        whb.ConfigureServices (fun (sc: IServiceCollection) ->
            sc.AddSingleton<Types.ListLookup>(lookupList)
            |> ignore)

    let buildApp database =
        new Microsoft.AspNetCore.TestHost.TestServer(
            new WebHostBuilder()
            |> ZettaiHost.configure
            |> configureTestServices database
        )

    [<Fact>]
    let ``App bootstraps`` () =
        use app = buildApp Map.empty
        use client = app.CreateClient()

        let response = (client.GetAsync "/").Result

        test <@ response.IsSuccessStatusCode @>
        test <@ response.Content.ReadAsStringAsync().Result = "Hello world!" @>

    [<Fact>]
    let ``Renders lists for a user`` () =
        let petList =
            { Name = ListName "pets"
              Items = [ { Description = "nestor" } ] }

        let data = [ User "jo", [ petList ] ] |> Map.ofList

        use app = buildApp data
        use client = app.CreateClient()

        let response = (client.GetAsync "/todo/jo/pets").Result

        test <@ response.IsSuccessStatusCode @>
        let parsed = deserialize<ToDoList> (response.Content.ReadAsStream())
        test <@ parsed = petList @>
