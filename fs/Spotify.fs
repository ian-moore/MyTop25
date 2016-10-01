module MyTop25.Spotify

open FSharp.Data

[<Literal>]
let authUrlBase = "https://accounts.spotify.com/authorize"

[<Literal>]
let tokenUrl = "https://accounts.spotify.com/api/token"

[<Literal>]
let meUrl = "https://api.spotify.com/v1/me"

[<Literal>]
let internal sampleTokenResponse = """{
    "access_token":"string-token",
    "token_type":"string-type",
    "expires_in":3600,
    "refresh_token":"string-token",
    "scope":"string-scope"
}"""

[<Literal>]
let internal sampleUserResponse = """{
  "display_name":"JMWizzler",
  "email":"email@example.com",
  "external_urls":{
    "spotify":"https://open.spotify.com/user/wizzler"
  },
  "href":"https://api.spotify.com/v1/users/wizzler",
  "id":"wizzler",
  "images":[{
    "height":null,
    "url":"https://fbcdn...2330_n.jpg",
    "width":null
  }],
  "product":"premium",
  "type":"user",
  "uri":"spotify:user:wizzler"
}"""

[<Literal>]
let topArtistsUrl = "https://api.spotify.com/v1/me/top/artists?time_range="

[<Literal>]
let internal sampleTopArtistsResponse = """{
  "items" : [ {
    "external_urls" : {
      "spotify" : "https://open.spotify.com/artist/3TVXtAsR1Inumwj472S9r4"
    },
    "followers" : {
      "href" : null,
      "total" : 6671834
    },
    "genres" : [ "canadian pop", "hip hop", "pop rap", "rap" ],
    "href" : "https://api.spotify.com/v1/artists/3TVXtAsR1Inumwj472S9r4",
    "id" : "3TVXtAsR1Inumwj472S9r4",
    "images" : [ {
      "height" : 640,
      "url" : "https://i.scdn.co/image/cb080366dc8af1fe4dc90c4b9959794794884c66",
      "width" : 640
    }, {
      "height" : 320,
      "url" : "https://i.scdn.co/image/6bd672a0f33705eda4b543c304c21a152f393291",
      "width" : 320
    }, {
      "height" : 160,
      "url" : "https://i.scdn.co/image/085ae2e76f402468fe9982851b51cf876e4f20fe",
      "width" : 160
    } ],
    "name" : "Drake",
    "popularity" : 100,
    "type" : "artist",
    "uri" : "spotify:artist:3TVXtAsR1Inumwj472S9r4"
  }],
  "total" : 50,
  "limit" : 20,
  "offset" : 0,
  "href" : "https://api.spotify.com/v1/me/top/artists",
  "previous" : null,
  "next" : "https://api.spotify.com/v1/me/top/artists?limit=20&offset=20"
}"""

type OAuthTokens = JsonProvider<sampleTokenResponse>
type SpotifyUser = JsonProvider<sampleUserResponse>
type TopArtists = JsonProvider<sampleTopArtistsResponse>

type TimeRange =
    | LongTerm
    | MediumTerm
    | ShortTerm

let timeRangeString t =
    match t with
    | LongTerm -> "long_term"
    | MediumTerm -> "medium_term"
    | ShortTerm -> "short_term"

let buildAuthUrl clientId redirectUri scope state =
    sprintf "%s?client_id=%s&response_type=code&redirect_uri=%s&scope=%s&state=%s" authUrlBase clientId redirectUri scope state

let makeTokenHeaders code redirectUri clientId clientSecret = 
  [ "grant_type", "authorization_code"
    "code", code
    "redirect_uri", redirectUri
    "client_id", clientId
    "client_secret", clientSecret ]

let requestTokens postParams requester =
    postParams |> requester tokenUrl |> OAuthTokens.Parse

let internal authHeader token =
    ["Authorization", sprintf "Bearer %s" token]

let requestUser accessToken requester =
    authHeader accessToken
    |> requester meUrl 
    |> SpotifyUser.Parse

let getTopArtists accessToken timeRange requester =
    let url = timeRangeString timeRange |> sprintf "%s%s" topArtistsUrl
    authHeader accessToken
    |> requester url
    |> TopArtists.Parse