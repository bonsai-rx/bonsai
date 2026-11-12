# Getting started

This page is the landing page of the documentation website. Replace it with an introduction to the package and how to use it.

## Building the website

The website uses the docfx tools repository, which must be added as a Git submodule from the documentation folder:

```
git submodule add https://github.com/bonsai-rx/docfx-tools bonsai-docfx
```

Building the website also requires PowerShell 7.4 or later and a Bonsai environment at the repository root, which is used to export images of the workflows. The website includes the API reference of the Release build, so restore the docfx tool and build the solution from the repository root first:

```
dotnet tool restore
dotnet build -c Release
```

To build and preview the website, run the `build.ps1` script from the documentation folder:

```
./build.ps1 --serve
```