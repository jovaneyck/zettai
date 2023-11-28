module Program

open Microsoft.Extensions.Hosting

[<EntryPoint>]
let main _ =
    Host
        .CreateDefaultBuilder()
        .ConfigureWebHostDefaults(ZettaiHost.configure >> ignore)
        .Build()
        .Run()

    0
