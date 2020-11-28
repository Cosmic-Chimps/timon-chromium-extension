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
      addToChannelId: Guid
      showNotification: bool }
    static member Default =
        { channels = List.empty
          username = String.Empty
          addToChannelId = Guid.Empty
          showNotification = false }

type Message =
    | LoadChannels
    | OnChannelsLoaded of Result<Channel list, FetchError>
    | AddLinkToChannel of Guid
    | OnUrlRead of string
    | LinkAdded of bool
    | HideNotification
    | OnLogout

let update (tokenStorageTo: TokenStorageTo) (model: Model) (msg: Message) =
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
            either postLinkToChannel (tokenStorageTo, payload) LinkAdded raise

        model, nextCmd
    | HideNotification -> { model with showNotification = false }, Cmd.none
    | LinkAdded _ -> { model with showNotification = true }, Cmd.none
    | OnLogout _ -> model, Cmd.none


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

let notificationView model dispatch =
    match model.showNotification with
    | false -> div [] []
    | true ->
        div [ Class "notification is-info" ] [
            button [ Class "delete"
                     OnClick(fun _ -> dispatch HideNotification) ] []
            str "The url has been saved"
        ]


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
        ]

        div [ Class "notification is-primary" ] [
            div [] [ str model.username ]
            div [] [
                a [ OnClick(fun _ -> dispatch OnLogout) ] [
                    span [ Class "icon is-small" ] [
                        i [ Class "mdi mdi-logout" ] []
                    ]
                    span [ DangerouslySetInnerHTML { __html = "&nbsp;Logout" } ] []
                ]
            ]
        ]

        div [ Class "container" ] [
            div [ Class "column is-8 is-offset-2" ] [
                notificationView model dispatch
                div [ Class "card" ] [
                    header [ Class "card-header" ] [
                        p [ Class "card-header-title" ] [
                            div [ Class "dropdown" ] [
                                div [ Class "dropdown-trigger" ] [
                                    button [ Class "button"
                                             AriaHasPopup true
                                             HTMLAttr.Custom("aria-controls", "dropdown-menu2") ] [
                                        span [] [ str "Content" ]
                                        span [ Class "icon is-small" ] [
                                            i [ Class "fas fa-angle-down"
                                                HTMLAttr.Custom("aria-hidden", "true") ] []
                                        ]
                                    ]
                                ]
                                div [ Class "dropdown-menu"
                                      Id "dropdown-menu2"
                                      Role "menu" ] [
                                    div [ Class "dropdown-content" ] [
                                        div [ Class "dropdown-item" ] [
                                            p [] [ str "You can insert" ]
                                        ]
                                        hr [ Class "dropdown-divider" ]
                                        div [ Class "dropdown-item" ] [
                                            p [] [ str "You simply need to use a" ]
                                        ]
                                        hr [ Class "dropdown-divider" ]
                                        a [ Href "#"; Class "dropdown-item" ] [
                                            str "This is a link"
                                        ]
                                    ]
                                ]
                            ]
                        ]
                    ]
                    div [ Class "card-content" ] [
                        div [ Class "content" ] [
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
                    ]
                ]
            ]
        ]
    ]
