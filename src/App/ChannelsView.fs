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
open Browser.Types
open Fable.Core.JS


type Model =
    { channels: Channel list
      username: string
      addToChannelId: Guid
      showNotification: bool
      activeClub: Club option
      clubs: Club list
      isClubsDropdownOpen: bool }
    static member Default =
        { channels = List.empty
          username = String.Empty
          addToChannelId = Guid.Empty
          showNotification = false
          activeClub = None
          clubs = List.empty
          isClubsDropdownOpen = false }

type Message =
    | LoadChannels
    | OnChannelsLoaded of Result<Channel list, FetchError>
    | AddLinkToChannel of Guid
    | OnUrlRead of string
    | LinkAdded of bool
    | HideNotification
    | OnLogout
    | LoadClubs
    | OnClubsLoaded of Result<Club list, FetchError>
    | OpenClubDropdown
    | CloseClubDropdown of MouseEvent
    | SelectClub of Club

let update (tokenStorageTo: TokenStorageTo) (model: Model) (msg: Message) =
    match msg with
    | LoadChannels ->
        match model.activeClub with
        | None -> model, Cmd.none
        | Some c ->
            let nextCmd =
                either getChannels (tokenStorageTo, c.id) OnChannelsLoaded raise

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
        match model.activeClub with
        | None -> model, Cmd.none
        | Some c ->
            printfn "%s - %O" url model.addToChannelId

            let payload = { url = url }

            let nextCmd =
                either postLinkToChannel (tokenStorageTo, c.id, model.addToChannelId, payload) LinkAdded raise

            model, nextCmd
    | HideNotification -> { model with showNotification = false }, Cmd.none
    | LinkAdded _ -> { model with showNotification = true }, Cmd.none
    | OnLogout _ -> model, Cmd.none
    | LoadClubs _ ->
        let nextCmd =
            either getClubs (tokenStorageTo) OnClubsLoaded raise

        let clubFromStorage = Club.GetFromLocalStorage()

        let selectClubCmd =
            match clubFromStorage with
            | Ok club -> Cmd.ofMsg (SelectClub club)
            | _ -> Cmd.none

        model, Cmd.batch [| nextCmd; selectClubCmd |]
    | OnClubsLoaded result ->
        match result with
        | Ok clubs -> { model with clubs = clubs }, Cmd.none
        | _ -> model, Cmd.none
    | OpenClubDropdown _ ->
        { model with
              isClubsDropdownOpen = true },
        Cmd.none
    | CloseClubDropdown evt ->
        { model with
              isClubsDropdownOpen = false },
        Cmd.none
    | SelectClub club ->
        Club.SaveInLocalStorage club

        { model with
              activeClub = Some club
              isClubsDropdownOpen = false },
        Cmd.ofMsg LoadChannels

let clubView (clubs: Club list) dispatch =
    clubs
    |> List.map (fun club ->
        a [ Class "dropdown-item"
            OnClick(fun evt ->
                evt.stopPropagation ()
                dispatch (SelectClub club)) ] [
            str club.name
        ])
    |> div [ Class "has-text-weight-normal" ]


let channelViews (model: Model) dispatch =
    match model.channels with
    | [] -> div [] []
    | value ->
        value
        |> List.map (fun channel ->
            let onClick id msg: DOMAttr = OnClick(fun _ -> dispatch (msg id))
            a [ Class "panel-block is-active"
                OnClick(fun _ -> dispatch (AddLinkToChannel channel.id)) ] [
                span [ Class "panel-icon" ] [
                    i [ Class "mdi mdi-pound"
                        AriaHidden true ] []
                ]
                str (sprintf "%s" channel.name)
            ])
        |> div []

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
    let clubDropdownActiveClass =
        match model.isClubsDropdownOpen with
        | true -> "is-active"
        | false -> ""

    let publicClubs =
        model.clubs |> List.filter (fun c -> c.isPublic)

    let privateClubs =
        model.clubs
        |> List.filter (fun c -> not c.isPublic)

    div [ OnClick(fun evt -> dispatch (CloseClubDropdown evt)) ] [
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
                article [ Class "panel" ] [
                    div [ Class "panel-heading" ] [
                        div [ OnClick(fun evt ->
                                  evt.stopPropagation ()
                                  dispatch OpenClubDropdown)
                              //   OnClick(fun _ -> dispatch CloseClubDropdown)
                              Class(sprintf "dropdown %s" clubDropdownActiveClass) ] [
                            div [ Class "dropdown-trigger" ] [
                                button [ Class "button"
                                         AriaHasPopup true
                                         HTMLAttr.Custom("aria-controls", "dropdown-menu2") ] [
                                    span [] [ str "Clubs" ]
                                    span [ Class "icon is-small" ] [
                                        i [ Class "mdi mdi-chevron-down"
                                            AriaHidden true ] []
                                    ]
                                ]
                            ]
                            div [ Class "dropdown-menu"; Role "menu" ] [
                                div [ Class "dropdown-content" ] [
                                    div [ Class "dropdown-item" ] [
                                        p [] [ str "Public" ]
                                    ]
                                    clubView publicClubs dispatch
                                    hr [ Class "dropdown-divider" ]
                                    div [ Class "dropdown-item" ] [
                                        p [] [ str "Protected" ]
                                    ]
                                    clubView privateClubs dispatch
                                ]
                            ]
                        ]
                    ]
                    div [ Class "panel-block has-background-black has-text-weight-semibold has-text-white panel-block" ] [
                        p [ Class "control has-icons-left" ] [
                            match model.activeClub with
                            | None _ -> str ""
                            | Some c -> str (sprintf "%s's Channels" c.name)
                        ]
                    ]
                    channelViews model dispatch
                ]
            ]
        ]
    ]
