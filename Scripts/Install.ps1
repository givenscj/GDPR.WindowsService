function Execute-Command
{
    param([string]$Command, [switch]$ShowOutput=$True)
    echo $Command
    if ($ShowOutput) {
        Invoke-Expression $Command
    } else {
        $out = Invoke-Expression $Command
    }
}

function GetConfiguration()
{
	[xml]$appConfig = get-content "../app.config";

    $mode = $appConfig.configuration.appSettings.add | where {$_.key -eq "Mode"}

    $json = get-content "../configuration.$($mode.value).json" -raw;

    $config = ConvertFrom-json $json;

    return $config;

    #[GDPR.Common.Configuration]::Load($json.toString());
}

function Install($svcConfig)
{
    $appId = $svcConfig.ApplicationId;
    $platform = $svcConfig.platform;

	$installUtilPath = "";
	$netFramework = "v4.0.30319";
	$installFolder = $Env:Programfiles + "\GDPR.Service\" + $appId;

    [System.io.directory]::CreateDirectory($installFolder);
    
	switch($svcConfig.platform)
	{
		"x64"
		{
			$installUtilPath = "C:\Windows\Microsoft.NET\Framework64\$netFramework\installutil.exe";		
            break;
		}
		"x32"
		{
			$installUtilPath = "C:\Windows\Microsoft.NET\Framework\$netFramework\installutil.exe";
            break;
		}
	}
	
	$serviceName = "GDPR." + $svcConfig.ApplicationId + ".exe";

	#copy the file to a specific folder...
	Copy-Item "../Bin/Debug/*" -Destination $installFolder -Recurse -ea SilentlyContinue;

	#rename the service and config...
	rename-item "$installFolder/GDPR.Service.exe" "$installFolder/$serviceName" -Force -ea SilentlyContinue;
    rename-item "$installFolder/GDPR.Service.exe.config" "$installFolder/$serviceName.config" -Force -ea SilentlyContinue;

    sc.exe delete $serviceName;
    #Remove-Service $serviceName;

    New-Service -Name $serviceName -BinaryPathName $("$installFolder/$serviceName") -DisplayName $serviceName -StartupType Automatic -Description $serviceName;

	#start the service...
	Start-Service $serviceName;
}

#load newtonsoft
Add-Type -Path "Newtonsoft.Json.dll";
Add-Type -Path "../Bin/Debug/GDPR.Common.dll";

$svcConfig = GetConfiguration;

Install $svcConfig;