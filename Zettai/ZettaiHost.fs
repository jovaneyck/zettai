module ZettaiHost

open Giraffe
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open System.Text.Json.Serialization
open Types

let showList (lookup: ListLookup) =
    fun (userName, listName) ->
        let list = lookup (User userName) (ListName.fromUntrusted listName)
        json list

let addList (write: ListWrite) =
    fun (userName, listName) ->
        let user = User userName

        try
            let name = ListName.fromUntrusted listName
            let newList = { Name = name; Items = [] }

            write user newList

            setStatusCode ((int) System.Net.HttpStatusCode.Created)
        with
        | ValidationException msg ->
            setStatusCode ((int) System.Net.HttpStatusCode.BadRequest)
            >=> text (sprintf "list name %s" msg)

let addItem (lookup: ListLookup) (write: ListWrite) =
    fun (userName, listName) (next: HttpFunc) (ctx: HttpContext) ->
        let newItem = ctx.BindJsonAsync<ToDoItem>().Result
        let user = User userName
        let list = lookup user (ListName.fromUntrusted listName)
        let updated = { list with Items = newItem :: list.Items }
        write user updated

        setStatusCode ((int) System.Net.HttpStatusCode.OK) next ctx

let webApp (lookup: ListLookup) (write: ListWrite) =
    choose [ GET >=> route "/" >=> text "Hello world!"
             GET >=> routef "/todo/%s/%s" (showList lookup)
             POST >=> routef "/todo/%s/%s" (addList write)
             POST
             >=> routef "/todo/%s/%s/item" (addItem lookup write)
             RequestErrors.NOT_FOUND "Not Found" ]

let configureApp (lookup: ListLookup) (write: ListWrite) (app: IApplicationBuilder) =
    app.UseGiraffe(webApp lookup write)

let serializationOptions =
    let o =
        JsonFSharpOptions
            .Default()
            .ToJsonSerializerOptions()

    o.PropertyNameCaseInsensitive <- true
    o

let configureServices (services: IServiceCollection) =
    services
        .AddGiraffe()
        .AddSingleton<Json.ISerializer>(SystemTextJson.Serializer(serializationOptions))
    |> ignore

let configure (lookup: ListLookup) (write: ListWrite) (whb: IWebHostBuilder) =
    whb
        .Configure((configureApp lookup write))
        .ConfigureServices(configureServices)
