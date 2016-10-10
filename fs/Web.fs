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
        (fun () -> Choice2Of2 <| Redirection.redirect "/login")
        (fun _ ->  Choice2Of2 <| Redirection.redirect "/login")
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

let serializeArtists (artists: Spotify.TopArtists.Root) (user: Spotify.SpotifyUser.Root) =
    let jsonProps = Map.ofList >> Chiron.Object
    artists.Items
    |> Array.fold (fun (a, m) x -> x.Id::a, m |> Map.add x.Id x) (List.empty, Map.empty)
    |> (fun (ids, artists) -> 
        jsonProps [
            "topIds", Chiron.Array (ids |> List.rev |> List.map Chiron.String)
            "artists", artists |> Map.map (fun k a ->
                jsonProps [
                    "id", Chiron.String a.Id
                    "name", Chiron.String a.Name
                    "externalUrl", Chiron.String a.ExternalUrls.Spotify
                    "imageUrl", Chiron.String (a.Images 
                        |> Array.map (fun i -> i.Url)
                        |> Array.head)
                ]) |> Chiron.Object
            "userDisplayName", Chiron.String user.DisplayName
            "userImage", Chiron.String (user.Images 
                |> Array.map (fun i -> i.Url) 
                |> Array.head)
        ])
    |> Chiron.Formatting.Json.format

let buildArtistResponse duration (user, metadata) = 
    let artists = httpGet |> Spotify.getTopArtists user.AccessToken duration
    let spotifyUser = httpGet |> Spotify.requestUser user.AccessToken
    serializeArtists artists spotifyUser
    |> Successful.OK
    >=> Writers.setMimeType "application/json; charset=utf-8"

let app settings =
    let storage = StorageClient (settings.AzureConnection, settings.AzureTable)
    let authUrl = Spotify.buildAuthUrl settings.ClientId settings.RedirectUri settings.Scope ""
    let completeOAuth ctx = 
        processAuth settings.RedirectUri settings.ClientId settings.ClientSecret 
        >> storeAuthTokens storage
        >> copyIdToStore ctx

    statefulForSession >=> choose [
        path "/" >=> Files.file "views/index.html"
        path "/login" >=> Redirection.redirect authUrl
        path "/logout" >=> clearAuth >=> Redirection.redirect "/"
        path "/oauth" >=> context (fun ctx ->
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
            path "/view" >=> Files.file "views/artists.html"
            path "/api/artists" >=> context (fun ctx ->
                match readIdFromStore ctx with
                | None -> RequestErrors.BAD_REQUEST "No spotify id was found."
                | Some id -> 
                    match ctx.request.queryParam "duration" with
                    | Choice1Of2 d -> storage.findUser id |> buildArtistResponse (Spotify.parseDuration d)
                    | Choice2Of2 _ -> storage.findUser id |> buildArtistResponse Spotify.MediumTerm
            )
        ]
        RequestErrors.NOT_FOUND "Not found."
    ]
