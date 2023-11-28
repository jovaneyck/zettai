namespace Zettai.SlowTests

open Xunit
open Swensen.Unquote

module AcceptanceTests =
    [<Fact>]
    let ``hello world`` () = test <@ 3 = 1 + 3 @>
