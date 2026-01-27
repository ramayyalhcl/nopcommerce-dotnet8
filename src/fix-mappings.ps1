$mappingFiles = Get-ChildItem -Path "Libraries\Nop.Data\Mapping" -Filter "*Map.cs" -Recurse | Where-Object { $_.Name -ne "NopEntityTypeConfiguration.cs" }
$fixed = 0

foreach ($file in $mappingFiles) {
    $content = Get-Content $file.FullName -Raw
    
    # Skip if looks good already
    if ($content -match "HasOne\(\)" -and $content -notmatch "HasRequired") {
        continue
    }
    
    $lines = Get-Content $file.FullName
    $newLines = @()
    $inUsings = $true
    $usingsAdded = $false
    
    foreach ($line in $lines) {
        # Add EF Core usings after first using statement
        if ($inUsings -and $line -match "^using " -and !$usingsAdded) {
            $newLines += $line
            $newLines += "using Microsoft.EntityFrameworkCore;"
            $newLines += "using Microsoft.EntityFrameworkCore.Metadata.Builders;"
            $usingsAdded = $true
            continue
        }
        
        if ($line -notmatch "^using ") {
            $inUsings = $false
        }
        
        # Fix HasRequired relationships
        $line = $line -replace "\.HasRequired\((.*?)\)\.WithMany\(\)\.HasForeignKey\((.*?)\)\.WillCascadeOnDelete\(false\)", ".HasOne(`$1).WithMany().HasForeignKey(`$2).OnDelete(DeleteBehavior.Restrict)"
        $line = $line -replace "\.HasRequired\((.*?)\)\.WithMany\(\)\.HasForeignKey\((.*?)\)\.WillCascadeOnDelete\(true\)", ".HasOne(`$1).WithMany().HasForeignKey(`$2).OnDelete(DeleteBehavior.Cascade)"
        $line = $line -replace "\.HasRequired\((.*?)\)\.WithMany\((.*?)\)\.HasForeignKey\((.*?)\)\.WillCascadeOnDelete\(false\)", ".HasOne(`$1).WithMany(`$2).HasForeignKey(`$3).OnDelete(DeleteBehavior.Restrict)"
        $line = $line -replace "\.HasRequired\((.*?)\)\.WithMany\((.*?)\)\.HasForeignKey\((.*?)\)\.WillCascadeOnDelete\(true\)", ".HasOne(`$1).WithMany(`$2).HasForeignKey(`$3).OnDelete(DeleteBehavior.Cascade)"
        $line = $line -replace "\.HasRequired\((.*?)\)\.WithMany\(\)", ".HasOne(`$1).WithMany().IsRequired()"
        $line = $line -replace "\.HasRequired\((.*?)\)\.WithMany\((.*?)\)", ".HasOne(`$1).WithMany(`$2).IsRequired()"
        
        # Fix HasOptional
        $line = $line -replace "\.HasOptional\((.*?)\)\.WithMany\(\)", ".HasOne(`$1).WithMany()"
        $line = $line -replace "\.HasOptional\((.*?)\)\.WithMany\((.*?)\)", ".HasOne(`$1).WithMany(`$2)"
        
        # Remove duplicate PostInitialize
        if ($line -match "^\s+PostInitialize\(\);\s*$" -and ($newLines[-1] -match "PostInitialize" -or $newLines[-2] -match "PostInitialize")) {
            continue
        }
        
        $newLines += $line
    }
    
    Set-Content -Path $file.FullName -Value ($newLines -join "`n") -NoNewline
    $fixed++
    Write-Host "Fixed: $($file.Name)"
}

Write-Output "`n=== Fix Complete ==="
Write-Output "Fixed: $fixed files"
