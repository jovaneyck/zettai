module ZettaiHost

open Giraffe
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open System.Text.Json.Serialization
open Domain

let showList (lookup: ListLookup) =
    fun (userName, listName) ->
        let list = lookup (User userName) (ListName.fromUntrusted listName)
        json list

let addList handler =
    fun (userName, listName) ->
        try
            let user = User userName
            let name = ListName.fromUntrusted listName

            let cmd = CreateList { User = user; List = name }
            handler cmd

            setStatusCode ((int) System.Net.HttpStatusCode.Created)
        with
        | ValidationException msg ->
            setStatusCode ((int) System.Net.HttpStatusCode.BadRequest)
            >=> text (sprintf "list name %s" msg)

let addItem handler =
    fun (userName, listName) (next: HttpFunc) (ctx: HttpContext) ->
        let newItem = ctx.BindJsonAsync<ItemData>().Result
        let user = User userName
        let listName = ListName.fromUntrusted listName

        let cmd =
            AddItemToList
                { User = user
                  List = listName
                  Item = newItem }

        handler cmd
        next ctx

let webApp lookup writeEvent =
    let handler cmd =
        (handleCommand lookup) cmd |> List.iter writeEvent

    choose [ GET >=> route "/" >=> text "Hello world!"
             GET >=> routef "/todo/%s/%s" (showList lookup)
             POST >=> routef "/todo/%s/%s" (addList handler)
             POST
             >=> routef "/todo/%s/%s/item" (addItem handler)
             RequestErrors.NOT_FOUND "Not Found" ]

let configureApp lookup eventWriter (app: IApplicationBuilder) =
    app.UseGiraffe(webApp lookup eventWriter)

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

let configure lookup eventWriter (whb: IWebHostBuilder) =
    whb
        .Configure((configureApp lookup eventWriter))
        .ConfigureServices(configureServices)
