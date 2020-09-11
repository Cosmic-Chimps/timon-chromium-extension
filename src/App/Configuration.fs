module Configuration

type Config = { timonEndPoint: string; env: string }

#if DEBUG
let config =
    { timonEndPoint = "http://localhost:5002"
      env = "Development" }
#else
let config =
    { timonEndPoint = "http://localhost:5002"
      env = "Production" }
#endif
