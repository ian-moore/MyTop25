#r "./packages/FAKE/tools/FakeLib.dll"

open Fake

let projectDir = __SOURCE_DIRECTORY__
let outputDir = projectDir </> "build"
let fsDir = projectDir </> "fs"
let jsDir = projectDir </> "js"

Target "Restore" (fun _ ->
    trace "NPM here"
)

Target "Build" (fun _ ->
    !! (fsDir </> "**/*.fsproj")
    |> MSBuildHelper.MSBuild
        outputDir
        "Build"
        ["Configuration", "Debug"]
    |> Log "Build Output: "
)

Target "Run" (fun _ ->
    let b, msg = FSIHelper.executeFSI projectDir "server.fsx" []
    msg |> Seq.iter (fun m -> printfn "%A" m)
)

Target "Default" DoNothing

"Restore"
    ==> "Build"
    ==> "Default"
    ==> "Run"

RunTargetOrDefault "Default"