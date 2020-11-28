module Service

open Models
open Thoth.Fetch
open Thoth.Json
open Fable.Core
open System

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


let renewToken refreshToken =
    promise {
        let url =
            (sprintf "%s/refresh-token" Configuration.config.timonEndPoint)

        let data =
            Encode.object [ "refreshToken", refreshToken ]

        let! resp = Fetch.tryPost (url, data, decoder = TokenResponse.Decoder)

        return match resp with
               | Ok tokenResponse -> tokenResponse
               | _ -> TokenResponse.Default
    }

let saveNewToken email tokenResponse =
    TokenLocalStorage.save tokenResponse email
    TokenLocalStorage.loadWithDefault ()


let getToken (tokenStorageTo: TokenStorageTo) =
    promise {
        let expireAt = tokenStorageTo.expirationDate

        return! match expireAt < DateTime.UtcNow with
                | true ->
                    promise {
                        let! tokenResponse = renewToken tokenStorageTo.token.refreshToken
                        return saveNewToken tokenStorageTo.username tokenResponse
                    }
                | false -> promise { return tokenStorageTo }
    }


let postLinkToChannel (tokenStorageTo: TokenStorageTo, payload: CreateLinkPayload) =
    promise {
        let url =
            (sprintf "%s/links" Configuration.config.timonEndPoint)

        let data =
            Encode.object [ "url", Encode.string payload.url
                            "channelId", Encode.guid payload.channelId
                            "via", Encode.string "chrome-extension"
                            "tagName", Encode.string "" ]



        let! tokenResponse = getToken tokenStorageTo

        let authorizationHeader =
            sprintf "Bearer %s" tokenResponse.token.accessToken

        let! resp = Fetch.tryPost (url, data, headers = [ Fetch.Types.Authorization authorizationHeader ])

        return match resp with
               | Ok _ -> true
               | _ -> false
    }
