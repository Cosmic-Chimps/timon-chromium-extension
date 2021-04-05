module LoginView

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

type UserName = string

type Model =
    { loginForm: LoginForm
      validatedLogin: Result<LoginForm, FSharp.Collections.Map<string, string list>> option
      failureReason: string option }

type Model with
    static member Default =
        { loginForm =
              { timonUrl = String.Empty
                email = String.Empty
                password = String.Empty }
          validatedLogin = None
          failureReason = None }

type Message =
    | SetValue of string * string
    | OnLogin
    | LoginValidated
    | LoginSucceeded of Result<TokenResponse, FetchError>
    | LoginSaved of TokenStorageTo


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
        { timonUrl = cannotBeBlank t "timonUrl" loginForm.timonUrl
          email = validEmail t "email" loginForm.email
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
    | SetValue ("timonUrl", value) ->
        { model.loginForm with
              timonUrl = value }
        |> validate,
        Cmd.none
    | SetValue ("email", value) -> { model.loginForm with email = value } |> validate, Cmd.none
    | SetValue ("password", value) ->
        { model.loginForm with
              password = value }
        |> validate,
        Cmd.none
    | OnLogin -> model.loginForm |> validateForced, Cmd.ofMsg (LoginValidated)
    | LoginValidated -> model, either doLogin (model.loginForm) LoginSucceeded raise
    | LoginSucceeded result ->
        match result with
        | Ok tokenResponse ->
            TokenLocalStorage.save tokenResponse model.loginForm.email
            let tokenStorageTo = TokenLocalStorage.loadWithDefault ()
            model, Cmd.ofMsg (LoginSaved tokenStorageTo)
        | _ ->
            { model with
                  failureReason = Some "Verify your email or password" },
            Cmd.none
    | _ -> model, Cmd.none

let errorAndClass name (result: Result<_, FSharp.Collections.Map<_, _>> option) =
    match result with
    | Some (Error e) when (e.ContainsKey name && e.[name] <> []) -> Some(System.String.Join(",", e.[name])), "is-danger"
    | Some _ -> None, "modified valid"
    | _ -> None, ""

let onInput prop dispatch : DOMAttr =
    OnInput
        (fun e ->
            !!e.target?value
            |> (fun value -> SetValue(prop, value))
            |> dispatch)

let inputForm intputType name placeholder validationResults value dispatch =
    let error, _ = errorAndClass name validationResults

    div [ Class "field" ] [
        div [ Class "control" ] [
            input [ onInput name dispatch
                    Class "input is-large"
                    Type intputType
                    Placeholder placeholder
                    DefaultValue value ]
            match error with
            | Some value ->
                span [ Class "has-text-danger" ] [
                    str value
                ]
            | None -> ()
        ]
    ]

let view (model: Model) dispatch =
    let onClick msg : DOMAttr = OnClick(fun _ -> dispatch msg)

    div [ Class "container" ] [
        div [ Class "column is-8 is-offset-2" ] [
            div [] [
                h1 [ Class "title has-text-grey" ] [
                    str "Log in"
                ]

                match model.failureReason with
                | Some (value) ->
                    div [ Class "is-danger message" ] [
                        div [ Class "message-header" ] [
                            str value
                        ]
                    ]
                | None -> ()

                inputForm "text" "timonUrl" "Timon Url" model.validatedLogin model.loginForm.timonUrl dispatch
                inputForm "email" "email" "Email" model.validatedLogin model.loginForm.email dispatch
                inputForm "password" "password" "Password" model.validatedLogin model.loginForm.password dispatch
                button [ onClick OnLogin
                         Class "button is-block is-primary is-large is-fullwidth" ] [
                    str "Log in"
                ]
            ]
        ]
    ]
