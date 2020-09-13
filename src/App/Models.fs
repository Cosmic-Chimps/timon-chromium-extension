module Models

open Thoth.Json
open System

type LoginForm = { email: string; password: string }

type CreateLinkPayload = { url: string; channelId: Guid }

type Channel =
    { id: Guid
      name: string }
    static member Decoder =
        Decode.object (fun get ->
            { id = get.Required.Field "id" Decode.guid
              name = get.Required.Field "name" Decode.string })

type TokenResponse =
    { accessToken: string
      expiresIn: int
      refreshToken: string }

    static member Default =
        { accessToken = String.Empty
          expiresIn = 0
          refreshToken = String.Empty }

    /// Transform a Book from JSON
    static member Decoder =
        Decode.object (fun get ->
            { accessToken = get.Required.Field "access_token" Decode.string
              expiresIn = get.Required.Field "expires_in" Decode.int
              refreshToken = get.Required.Field "refresh_token" Decode.string })

    /// Transform JSON as Book
    static member Encoder(json: TokenResponse) =
        Encode.object [ "access_token", Encode.string json.accessToken
                        "expires_in", Encode.int json.expiresIn
                        "refresh_token", Encode.string json.refreshToken ]

let tokenResponseCoder: ExtraCoders =
    Extra.empty
    |> Extra.withCustom TokenResponse.Encoder TokenResponse.Decoder


module TokenLocalStorage =
    [<LiteralAttribute>]
    let private STORAGE_KEY = "TimonToken"

    let private STORAGE_USERNAME_KEY = "TimonUsername"

    let save (tokenResponse: TokenResponse) (username: string): unit =
        Encode.Auto.toString (0, tokenResponse)
        |> fun json -> (STORAGE_KEY, json)
        |> Browser.WebStorage.localStorage.setItem

        Browser.WebStorage.localStorage.setItem (STORAGE_USERNAME_KEY, username)

    let load () =
        let token =
            Browser.WebStorage.localStorage.getItem STORAGE_KEY
            |> unbox
            |> Decode.fromString (Decode.Auto.generateDecoder<TokenResponse> ())

        let username =
            Browser.WebStorage.localStorage.getItem STORAGE_USERNAME_KEY
            |> unbox
            |> string

        (token, username)

    let loadWithDefault () =
        let result = load ()
        match result with
        | Ok tokenResponse, username -> (tokenResponse, username)
        | a, b -> (TokenResponse.Default, String.Empty)
