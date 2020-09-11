module Models

open Thoth.Json

type LoginForm = { email: string; password: string }

type TokenResponse =
    { accessToken: string
      expiresIn: int
      username: string
      refreshToken: string }

    /// Transform a Book from JSON
    static member Decoder =
        Decode.object (fun get ->
            { accessToken = get.Required.Field "access_token" Decode.string
              expiresIn = get.Required.Field "expires_in" Decode.int
              username = get.Required.Field "username" Decode.string
              refreshToken = get.Required.Field "refresh_token" Decode.string })

    /// Transform JSON as Book
    static member Encoder(json: TokenResponse) =
        Encode.object [ "access_token", Encode.string json.accessToken
                        "expires_in", Encode.int json.expiresIn
                        "username", Encode.string json.username
                        "refresh_token", Encode.string json.refreshToken ]

let tokenResponseCoder: ExtraCoders =
    Extra.empty
    |> Extra.withCustom TokenResponse.Encoder TokenResponse.Decoder
