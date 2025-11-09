[CmdletBinding(PositionalBinding=$false)]
Param(
    [string][Alias('config')]$configuration = "Debug",
    [switch] $clean,
    [switch] $restore,
    [switch] $build,
    [switch] $test,
    [switch] $publish,
    [switch] $deploy
)

$SolutionPath = $PSScriptRoot + "\..\src\taskmgr.sln"
$ProjectPath = $PSScriptRoot + "\..\src\taskmgr\taskmgr.csproj"

function Publish([string] $config) {
    # dotnet publish $ProjectPath -c $config -r win-x64 --self-contained true /p:PublishTrimmed=false /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true /p:UseAppHost=true
    & "dotnet" publish $ProjectPath -c $config -r win-x64 --self-contained -p:PublishAot=true
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
    Publish -config $configuration
}

if ($deploy) {
    Publish -config Release    
}
