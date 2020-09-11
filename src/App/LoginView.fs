module Login

open System
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
open Validation

type LoginForm = { email: string; password: string }

type Model =
    { loginForm: LoginForm
      validatedLogin: Result<LoginForm, FSharp.Collections.Map<string, string list>> option }

type Model with
    static member Default =
        { loginForm =
              { email = String.Empty
                password = String.Empty }
          validatedLogin = None }

type Message =
    | SetValue of string * string
    | OnLogin
    | LoginValidated


let validateForm (loginForm: LoginForm) =
    let cannotBeBlank (validator: Validator<string>) name value =
        validator.Test name value
        |> validator.NotBlank(name + " cannot be blank")
        |> validator.End

    let validEmail (validator: Validator<string>) name value =
        validator.Test name value
        |> validator.NotBlank(name + " cannot be blank")
        |> validator.IsMail(name + " should be an email format")
        |> validator.End

    all
    <| fun t ->
        { email = validEmail t "email" loginForm.email
          password = cannotBeBlank t "password" loginForm.password }


let update model msg =
    let validateForced form =
        let validated = validateForm form
        { model with
              loginForm = form
              validatedLogin = Some validated }

    let validate form =
        match model.validatedLogin with
        | None -> { model with loginForm = form }
        | Some _ -> validateForced form

    match msg with
    | SetValue ("email", value) -> { model.loginForm with email = value } |> validate, Cmd.none
    | SetValue ("password", value) ->
        { model.loginForm with
              password = value }
        |> validate,
        Cmd.none
    | OnLogin -> model.loginForm |> validateForced, Cmd.ofMsg (LoginValidated)
    | LoginValidated ->

        model, Cmd.none
    | _ -> model, Cmd.none

let errorAndClass name (result: Result<_, FSharp.Collections.Map<_, _>> option) =
    match result with
    | Some (Error e) when (e.ContainsKey name && e.[name] <> []) -> Some(System.String.Join(",", e.[name])), "is-danger"
    | Some _ -> None, "modified valid"
    | _ -> None, ""

let onInput prop dispatch: DOMAttr =
    OnInput(fun e ->
        !!e.target?value
        |> (fun value -> SetValue(prop, value))
        |> dispatch)

let inputForm intputType name validationResults dispatch =
    let error, _ = errorAndClass name validationResults
    div [ Class "field" ] [
        div [ Class "control" ] [
            input [ onInput name dispatch
                    Class "input is-large"
                    Type intputType
                    Placeholder name ]
            match error with
            | Some value ->
                span [ Class "has-text-danger" ] [
                    str value
                ]
            | None -> ()
        ]
    ]

let view (model: Model) dispatch =
    let onClick msg: DOMAttr = OnClick(fun _ -> dispatch msg)

    div [] [
        h1 [ Class "title has-text-grey" ] [
            str "Log in"
        ]

        inputForm "email" "email" model.validatedLogin dispatch
        inputForm "password" "password" model.validatedLogin dispatch
        button [ onClick OnLogin
                 Class "button is-block is-primary is-large is-fullwidth" ] [
            str "Log in"
        ]
    ]
