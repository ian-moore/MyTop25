#r "./packages/FAKE/tools/FakeLib.dll"

open Fake
open Fake.FileHelper

let projectDir = __SOURCE_DIRECTORY__
let outputDir = projectDir </> "build"
let fsDir = projectDir </> "fs"
let jsDir = projectDir </> "js"

Target "Clean" (fun _ ->
    CleanDir outputDir
)

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

Target "Copy Files" (fun _ ->
    CopyDir (outputDir </> "views") (projectDir </> "views") allFiles
)

Target "Run" (fun _ ->
    let b, msg = FSIHelper.executeFSI projectDir "server.fsx" []
    msg |> Seq.iter (fun m -> printfn "%A" m)
)

Target "Default" DoNothing

"Clean"
    ==> "Restore"
    ==> "Build"
    ==> "Copy Files"
    ==> "Default"
    ==> "Run"

RunTargetOrDefault "Default"