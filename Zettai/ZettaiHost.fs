module ZettaiHost

open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection
open Giraffe
open Types
open Microsoft.AspNetCore.Http
open System.Text.Json.Serialization

let showList (lookup: ListLookup) =
    fun (userName, listName) ->
        let list = lookup (User userName) (ListName listName)
        json list

let webApp (lookup: ListLookup) =
    choose [ GET >=> route "/" >=> text "Hello world!"
             GET >=> routef "/todo/%s/%s" (showList lookup)
             RequestErrors.NOT_FOUND "Not Found" ]

let configureApp (lookup: ListLookup) (app: IApplicationBuilder) = app.UseGiraffe(webApp lookup)

let serializationOptions =
    JsonFSharpOptions
        .Default()
        .ToJsonSerializerOptions()

let configureServices (services: IServiceCollection) =
    services
        .AddGiraffe()
        .AddSingleton<Json.ISerializer>(SystemTextJson.Serializer(serializationOptions))
    |> ignore

let configure (lookup: ListLookup) (whb: IWebHostBuilder) =
    whb
        .Configure((configureApp lookup))
        .ConfigureServices(configureServices)
