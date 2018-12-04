$documentsPath = [Environment]::GetFolderPath('MyDocuments')
$folderName = 'Pentaho'
cd $documentsPath
New-Item -ErrorAction Ignore -Path $folderName -ItemType 'directory'
cd $folderName
$client = new-object System.Net.WebClient
$downloadPath = 'https://github.com/Flexberry/NewPlatform.Flexberry.Analytics/tree/master/pentaho'
$client.DownloadFile("$downloadPath/.env", "$pwd\.env")
$client.DownloadFile("$downloadPath/pull.sh", "$pwd\pull.cmd")
$client.DownloadFile("$downloadPath/docker-compose.yml", "$pwd\docker-compose.yml")
$client.DownloadFile("$downloadPath/composeStart.sh", "$pwd\composeStart.cmd")
$client.DownloadFile("$downloadPath/composeStop.sh", "$pwd\composeStop.cmd")
$client.DownloadFile("$downloadPath/swarmStart.sh", "$pwd\swarmStart.cmd")
$client.DownloadFile("$downloadPath/swarmStop.sh", "$pwd\swarmStop.cmd")
