param(
    [string] $ResultsDirectory = "TestResults",
    [int] $MaxFailuresToPrint = 50
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Escape-GitHubCommandValue
{
    param([string] $Value)

    return $Value.Replace("%", "%25").Replace("`r", "%0D").Replace("`n", "%0A")
}

function Escape-GitHubCommandProperty
{
    param([string] $Value)

    return (Escape-GitHubCommandValue $Value).Replace(":", "%3A").Replace(",", "%2C")
}

function Get-FirstLine
{
    param([string] $Value)

    if ([string]::IsNullOrWhiteSpace($Value))
    {
        return ""
    }

    return (($Value.Trim() -split "`r?`n", 2)[0]).Trim()
}

function Get-StackLocation
{
    param([string] $StackTrace)

    if ([string]::IsNullOrWhiteSpace($StackTrace))
    {
        return $null
    }

    foreach ($line in $StackTrace -split "`r?`n")
    {
        if ($line -match '\s+in\s+(.+):line\s+(\d+)\s*$')
        {
            $path = $Matches[1].Replace("\", "/")
            $workspace = $env:GITHUB_WORKSPACE

            if (![string]::IsNullOrWhiteSpace($workspace))
            {
                $workspace = $workspace.Replace("\", "/").TrimEnd("/")
                if ($path.StartsWith("$workspace/"))
                {
                    $path = $path.Substring($workspace.Length + 1)
                }
            }

            return [pscustomobject]@{
                File = $path
                Line = [int] $Matches[2]
            }
        }
    }

    return $null
}

function Get-ChildText
{
    param(
        [System.Xml.XmlNode] $Node,
        [string] $XPath
    )

    $child = $Node.SelectSingleNode($XPath)
    if ($null -eq $child)
    {
        return ""
    }

    return $child.InnerText
}

function Write-TestFailureAnnotation
{
    param(
        [string] $Title,
        [string] $Message,
        [object] $Location
    )

    $properties = "title=$(Escape-GitHubCommandProperty $Title)"
    if ($null -ne $Location)
    {
        $properties += ",file=$(Escape-GitHubCommandProperty $Location.File),line=$($Location.Line)"
    }

    Write-Host "::error $properties::$(Escape-GitHubCommandValue $Message)"
}

function Write-StepSummary
{
    param([System.Collections.Generic.List[object]] $Failures)

    if ([string]::IsNullOrWhiteSpace($env:GITHUB_STEP_SUMMARY))
    {
        return
    }

    $lines = [System.Collections.Generic.List[string]]::new()
    $lines.Add("### Failed tests")
    $lines.Add("")
    $lines.Add("Found $($Failures.Count) failed test result(s).")
    $lines.Add("")
    $lines.Add("| Test | Error |")
    $lines.Add("| --- | --- |")

    foreach ($failure in $Failures | Select-Object -First $MaxFailuresToPrint)
    {
        $testName = $failure.TestName.Replace("|", "\|")
        $message = (Get-FirstLine $failure.Message).Replace("|", "\|")
        $lines.Add("| ``$testName`` | $message |")
    }

    if ($Failures.Count -gt $MaxFailuresToPrint)
    {
        $lines.Add("")
        $lines.Add("Only the first $MaxFailuresToPrint failures are shown in this summary.")
    }

    Add-Content -Path $env:GITHUB_STEP_SUMMARY -Value $lines
}

if (!(Test-Path -LiteralPath $ResultsDirectory))
{
    Write-Host "No test result directory found: $ResultsDirectory"
    return
}

$trxFiles = @(Get-ChildItem -LiteralPath $ResultsDirectory -Filter "*.trx" -File -Recurse)
if ($trxFiles.Count -eq 0)
{
    Write-Host "No TRX files found in $ResultsDirectory."
    return
}

$failures = [System.Collections.Generic.List[object]]::new()

foreach ($trxFile in $trxFiles)
{
    try
    {
        [xml] $document = Get-Content -LiteralPath $trxFile.FullName -Raw
        $results = $document.SelectNodes("//*[local-name()='UnitTestResult']")

        foreach ($result in $results)
        {
            $outcome = $result.GetAttribute("outcome")
            if ($outcome -in @("Passed", "NotExecuted", "Inconclusive"))
            {
                continue
            }

            $message = Get-ChildText $result ".//*[local-name()='ErrorInfo']/*[local-name()='Message']"
            $stackTrace = Get-ChildText $result ".//*[local-name()='ErrorInfo']/*[local-name()='StackTrace']"

            $failures.Add([pscustomobject]@{
                TestName = $result.GetAttribute("testName")
                Outcome = $outcome
                Duration = $result.GetAttribute("duration")
                Message = $message
                StackTrace = $stackTrace
                ResultFile = $trxFile.Name
                Location = Get-StackLocation $stackTrace
            })
        }
    }
    catch
    {
        Write-Warning "Failed to parse '$($trxFile.FullName)': $_"
    }
}

if ($failures.Count -eq 0)
{
    Write-Host "No failed tests found in $ResultsDirectory."
    return
}

Write-Host ""
Write-Host "==================== Failed test summary ===================="
Write-Host "Found $($failures.Count) failed test result(s) in $($trxFiles.Count) TRX file(s)."
Write-Host ""

foreach ($failure in $failures | Select-Object -First $MaxFailuresToPrint)
{
    $firstLine = Get-FirstLine $failure.Message
    $annotationMessage = if ([string]::IsNullOrWhiteSpace($firstLine)) { $failure.Outcome } else { $firstLine }
    Write-TestFailureAnnotation $failure.TestName $annotationMessage $failure.Location

    Write-Host "FAILED: $($failure.TestName)"
    Write-Host "Outcome: $($failure.Outcome)"
    Write-Host "TRX: $($failure.ResultFile)"

    if ($null -ne $failure.Location)
    {
        Write-Host "Location: $($failure.Location.File):$($failure.Location.Line)"
    }

    if (![string]::IsNullOrWhiteSpace($failure.Duration))
    {
        Write-Host "Duration: $($failure.Duration)"
    }

    if (![string]::IsNullOrWhiteSpace($failure.Message))
    {
        Write-Host "Message:"
        Write-Host $failure.Message.Trim()
    }

    if (![string]::IsNullOrWhiteSpace($failure.StackTrace))
    {
        Write-Host "Stack trace:"
        Write-Host $failure.StackTrace.Trim()
    }

    Write-Host ""
}

if ($failures.Count -gt $MaxFailuresToPrint)
{
    Write-Host "Only the first $MaxFailuresToPrint failures were printed."
}

Write-StepSummary $failures
