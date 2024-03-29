module ZettaiHost

open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection
open Giraffe
open Types
open Microsoft.AspNetCore.Http
open System.Text.Json.Serialization

let helloWorld: HttpHandler = text "Hello world!"

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

let showList =
    fun (userName, listName) (next: HttpFunc) (ctx: HttpContext) ->
        let lookup = ctx.GetService<ListLookup>()
        let list = lookup (User userName) (ListName listName)

        json list next ctx

let webApp =
    choose [ GET >=> route "/" >=> helloWorld
             GET >=> routef "/todo/%s/%s" showList
             RequestErrors.NOT_FOUND "Not Found" ]

let configureApp (app: IApplicationBuilder) = app.UseGiraffe webApp

let serializationOptions =
    JsonFSharpOptions
        .Default()
        .ToJsonSerializerOptions()

let configureServices (services: IServiceCollection) =
    services
        .AddGiraffe()
        .AddSingleton<Json.ISerializer>(SystemTextJson.Serializer(serializationOptions))
        .AddSingleton<ListLookup>(mapLookup)
    |> ignore

let configure (whb: IWebHostBuilder) =
    whb
        .Configure(configureApp)
        .ConfigureServices(configureServices)
