param(
  [string]$Destination = 'E:\SteamLibrary\steamapps\common\How to Fish\How to Fish\BepInEx\plugins\EnvinciblesMods-CompleteCheatMenu\CompleteCheatMenu.dll',
  [switch]$RestoreKai
)
$backup = 'E:\SteamLibrary\steamapps\common\How to Fish\How to Fish\BepInEx\plugins\EnvinciblesMods-CompleteCheatMenu\CompleteCheatMenu.pre-aim-20260825_123624.dll.bak'
Copy-Item -LiteralPath $backup -Destination $Destination -Force
if ($RestoreKai) {
  $disabled = 'E:\SteamLibrary\steamapps\common\How to Fish\How to Fish\BepInEx\plugins\Kai935-FishAimbot\AimbotRevised.dll.disabled'
  $enabled = 'E:\SteamLibrary\steamapps\common\How to Fish\How to Fish\BepInEx\plugins\Kai935-FishAimbot\AimbotRevised.dll'
  if (Test-Path -LiteralPath $disabled) { Move-Item -LiteralPath $disabled -Destination $enabled -Force }
}
Get-FileHash -LiteralPath $Destination -Algorithm SHA256
