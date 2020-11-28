namespace JS

open Fable.Core

type IJwtDecode =
    abstract decode: string -> string

module JwtDecode =

    // [<Import("jwt_decode", from = "jwt-decode")>]
    // let private native: IJwtDecode = jsNative

    // let decode = native.decode

    // [<Import("jwt_decode", from = "jwt-decode")>]
    // let private jwt_decode (value: string): string = jsNative
    // let decode = jwt_decode

    [<Import("decode", from = "./jwtDecode.js")>]
    let decode (value: string): obj = jsNative

// [<ImportMember("jwt-decode")>]
// let jwt_decode (value: string): string = jsNative

// let private imported = import<IJSCookie> "*" "jwt-simple"
