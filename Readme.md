# Installation

The package is published to nuget.org, so no custom feed or credentials are required.

```bash
dotnet tool install --global Itecho.TsGen
```

To pin it per-repository instead, add a local tool manifest and install into it:

```bash
dotnet new tool-manifest        # if you don't already have one
dotnet tool install Itecho.TsGen
```

Which will generate the following file:

```json
{
  "version": 1,
  "isRoot": true,
  "tools": {
    "itecho.tsgen": {
      "version": "1.0.0",
      "commands": [
        "itecho.tsgen"
      ]
    }
  }
}
```

Then run the following command to restore the tool:

```bash
dotnet tool restore
```

# Usage

```bash
dotnet itecho.tsgen ...
```

Configure in csproj to run the tool after build

```xml

<Target Name="PostBuild" AfterTargets="PostBuildEvent">
    <Exec Command="dotnet itecho.tsgen ..."/>
</Target>
```

# Prepare Release

Update .csproj version, create tag in git and push.
Automated with tool Versionize. https://github.com/versionize/versionize
Follow commit convention: https://www.conventionalcommits.org/en/v1.0.0/

```bash
dotnet versionize
```

```bash
# fix
git commit -a -m "fix: something went wrong we need a bugfix release"
dotnet versionize
# Generates version 1.0.1

# feat
git commit -a -m "feat: something really awesome coming in the next release"
dotnet versionize
# Generates version 1.1.0

# BREAKING CHANGE
git commit -a -m "feat: a really cool new feature" -m "BREAKING CHANGE: the API will break. sorry"
dotnet versionize
# Generates version 2.0.0
```

# Publish

After running `dotnet versionize`, the version is bumped, CHANGELOG.md is updated, and a git tag is created.

## Step 1: Push changes and tags

```bash
git push --follow-tags origin main
```

## Step 2: Create a GitHub Release

The GitHub Actions workflow triggers on release creation (not tag push). Create a release using one of these methods:

**Option A: GitHub CLI**
```bash
gh release create v8.0.3 --generate-notes
```

**Option B: GitHub Web UI**
1. Go to https://github.com/itechodev/typescriptgen/releases/new
2. Select the tag (e.g., `v8.0.3`)
3. Add a title and description
4. Click "Publish release"

## What happens next

The GitHub Actions workflow (`.github/workflows/release.yml`) will:
1. Build the project in Release mode
2. Run tests
3. Pack the NuGet package
4. Push to nuget.org at `https://api.nuget.org/v3/index.json` (using the `NUGET_ORG_API_KEY` repository secret)

