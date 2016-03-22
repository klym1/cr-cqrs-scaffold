namespace build

module common =

    open Fake
    open Fake.Testing.NUnit3
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

        
    let runTests testPattern outputPath outputFile = (fun _ -> 
        !! (testPattern)
            |> NUnit3 (fun p ->
                {p with 
                    ShadowCopy = false;
                    ToolPath = "../tools/nunit/nunit3-console.exe";
                    ResultSpecs = [outputPath + outputFile];
                }
            )
    )

    Target "RestorePackages" (fun _ -> 
        !! "./**/packages.config"
        |> Seq.iter (RestorePackage (fun p -> 
            { p with
                Sources =   "https://api.nuget.org/v3/index.json" ::
                            "https://www.myget.org/F/cognisant-libs/api/v2" ::
                            p.Sources
                TimeOut = TimeSpan.FromMinutes 10.
            }))
    )