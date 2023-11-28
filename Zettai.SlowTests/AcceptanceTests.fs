namespace Zettai.SlowTests

open Xunit
open Swensen.Unquote
open Microsoft.AspNetCore.Hosting

module AcceptanceTests =
    let buildClient () = 
        let app = new Microsoft.AspNetCore.TestHost.TestServer(
            new WebHostBuilder() 
            |> ZettaiHost.configure)
        let client = app.CreateClient()
        client

    [<Fact>]
    let ``App bootstraps`` () =
        let client = buildClient ()

        let response = client.GetAsync "/" |> _.Result

        test <@ response.IsSuccessStatusCode @>
        test <@ response.Content.ReadAsStringAsync().Result = "Hello world!" @>
