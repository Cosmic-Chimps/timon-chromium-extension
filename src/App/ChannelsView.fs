module ChannelsView

open System
open Fable.React
open Fable.React.Props
open Fable.Import
open Elmish
open Fable.Core.JsInterop
open Validation
open Models
open Elmish.Cmd.OfPromise
open Service
open Thoth.Fetch


type Model =
    { channels: Channel list
      username: string
      addToChannelId: Guid }
    static member Default =
        { channels = List.empty
          username = String.Empty
          addToChannelId = Guid.Empty }

type Message =
    | LoadChannels
    | OnChannelsLoaded of Result<Channel list, FetchError>
    | AddLinkToChannel of Guid
    | OnUrlRead of string
    | LinkAdded of bool

let update (tokenResponse: TokenResponse) (model: Model) (msg: Message) =
    match msg with
    | LoadChannels ->
        let nextCmd =
            either getChannels () OnChannelsLoaded raise

        model, nextCmd
    | OnChannelsLoaded result ->
        match result with
        | Ok channels -> { model with channels = channels }, Cmd.none
        | _ -> model, Cmd.none
    | AddLinkToChannel channelId ->
        let nextCmd =
            Cmd.OfPromise.either JS.BrowserAction.getTabUrl () OnUrlRead raise

        { model with
              addToChannelId = channelId },
        nextCmd
    | OnUrlRead url ->
        printfn "%s - %O" url model.addToChannelId

        let payload =
            { url = url
              channelId = model.addToChannelId }

        let nextCmd =
            either postLinkToChannel (tokenResponse, payload) LinkAdded raise

        model, nextCmd
    | LinkAdded _ -> model, Cmd.none


let channelViews (model: Model) dispatch =
    match model.channels with
    | [] -> tbody [] []
    | value ->
        value
        |> List.map (fun channel ->
            let onClick id msg: DOMAttr = OnClick(fun _ -> dispatch (msg id))
            tr [] [
                td [] [
                    str (sprintf "#%s" channel.name)
                ]
                td [ Class "level-right media" ] [
                    a [ Class "button is-primary is-small"
                        onClick channel.id AddLinkToChannel ] [
                        str "Add"
                    ]

                    a [ Class "button is-info is-small ml-4" ] [
                        str "Open"
                    ]
                ]
            ])
        |> tbody []

let view (model: Model) (dispatch: Message -> unit) =
    div [] [
        nav [ Class "navbar is-primary"
              Role "navigation"
              AriaLabel "main navigation" ] [
            div [ Class "navbar-brand" ] [
                span [ Class "navbar-item has-text-weight-bold is-size-5" ] [
                    img [ Src "images/meerkat.svg"
                          Alt "logo" ]
                    // html ["&nbspl";"Timón"]
                    // str "&nbsp;Timón"
                    div [ DangerouslySetInnerHTML { __html = "&nbsp;Timón" } ] []
                ]
            ]
            div [ Class "navbar-end" ] [
                div [ Class "navbar-item" ] [
                    str model.username
                ]
            ]
        ]
        div [ Class "card" ] [
            header [ Class "card-header " ] [
                p [ Class "card-header-title" ] [
                    str "Channels"
                ]
            ]
            div [ Class "card-table" ] [
                div [ Class "content" ] [
                    table [ Class "table is-fullwidth is-striped" ] [
                        channelViews model dispatch
                    ]
                ]
            ]
        ]
    ]
