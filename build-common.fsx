namespace Hoverhand

module BuildCommon =

    open Fake
    open System

    let buildMode = getBuildParamOrDefault "buildMode" "Release"

    let version = getBuildParamOrDefault "version" "0.0.0"

    let build pattern outputFolder _ = 
        !! pattern
            |> MSBuild outputFolder "Build" ["Configuration", buildMode]
            |> Log "Build-Output: "

    let zipPackage sourceFolder zipPath = 
        !! (sourceFolder + "/**/*.*")
            |> Zip (sourceFolder + "/") zipPath

    let nugetPackage baseFolder packageName description outputPath = 
        NuGet (fun p -> 
            { p with
                Project = packageName
                Version = version
                Summary = description
                Description = description
                WorkingDir = baseFolder
                OutputPath = outputPath
                Files = [
                    ((@"\**\*.*"), None, None)
                ]
            }) "Template.nuspec"

        
    let runTests testPattern outputPath = (fun _ -> 
        !! (testPattern)
            |> NUnit (fun p ->
                {p with 
                    DisableShadowCopy = true;
                    ToolPath = "../../tools/NUnit";
                    OutputFile = outputPath
                }
            )
    )

    Target "RestorePackages" (fun _ -> 
        !! "./**/packages.config"
        |> Seq.iter (RestorePackage (fun p -> 
            { p with
                Sources =   "https://www.nuget.org/api/v2" ::
                            "https://www.myget.org/F/cognisant-libs/api/v2" ::
                            p.Sources
                TimeOut = TimeSpan.FromMinutes 10.
            }))
    )

