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


let getChannels (): JS.Promise<Result<Channel list, FetchError>> =
    promise {
        let url =
            (sprintf "%s/channels" Configuration.config.timonEndPoint)

        return! Fetch.tryGet (url, decoder = (Decode.list Channel.Decoder))
    }


// let renewToken endpoint refreshTokenRequest =
//     httpAsync
//         {
//             POST (sprintf "%s/refresh-token" endpoint)
//             body
//             json (Json.serialize refreshTokenRequest)
//         }
//         |> Async.RunSynchronously
//         |> toText
//         |> TokenProvider.Parse

// let getToken (tokenResponse: TokenResponse) endpoint = async {
//     let expireAt = ctx.HttpContext.User.Claims
//                     |> Seq.find (fun c ->  c.Type = "TimonExpiredDate" )
//                     |> fun c -> DateTime.Parse(c.Value)

//     let timonRefreshToken = ctx.HttpContext.User.Claims
//                             |> Seq.find (fun c ->  c.Type = "TimonRefreshToken" )
//                             |> fun c -> c.Value
//                             |> protector.Unprotect

//     let timonToken = ctx.HttpContext.User.Claims
//                     |> Seq.find (fun c ->  c.Type = "TimonToken" )
//                     |> fun c -> c.Value

//     return! match expireAt < DateTime.UtcNow with
//              | true ->
//                 renewToken endpoint { refreshToken = timonRefreshToken }
//                 |>  singInUser ctx protector ctx.HttpContext.User.Identity.Name
//              | false -> async { return timonToken }
// }


let postLinkToChannel (tokenResponse: TokenResponse, payload: CreateLinkPayload) =
    promise {
        let url =
            (sprintf "%s/links" Configuration.config.timonEndPoint)

        let data =
            Encode.object [ "url", Encode.string payload.url
                            "channelId", Encode.guid payload.channelId
                            "via", Encode.string "chrome-extension"
                            "tagName", Encode.string "" ]

        let authorizationHeader =
            sprintf "Bearer %s" tokenResponse.accessToken

        let! resp = Fetch.tryPost (url, data, headers = [ Fetch.Types.Authorization authorizationHeader ])

        return match resp with
               | Ok _ -> true
               | _ -> false
    }
