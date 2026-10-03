# Reset MySQL Root Password Script
# Requires Administrator privileges

$ErrorActionPreference = 'Continue'
$mysqlBase = "C:\Program Files\MySQL\MySQL Server 8.0"
$mysqld = Join-Path $mysqlBase "bin\mysqld.exe"
$mysql = Join-Path $mysqlBase "bin\mysql.exe"
$myIni = "C:\PROGRA~3\MySQL\MYSQLS~1.0\my.ini"
$serviceName = "MySQL80"
$newPassword = "root"

Write-Host "========================================================" -ForegroundColor Cyan
Write-Host "         MySQL Root Password Reset Utility              " -ForegroundColor Cyan
Write-Host "========================================================" -ForegroundColor Cyan

# Step 1: Stop MySQL80 service
Write-Host "`n[1/5] Stopping MySQL service..." -ForegroundColor Yellow
Stop-Service -Name $serviceName -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 3

# Also kill any running mysqld processes to prevent port conflicts
Get-Process -Name mysqld -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 2
Write-Host "  MySQL service stopped." -ForegroundColor Green

# Step 2: Create SQL reset init script
$initSqlPath = Join-Path $env:TEMP "mysql_reset_init.sql"
Write-Host "`n[2/5] Preparing password reset SQL file..." -ForegroundColor Yellow
$sqlContent = @"
ALTER USER 'root'@'localhost' IDENTIFIED BY '$newPassword';
CREATE DATABASE IF NOT EXISTS digital_memory_map CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
FLUSH PRIVILEGES;
"@
[System.IO.File]::WriteAllText($initSqlPath, $sqlContent, [System.Text.Encoding]::ASCII)
Write-Host "  SQL init script created at $initSqlPath" -ForegroundColor Green

$myIni = "C:\PROGRA~3\MySQL\MYSQLS~1.0\my.ini"
if (-not (Test-Path $myIni)) {
    $myIni = (cmd.exe /c "for %I in (`"C:\ProgramData\MySQL\MySQL Server 8.0\my.ini`") do @echo %~sI").Trim()
}

# Step 3: Start mysqld temporarily with --init-file
Write-Host "`n[3/5] Applying new password to MySQL..." -ForegroundColor Yellow
$procArgs = @(
    "--defaults-file=$myIni",
    "--init-file=$initSqlPath",
    "--console"
)
$process = Start-Process -FilePath $mysqld -ArgumentList $procArgs -PassThru -NoNewWindow
Start-Sleep -Seconds 8

# Step 4: Terminate the temporary mysqld process
Write-Host "`n[4/5] Finalizing changes..." -ForegroundColor Yellow
if ($process -and -not $process.HasExited) {
    Stop-Process -Id $process.Id -Force -ErrorAction SilentlyContinue
}
Get-Process -Name mysqld -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 3
Remove-Item $initSqlPath -Force -ErrorAction SilentlyContinue

# Step 5: Start MySQL service normally
Write-Host "`n[5/5] Restarting MySQL service normally..." -ForegroundColor Yellow
Start-Service -Name $serviceName
Start-Sleep -Seconds 3

# Verification
Write-Host "`n========================================================" -ForegroundColor Cyan
Write-Host "                 Verifying Connection                   " -ForegroundColor Cyan
Write-Host "========================================================" -ForegroundColor Cyan

$testOutput = & $mysql -u root "-p$newPassword" -e "SHOW DATABASES LIKE 'digital_memory_map';" 2>&1
if ($LASTEXITCODE -eq 0) {
    Write-Host "`n>>> SUCCESS! MySQL root password has been reset to: root" -ForegroundColor Green
    Write-Host ">>> Database 'digital_memory_map' is ready!" -ForegroundColor Green
} else {
    Write-Host "`n[!] Verification note: $testOutput" -ForegroundColor Yellow
}

Write-Host "`nYou can now return to the browser and register or log in!" -ForegroundColor Cyan
