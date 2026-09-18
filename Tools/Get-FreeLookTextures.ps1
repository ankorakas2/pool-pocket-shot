# CC0 Poly Haven 1K maps into Assets/Resources/Look
$dir = Join-Path $PSScriptRoot "..\Assets\Resources\Look"
New-Item -ItemType Directory -Force -Path $dir | Out-Null
$files = @{
  "Wood_Albedo.jpg" = "https://dl.polyhaven.org/file/ph-assets/Textures/jpg/1k/wood_table_001/wood_table_001_diff_1k.jpg"
  "Wood_Normal.jpg" = "https://dl.polyhaven.org/file/ph-assets/Textures/jpg/1k/wood_table_001/wood_table_001_nor_gl_1k.jpg"
  "Wood_Rough.jpg" = "https://dl.polyhaven.org/file/ph-assets/Textures/jpg/1k/wood_table_001/wood_table_001_rough_1k.jpg"
  "Fabric_Albedo.jpg" = "https://dl.polyhaven.org/file/ph-assets/Textures/jpg/1k/fabric_pattern_07/fabric_pattern_07_col_1_1k.jpg"
  "Fabric_Normal.jpg" = "https://dl.polyhaven.org/file/ph-assets/Textures/jpg/1k/fabric_pattern_07/fabric_pattern_07_nor_gl_1k.jpg"
  "Fabric_Rough.jpg" = "https://dl.polyhaven.org/file/ph-assets/Textures/jpg/1k/fabric_pattern_07/fabric_pattern_07_rough_1k.jpg"
  "Metal_Albedo.jpg" = "https://dl.polyhaven.org/file/ph-assets/Textures/jpg/1k/metal_plate/metal_plate_diff_1k.jpg"
  "Metal_Normal.jpg" = "https://dl.polyhaven.org/file/ph-assets/Textures/jpg/1k/metal_plate/metal_plate_nor_gl_1k.jpg"
  "Metal_Rough.jpg" = "https://dl.polyhaven.org/file/ph-assets/Textures/jpg/1k/metal_plate/metal_plate_rough_1k.jpg"
  "Metal_Metallic.jpg" = "https://dl.polyhaven.org/file/ph-assets/Textures/jpg/1k/metal_plate/metal_plate_metal_1k.jpg"
}
foreach ($name in $files.Keys) {
  Invoke-WebRequest -Uri $files[$name] -OutFile (Join-Path $dir $name) -UseBasicParsing
}
Write-Host "Textures saved to $dir (CC0, Poly Haven). Reopen Unity so LookTextureImport marks normals."
