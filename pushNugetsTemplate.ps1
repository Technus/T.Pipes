# Copy to pushNugets.ps1, and leave the template alone as to not include the private API key

$API_KEY = "INSERT_API_KEY_HERE_BUT_NOT_IN_TEMPLATE"
$VERSION = "1.0.0"

dotnet nuget push ".\T.Pipes.Abstractions\bin\Release\T.Pipes.Abstractions.$VERSION.nupkg" --api-key $API_KEY --source https://api.nuget.org/v3/index.json
dotnet nuget push ".\T.Pipes.SourceGeneration\bin\Release\T.Pipes.SourceGeneration.$VERSION.nupkg" --api-key $API_KEY --source https://api.nuget.org/v3/index.json
dotnet nuget push ".\T.Pipes\bin\Release\T.Pipes.$VERSION.nupkg" --api-key $API_KEY --source https://api.nuget.org/v3/index.json