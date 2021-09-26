$packages = ".\.packages"

if(Test-Path $packages) { Remove-Item $packages -Force -Recurse }

$libraries = Get-ChildItem .\src\ImageMatchNet*

foreach ($library in $libraries) {
    dotnet pack $library -c Release -o $packages
}