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

type Model =
    { loginModel: Login.Model
      email: string }
    static member Default =
        { loginModel = Login.Model.Default
          email = String.Empty }

type Msg = LoginMsg of Login.Message

let init () =
    let model = Model.Default
    model, Cmd.none

let update (msg: Msg) (model: Model): Model * Cmd<Msg> =
    match msg with
    | LoginMsg msg ->
        let m, cmd = Login.update model.loginModel msg
        let model' = { model with loginModel = m }
        model', Cmd.map LoginMsg cmd

let view (model: Model) (dispatch: Msg -> unit): ReactElement =
    div [ Class "container" ] [
        div [ Class "column is-8 is-offset-2" ] [
            Login.view model.loginModel (LoginMsg >> dispatch)
        ]
    ]

Program.mkProgram init update view
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
