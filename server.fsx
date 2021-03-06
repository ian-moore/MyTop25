#r "packages/FSharp.Configuration/lib/net40/FSharp.Configuration.dll"
#r "packages/Suave/lib/net40/Suave.dll"
#r "bin/MyTop25.dll"

open FSharp.Configuration
open MyTop25.Web
open Suave

type ServerConfig = YamlConfig<"serverconfig.yaml">

let config = ServerConfig ()

let settings =
  { ClientId = config.SpotifyClientId
    ClientSecret = config.SpotifyClientSecret
    RedirectUri = config.SpotifyRedirectUri.AbsoluteUri
    Scope = "user-top-read" 
    AzureConnection = config.AzureStorageConnectionString
    AzureTable = config.AzureStorageTableName }

let homeDir = System.IO.Path.Combine (__SOURCE_DIRECTORY__, "content")
let suaveConfig = { defaultConfig with homeFolder = Some homeDir }

app settings |> startWebServer suaveConfig