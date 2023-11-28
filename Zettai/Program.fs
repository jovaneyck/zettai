open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.DependencyInjection
open Giraffe

let helloWorld: HttpHandler = text "Hello world!"

let showList (user, list) =
    text $"Here is the list {list} for user {user}."

let webApp =
    choose [ GET >=> route "/" >=> helloWorld
             GET >=> (routef "/todo/%s/%s" showList) ]

let configureApp (app: IApplicationBuilder) = app.UseGiraffe webApp

let configureServices (services: IServiceCollection) = services.AddGiraffe() |> ignore

[<EntryPoint>]
let main _ =
    Host
        .CreateDefaultBuilder()
        .ConfigureWebHostDefaults(fun whb ->
            whb
                .Configure(configureApp)
                .ConfigureServices(configureServices)
            |> ignore)
        .Build()
        .Run()

    0
