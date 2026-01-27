$mappingFiles = Get-ChildItem -Path "Libraries\Nop.Data\Mapping" -Filter "*Map.cs" -Recurse
$converted = 0
$skipped = 0

foreach ($file in $mappingFiles) {
    $content = Get-Content $file.FullName -Raw
    
    # Skip if already converted
    if ($content -match "IEntityTypeConfiguration" -or $content -match "EntityTypeBuilder") {
        $skipped++
        continue
    }
    
    # Skip base class
    if ($file.Name -eq "NopEntityTypeConfiguration.cs") {
        $skipped++
        continue
    }
    
    # Pattern replacements
    $newContent = $content
    
    # Add using statements
    if ($content -notmatch "Microsoft.EntityFrameworkCore") {
        $usingBlock = "using Microsoft.EntityFrameworkCore;`nusing Microsoft.EntityFrameworkCore.Metadata.Builders;`n"
        $newContent = $newContent -replace "(using [^;]+;`n)", "`$1$usingBlock"
    }
    
    # Extract class name and entity type
    if ($content -match "class\s+(\w+)\s*:\s*NopEntityTypeConfiguration<(\w+)>") {
        $className = $Matches[1]
        $entityType = $Matches[2]
        
        # Replace constructor with Configure method
        $newContent = $newContent -replace "public\s+$className\s*\(\s*\)\s*`n\s*\{", "public override void Configure(EntityTypeBuilder<$entityType> builder)`n        {"
        
        # Replace this. with builder.
        $newContent = $newContent -replace "\bthis\.", "builder."
        
        # Update many-to-many
        $newContent = $newContent -replace "\.Map\(m\s*=>\s*m\.ToTable\(`"(.+?)`"\)\)", ".UsingEntity(j => j.ToTable(`"`$1`"))"
        
        # Add PostInitialize
        $newContent = $newContent -replace "(\n\s+)\}", "`$1    PostInitialize();`$1}"
        
        Set-Content -Path $file.FullName -Value $newContent -NoNewline
        $converted++
        Write-Host "Converted: $($file.Name)"
    }
}

Write-Output "`n=== Conversion Complete ==="
Write-Output "Converted: $converted files"
Write-Output "Skipped: $skipped files"
