#!/bin/bash

configuration="Debug"

usage() {
    echo "Usage: $0 [OPTIONS]"
    echo ""
    echo "Options:"
    echo "  -c, --config <config>   Specify the build configuration (e.g., Release). Default: Debug"
    echo "  --clean                 Clean the solution."
    echo "  --restore               Restore NuGet packages."
    echo "  --build                 Build the solution."
    echo "  --test                  Run tests."
    echo "  --publish               Publish the project."
    echo "  --deploy                Publish the project with 'Release' configuration (overrides --config)."
    echo "  -h, --help              Display this help message."
    echo ""
    echo "Example: $0 --clean --restore --build --test --publish --config Release"
    exit 1
}

while [[ "$#" -gt 0 ]]; do
    case "$1" in
        -c|--config)
            configuration="$2"
            shift # Skip argument name
            ;;
        --clean)
            clean=true
            ;;
        --restore)
            restore=true
            ;;
        --build)
            build=true
            ;;
        --test)
            test=true
            ;;
        --publish)
            publish=true
            ;;
        --deploy)
            deploy=true
            ;;
        -h|--help)
            usage
            ;;
        *)
            echo "Error: Unknown parameter $1"
            usage
            ;;
    esac
    shift # Move to next argument/value
done

# --- Define Paths ---
# In bash, $0 is the script name. PWD is the current working directory.
# The `dirname` command is used to get the directory of the script.
ScriptDir="$( cd "$( dirname "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )"

SolutionPath="$ScriptDir/../src/taskmgr.sln"
ProjectPath="$ScriptDir/../src/taskmgr/taskmgr.csproj"

function publish_project() {
    local config=$1
    echo "🛠️ Publishing project with configuration: $config"
    # For a general macOS build, you can use 'osx-x64' or 'osx-arm64' depending on the target machine,
    # or omit the '-r' flag entirely to build for the current host environment.
    # We will omit it for a general host build. If you need a specific RID, uncomment and change the line below:
    # dotnet publish "$ProjectPath" -c "$config" -r osx-x64 --self-contained -p:PublishAot=true
    dotnet publish "$ProjectPath" -c "$config" -r osx-arm64 --self-contained -p:PublishAot=true
}

if [[ "$clean" == true ]]; then
    echo "Cleaning solution..."
    dotnet clean "$SolutionPath"
fi

if [[ "$restore" == true ]]; then
    echo "Restoring NuGet packages..."
    dotnet restore "$SolutionPath"
fi

if [[ "$build" == true ]]; then
    echo "Building solution with configuration: $configuration"
    # Configuration is passed via -c/--configuration flag, not /p:configuration=
    dotnet build "$SolutionPath" --configuration "$configuration" /p:buildtests=true
fi

if [[ "$test" == true ]]; then
    echo "Running tests..."
    dotnet test "$SolutionPath"
fi

if [[ "$publish" == true ]]; then
    publish_project "$configuration"
fi

if [[ "$deploy" == true ]]; then
    # Overrides any passed configuration to force 'Release'
    publish_project "Release"
fi

echo "Script finished."
