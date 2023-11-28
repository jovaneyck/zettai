namespace Zettai.SlowTests

open Xunit
open Swensen.Unquote
open Program

module AcceptanceTests =
    [<Fact>]
    let ``hello world`` () =
        let app = new Microsoft.AspNetCore.TestHost.TestServer(webBuilder ())
        let client = app.CreateClient()

        let response = (client.GetAsync "/").Result

        test <@ response.IsSuccessStatusCode @>
        test <@ response.Content.ReadAsStringAsync().Result = "Hello world!" @>
