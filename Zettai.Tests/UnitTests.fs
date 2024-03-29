namespace Zettai.Tests

open Xunit
open Swensen.Unquote


open Types

module UnitTests =
    module ListName =
        [<Fact>]
        let ``Accepts valid list names`` () =
            test <@ ListName.fromTrusted "name" = ListName.fromUntrusted "name" @>

        [<Fact>]
        let ``Rejects invalid list names`` () =
            raises<ValidationException> <@ ListName.fromUntrusted "too-long-1234567890123456789012345678901234567890" @>
