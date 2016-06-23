#I @"packages\FAKE\"
#r "FakeLib.dll"
#load @".\csharp\lib\PomUtil.fsx"

open Fake
open Fake.AssemblyInfoFile
open Fake.SignHelper
open System.IO

let tags = "c# phonenumbers libphonenumber"

let toolsDir = @".\csharp\lib\"
let buildDir = @".\build\"
let signedDir = @".\build\signed\"
let deployDir = @".\artifacts\"
let testDir =  @".\test\"
let metaDir = @"csharp\PhoneNumbers\"
let keyFile = @".\csharp\PhoneNumbers\key.snk"
let pomPath = @".\java\pom.xml"

let nunitPath = @".\packages\NUnit.Runners\tools"

let appReferences  = !! @"csharp\PhoneNumbers\PhoneNumbers.csproj"     
let testReferences = !! @"csharp\PhoneNumbers.Test\*.csproj"     
let metadataFiles = !! @"paket-files\**\*.xml"

let incrementedVersion = 
  let pomVer = PomUtil.getPomProjectVersion()
  let verFile = if File.Exists "version.txt" then File.ReadAllText "version.txt" else ""
  let newVer = sprintf "%s.0" pomVer
  let currentVer = 
    if verFile = "" then newVer
    elif verFile.StartsWith(pomVer) then
      match verFile.Split([|'.'|]) with
      | [|_;_;_;buildNo|] -> int buildNo |> fun n -> sprintf "%s.%i" pomVer (n + 1)
      | _ -> newVer
    else newVer
  File.WriteAllText("version.txt",currentVer)
  currentVer

let baseAssemblyInfo = 
  [ Attribute.Title "PhoneNumbers"
    Attribute.Description "Google's libphonenumber"
    Attribute.Product "PhoneNumbers"
    Attribute.Copyright "Copyright © Google 2015-2016"
    Attribute.Version incrementedVersion
    Attribute.FileVersion incrementedVersion ]

Target "Clean" (fun _ ->
    CleanDirs [buildDir; testDir]
)

Target "CopyMetadata" (fun _ ->
  metadataFiles |> Seq.iter (CopyFile metaDir)
)

Target "BuildApp" (fun _ ->
    CreateCSharpAssemblyInfo @".\csharp\PhoneNumbers\Properties\AssemblyInfo.cs" <| baseAssemblyInfo @ [ Attribute.InternalsVisibleTo "PhoneNumbers.Test" ]

    MSBuildRelease buildDir "Build" appReferences
        |> Log "AppBuild-Output:"
)

Target "BuildTest" (fun _ ->
    MSBuildRelease testDir "Build" testReferences
        |> Log "TestBuild-Output:"
)

Target "Test" (fun _ ->
 !! (testDir + @"\*Test.dll") 
        |> NUnit (fun p -> 
            { p with 
                ToolPath = nunitPath; 
                TimeOut = System.TimeSpan.FromMinutes(60.)
                OutputFile = testDir + @"TestResults.xml" })
)

Target "BuildSigned" (fun _ ->
    let pomVer = sprintf "%s.*" <| PomUtil.getPomProjectVersion()
    CreateCSharpAssemblyInfo @".\csharp\PhoneNumbers\Properties\AssemblyInfo.cs" <| baseAssemblyInfo @ [ Attribute.KeyFile "key.snk" ]

    MSBuildRelease signedDir "Build"  appReferences 
        |> Log "AppBuild-Output:"   
)

  
Target "All" DoNothing

"Clean"
    ==> "CopyMetadata"
    ==> "BuildApp"
    ==> "BuildTest"
    ==> "Test"
    ==> "BuildSigned"
    ==> "All"

Run <| getBuildParamOrDefault "target" "All"