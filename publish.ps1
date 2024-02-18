$runtimes = @('linux-x64', 'linux-arm', 'win-x64', 'osx-x64', 'osx-arm64')

foreach ($runtime in $runtimes) {
    dotnet publish -c Release -r $runtime -p:PublishReadyToRun=false -p:TieredCompilation=false -p:PublishTrimmed=false --self-contained -o ./publish/$runtime
    Push-Location -Path "publish/$runtime" | Out-Null
    Compress-Archive -Path * -DestinationPath "../iFakeLocation-$runtime.zip" -Force
    Pop-Location
}
