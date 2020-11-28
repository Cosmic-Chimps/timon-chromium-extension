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
      tokenStorageTo: TokenStorageTo
      isLoggedIn: bool }
    static member Default =
        { loginModel = LoginView.Model.Default
          email = String.Empty
          tokenStorageTo = TokenStorageTo.Default
          channelsListModel = ChannelsView.Model.Default
          isLoggedIn = false }

type Msg =
    | LoginMsg of LoginView.Message
    | ChannelsMsg of ChannelsView.Message
    | Init

let init (tokenStorageTo: TokenStorageTo) =
    let model = Model.Default

    let channelModel =
        { model.channelsListModel with
              username = tokenStorageTo.username }

    { model with
          email = tokenStorageTo.username
          tokenStorageTo = tokenStorageTo
          channelsListModel = channelModel },

    Cmd.ofMsg Init

let update (msg: Msg) (model: Model): Model * Cmd<Msg> =
    match msg with
    | Init ->
        match model.tokenStorageTo.token.refreshToken = String.Empty with
        | true -> model, Cmd.none
        | false ->
            let nextCmd = Cmd.ofMsg ChannelsView.Message.LoadClubs

            { model with isLoggedIn = true }, Cmd.map ChannelsMsg nextCmd

    | ChannelsMsg (ChannelsView.Message.OnLogout _) ->
        TokenLocalStorage.clear ()
        TokenLocalStorage.loadWithDefault () |> init

    | ChannelsMsg msg ->
        let m, cmd =
            ChannelsView.update model.tokenStorageTo model.channelsListModel msg

        { model with channelsListModel = m }, Cmd.map ChannelsMsg cmd

    | LoginMsg (LoginView.Message.LoginSaved tokenStorageTo) ->
        let channelModel =
            { model.channelsListModel with
                  username = tokenStorageTo.username }

        let nextCmd = Cmd.ofMsg ChannelsView.Message.LoadClubs

        { model with
              isLoggedIn = true
              tokenStorageTo = tokenStorageTo
              email = tokenStorageTo.username
              channelsListModel = channelModel },
        Cmd.map ChannelsMsg nextCmd
    | LoginMsg msg ->
        let m, cmd = LoginView.update model.loginModel msg
        let model' = { model with loginModel = m }
        model', Cmd.map LoginMsg cmd


let view (model: Model) (dispatch: Msg -> unit): ReactElement =
    match model.isLoggedIn with
    | false -> LoginView.view model.loginModel (LoginMsg >> dispatch)
    | true -> ChannelsView.view model.channelsListModel (ChannelsMsg >> dispatch)

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
