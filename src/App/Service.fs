module Service

open Models
open Thoth.Fetch
open Thoth.Json
open Fable.Core

let doLogin (loginForm: LoginForm) =
    promise {
        let url =
            (sprintf "%s/login" Configuration.config.timonEndPoint)

        let data =
            Encode.object [ "email", Encode.string loginForm.email
                            "password", Encode.string loginForm.password ]

        return! Fetch.tryPost (url, data, decoder = TokenResponse.Decoder)
    }
