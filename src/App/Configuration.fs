module Configuration

type Config = { timonEndPoint: string; env: string }

[<LiteralAttribute>]
let TIMON_ENDPOINT_KEY = "TIMON_ENDPOINT_KEY"

#if DEBUG
let config =
    { timonEndPoint = "http://localhost:5002"
      env = "Development" }
#else
let config =
    { timonEndPoint = "http://localhost:5002"
      env = "Production" }
#endif

let saveTimonEndpointUrl timonUrl =
    Browser.WebStorage.localStorage.setItem (TIMON_ENDPOINT_KEY, timonUrl)

let getTimonEndpointUrl =
    Browser.WebStorage.localStorage.getItem TIMON_ENDPOINT_KEY
