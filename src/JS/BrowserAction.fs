namespace JS

open Fable.Core

type IBrowserAction =
    abstract setBadgeText: string -> unit
    abstract getTabUrl: unit -> JS.Promise<string>

module BrowserAction =

    [<Import("*", "./browserAction.js")>]
    let private native: IBrowserAction = jsNative

    let setBadgeText = native.setBadgeText
    let getTabUrl = native.getTabUrl
