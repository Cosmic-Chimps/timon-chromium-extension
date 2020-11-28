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

    /// Transform a TokenResponse from JSON
    static member Decoder =
        Decode.object (fun get ->
            { accessToken = get.Required.Field "access_token" Decode.string
              expiresIn = get.Required.Field "expires_in" Decode.int
              refreshToken = get.Required.Field "refresh_token" Decode.string })

    /// Transform JSON as TokenResponse
    static member Encoder(json: TokenResponse) =
        Encode.object [ "access_token", Encode.string json.accessToken
                        "expires_in", Encode.int json.expiresIn
                        "refresh_token", Encode.string json.refreshToken ]

let tokenResponseCoder: ExtraCoders =
    Extra.empty
    |> Extra.withCustom TokenResponse.Encoder TokenResponse.Decoder

type TokenStorageTo =
    { token: TokenResponse
      username: string
      expirationDate: DateTime }

    static member Default =
        { token = TokenResponse.Default
          username = String.Empty
          expirationDate = DateTime.MinValue }

    /// Transform a TokenStorageTo from JSON
    static member Decoder =
        Decode.object (fun get ->
            { token = get.Required.Field "token" TokenResponse.Decoder
              username = get.Required.Field "username" Decode.string
              expirationDate = get.Required.Field "expirationDate" Decode.datetime })

    /// Transform JSON as TokenStorageTo
    static member Encoder(json: TokenStorageTo) =
        Encode.object [ "token", TokenResponse.Encoder json.token
                        "username", Encode.string json.username
                        "expirationDate", Encode.datetime json.expirationDate ]


module TokenLocalStorage =
    [<LiteralAttribute>]
    let private STORAGE_KEY = "TimonToken"

    let save (tokenResponse: TokenResponse) (username: string): unit =
        let expiresAt =
            DateTime.UtcNow.Add(TimeSpan.FromSeconds(float (tokenResponse.expiresIn)))

        let tokenStorageTo =
            { token = tokenResponse
              username = username
              expirationDate = expiresAt }

        TokenStorageTo.Encoder tokenStorageTo
        |> fun jsonValue -> Encode.Auto.toString (0, jsonValue)
        |> fun json -> (STORAGE_KEY, json)
        |> Browser.WebStorage.localStorage.setItem


    let clear () = Browser.WebStorage.localStorage.clear ()

    let load () =
        Browser.WebStorage.localStorage.getItem STORAGE_KEY
        |> unbox
        |> Decode.fromString (TokenStorageTo.Decoder)
    // |> Decode.fromString (Decode.Auto.generateDecoder<TokenStorageTo> ())

    let loadWithDefault () =
        let result = load ()
        match result with
        | Ok tokenStorageTo -> tokenStorageTo
        | _ -> TokenStorageTo.Default
