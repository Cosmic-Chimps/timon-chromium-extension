module App

open Fable.Core
open Fable.React
open Fable.React.Props
open Fable.Import
open Browser.Dom
open Elmish
open Elmish.React
open Shared
open Fable.Core.JsInterop
open Fable.Core.JS
open System
open Models

type Model =
    { loginModel: LoginView.Model
      channelsListModel: ChannelsView.Model
      email: string
      tokenResponse: TokenResponse
      isLoggedIn: bool }
    static member Default =
        { loginModel = LoginView.Model.Default
          email = String.Empty
          tokenResponse = TokenResponse.Default
          channelsListModel = ChannelsView.Model.Default
          isLoggedIn = false }

type Msg =
    | LoginMsg of LoginView.Message
    | ChannelsMsg of ChannelsView.Message
    | Init

let init (tokenResponse, username) =
    let model = Model.Default

    let channelModel =
        { model.channelsListModel with
              username = username }

    { model with
          email = username
          tokenResponse = tokenResponse
          channelsListModel = channelModel },
    Cmd.ofMsg Init

let update (msg: Msg) (model: Model): Model * Cmd<Msg> =
    match msg with
    | Init ->
        match model.tokenResponse.refreshToken <> String.Empty with
        | false -> model, Cmd.none
        | true ->
            let nextCmd =
                Cmd.ofMsg ChannelsView.Message.LoadChannels

            { model with isLoggedIn = true }, Cmd.map ChannelsMsg nextCmd

    | ChannelsMsg msg ->
        let m, cmd =
            ChannelsView.update model.tokenResponse model.channelsListModel msg

        { model with channelsListModel = m }, Cmd.map ChannelsMsg cmd

    | LoginMsg (LoginView.Message.LoginSaved (tokenResponse, username)) ->
        let channelModel =
            { model.channelsListModel with
                  username = username }

        { model with
              isLoggedIn = true
              tokenResponse = tokenResponse
              email = username
              channelsListModel = channelModel },
        Cmd.none
    | LoginMsg msg ->
        let m, cmd = LoginView.update model.loginModel msg
        let model' = { model with loginModel = m }
        model', Cmd.map LoginMsg cmd


let view (model: Model) (dispatch: Msg -> unit): ReactElement =
    div [ Class "container" ] [
        div [ Class "column is-8 is-offset-2" ] [
            match model.isLoggedIn with
            | false -> LoginView.view model.loginModel (LoginMsg >> dispatch)
            | true -> ChannelsView.view model.channelsListModel (ChannelsMsg >> dispatch)
        ]
    ]

Program.mkProgram (TokenLocalStorage.loadWithDefault >> init) update view
|> Program.withReactBatched "app"
|> Program.run

// Program.mkProgram (Counter.loadWithDefault >> init) update view
// |> Program.withReactBatched "options"
// |> Program.run

// let init (defaultCounter: Counter): Model * Cmd<Msg> =
//     let model =
//         { Value = String.Empty
//           DefaultCounter = defaultCounter }

//     let cmd = []
//     model, cmd
