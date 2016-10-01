module MyTop25.Storage

open FSharp.Azure.Storage.Table
open Microsoft.WindowsAzure.Storage
open Microsoft.WindowsAzure.Storage.Table

type User = 
    { [<PartitionKey>] Object: string
      [<RowKey>] UserId: string
      RefreshToken: string
      AccessToken: string
      TokenValidTo: System.DateTime }

let azureUser =
  { Object = "user"
    UserId = ""
    RefreshToken = ""
    AccessToken = ""
    TokenValidTo = System.DateTime.MinValue }

type StorageClient (connectionString, tableName) =
    let a = CloudStorageAccount.Parse connectionString
    let tc = a.CreateCloudTableClient ()
    let inMyTable x = inTable tc tableName x
    let fromMyTable q = fromTable tc tableName q

    member this.insertOrReplace x =
        InsertOrReplace x |> inMyTable

    member this.findUser userId =
        Query.all<User>
        |> Query.where <@ fun g s -> s.PartitionKey = "user" && s.RowKey = userId @>
        |> fromMyTable
        |> Seq.head
    

