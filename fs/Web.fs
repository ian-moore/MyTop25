module MyTop25.Web

open FSharp.Data
open MyTop25.Storage
open Suave
open Suave.Cookie
open Suave.State.CookieStateStore
open Suave.Filters
open Suave.Operators

type MyTop25Settings = 
  { ClientId: string
    ClientSecret: string
    RedirectUri: string
    Scope: string 
    AzureConnection: string
    AzureTable: string }

let httpPost url requestBody = 
    Http.RequestString (url, body = (FormValues requestBody))

let httpGet url requestHeaders =
    Http.RequestString (url, headers = requestHeaders)

let verifyAuth protectedPart = 
    Authentication.authenticate Cookie.CookieLife.Session false
        (fun () -> Choice2Of2 <| RequestErrors.FORBIDDEN "Not recognized.")
        (fun _ ->  Choice2Of2 <| RequestErrors.FORBIDDEN "Authentication is invalid.")
        protectedPart

let clearAuth =
    unsetPair Authentication.SessionAuthCookie >=> unsetPair StateCookie

let processAuth redirectUri clientId clientSecret code =
    let headers = Spotify.makeTokenHeaders code redirectUri clientId clientSecret
    let tokens = httpPost |> Spotify.requestTokens headers
    let user = httpGet |> Spotify.requestUser tokens.AccessToken
    user, tokens

let storeAuthTokens (storage: StorageClient) ((user: Spotify.SpotifyUser.Root), (tokens: Spotify.OAuthTokens.Root)) =
    { azureUser with 
        UserId = user.Id 
        AccessToken = tokens.AccessToken 
        RefreshToken = tokens.RefreshToken 
        TokenValidTo = float tokens.ExpiresIn |> System.DateTime.UtcNow.AddSeconds }
    |> storage.insertOrReplace 
    |> ignore
    user.Id

let copyIdToStore ctx id =
    match HttpContext.state ctx with
    | None -> Successful.NO_CONTENT
    | Some store -> store.set "spotify-id" id

let readIdFromStore ctx =
    match HttpContext.state ctx with
    | None -> None
    | Some store -> store.get "spotify-id"

let app settings =
    let storage = StorageClient (settings.AzureConnection, settings.AzureTable)
    let authUrl = Spotify.buildAuthUrl settings.ClientId settings.RedirectUri settings.Scope ""
    let completeOAuth ctx = 
        processAuth settings.RedirectUri settings.ClientId settings.ClientSecret 
        >> storeAuthTokens storage
        >> copyIdToStore ctx

    choose [
        path "/" >=> Files.file "views/index.html"
        path "/login" >=> Redirection.redirect authUrl
        path "/logout" >=> clearAuth >=> Redirection.redirect "/"
        path "/oauth" >=> statefulForSession >=> context (fun ctx ->
            let req = ctx.request
            match (req.queryParam "code", req.queryParam "error") with
            | Choice1Of2 code, _ ->
                completeOAuth ctx code 
                >=> Authentication.authenticated Session false
                >=> Redirection.redirect "/view"
            | _, Choice1Of2 error -> RequestErrors.UNAUTHORIZED error
        )
        pathScan "/content/%s" (sprintf "content/%s" >> Files.file)
        verifyAuth <| choose [
            path "/view" >=> statefulForSession >=> Files.file "views/artists.html"
            path "/api/artists" >=> statefulForSession >=> context (fun ctx ->
                match readIdFromStore ctx with
                | None -> RequestErrors.BAD_REQUEST "No spotify id was found."
                | Some id -> 
                    let user, metadata = storage.findUser id
                    let artists = httpGet |> Spotify.getTopArtists user.AccessToken Spotify.MediumTerm
                    artists.Items 
                    |> Array.fold (fun s i -> sprintf "%s, %s" i.Name s) "" 
                    |> Successful.OK
            )
        ]
        RequestErrors.NOT_FOUND "Not found."
    ]
