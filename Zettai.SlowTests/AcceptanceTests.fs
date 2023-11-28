namespace Zettai.SlowTests

open Xunit
open Swensen.Unquote
open Microsoft.AspNetCore.Hosting

module AcceptanceTests =
    let buildApp () = 
        new Microsoft.AspNetCore.TestHost.TestServer(
            new WebHostBuilder() 
            |> ZettaiHost.configure)
        
    [<Fact>]
    let ``App bootstraps`` () =
        use app = buildApp ()
        use client = app.CreateClient()

        let response = client.GetAsync "/" |> _.Result

        test <@ response.IsSuccessStatusCode @>
        test <@ response.Content.ReadAsStringAsync().Result = "Hello world!" @>
