[CmdletBinding(PositionalBinding=$false)]
Param(
    [string][Alias('config')]$configuration = "Debug",
    [string][Alias('r')]$runtime = "win-x64",
    [switch] $clean,
    [switch] $restore,
    [switch] $build,
    [switch] $test,
    [switch] $publish,
    [switch] $deploy
)

$SolutionPath = $PSScriptRoot + "\..\src\taskmgr.sln"
$ProjectPath = $PSScriptRoot + "\..\src\taskmgr\taskmgr.csproj"

function Publish([string] $config, [string] $rid) {
    Write-Host "🛠️ Publishing project with configuration: $config, runtime: $rid"
    & "dotnet" publish $ProjectPath -c $config -r $rid --self-contained -p:PublishAot=true
}

if ($clean) {
    & "dotnet" clean $SolutionPath
}

if ($build) {
    & "dotnet" build $SolutionPath /p:configuration=$configuration /p:buildtests=true
}

if ($restore) {
    & "dotnet" restore $SolutionPath
}

if ($test) {
    & "dotnet" test $SolutionPath
}

if ($publish) {
    Publish -config $configuration -rid $runtime
}

if ($deploy) {
    Publish -config Release -rid $runtime
}
