dotnet run --project ./CakeBuild/CakeBuild.csproj -- "$@"
rm -rf "$VINTAGE_STORY/Mods/spawnersapi"
cp -r ./Releases/spawnersapi "$VINTAGE_STORY/Mods/spawnersapi"