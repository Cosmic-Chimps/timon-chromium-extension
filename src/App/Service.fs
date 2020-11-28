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


let authorizationHeader (tokenStorageTo: TokenStorageTo) =
    promise {
        let! tokenResponse = getToken tokenStorageTo

        return sprintf "Bearer %s" tokenResponse.token.accessToken
    }

let postLinkToChannel (tokenStorageTo: TokenStorageTo, clubId: Guid, channelId: Guid, payload: CreateLinkPayload) =
    promise {
        let url =
            (sprintf "%s/clubs/%O/channels/%O/links" Configuration.config.timonEndPoint clubId channelId)

        let data =
            Encode.object [ "url", Encode.string payload.url
                            "via", Encode.string "chrome-extension"
                            "tagName", Encode.string "" ]

        let! authorizationHeader = authorizationHeader tokenStorageTo

        let! resp = Fetch.tryPost (url, data, headers = [ Fetch.Types.Authorization authorizationHeader ])

        return match resp with
               | Ok _ -> true
               | _ -> false
    }

let getChannels (tokenStorageTo: TokenStorageTo, clubId: Guid): JS.Promise<Result<Channel list, FetchError>> =
    promise {

        let url =
            (sprintf "%s/clubs/%O/channels" Configuration.config.timonEndPoint clubId)

        let! authorizationHeader = authorizationHeader tokenStorageTo

        return! Fetch.tryGet
                    (url,
                     decoder = (Decode.list Channel.Decoder),
                     headers = [ Fetch.Types.Authorization authorizationHeader ])
    }

let getClubs (tokenStorageTo: TokenStorageTo): JS.Promise<Result<Club list, FetchError>> =
    promise {
        let url =
            (sprintf "%s/clubs" Configuration.config.timonEndPoint)


        let! authorizationHeader = authorizationHeader tokenStorageTo

        return! Fetch.tryGet
                    (url,
                     decoder = (Decode.list Club.Decoder),
                     headers = [ Fetch.Types.Authorization authorizationHeader ])
    }
