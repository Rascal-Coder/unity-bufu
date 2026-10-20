param([string]$Group='products',[string]$Prefix='Commercial',[string]$Player='E:\rea-games\unity-bufu\Build\LayoutFixCommercialPlayer\BufuRebuild.exe',[switch]$RecordMotion)
$fixRoot='E:\rea-games\unity-bufu\Documentation\LayoutFix-2026-10-20'
$cases=if($Group -in @('products','stores')) {
    @(@('FinalStore390','390','844','--verify-alignment','--purchases-only'),@('FinalStore540','540','960','--verify-alignment','--purchases-only'),@('FinalStoreLandscape','960','540','--verify-alignment','--purchases-only'),@('FinalLocale390','390','844','--verify-rebuild','--verify-localization'),@('FinalLocale540','540','960','--verify-rebuild','--verify-localization'),@('FinalLocaleLandscape','960','540','--verify-rebuild','--verify-localization'))
} elseif($Group -eq 'scenes') {
    @(@('FinalAlignment540','540','960','--verify-alignment','--alignment-full'),@('FinalDice390','390','844','--verify-alignment','--dice-only'),@('FinalDiceLandscape','960','540','--verify-alignment','--dice-only'),@('FinalActivities390','390','844','--verify-alignment','--activities-only'),@('FinalActivitiesLandscape','960','540','--verify-alignment','--activities-only'))
} else {
    @(@('GuardStore390','390','844','--verify-alignment','--purchases-only'),@('GuardDice540','540','960','--verify-alignment','--dice-only'),@('GuardActivities390','390','844','--verify-alignment','--activities-only'),@('GuardLocale390','390','844','--verify-rebuild','--verify-localization'))
}
if($Group -eq 'stores'){$cases=$cases[0..2]}
foreach($case in $cases) {
    $runName=if($Group -eq 'guards'){$case[0]}else{$case[0].Replace('Final',$Prefix)}
    $runPath=Join-Path $fixRoot $runName
    New-Item -ItemType Directory -Force -Path $runPath | Out-Null
    $caseArguments=@($case[3],$runPath,$case[4],'-screen-fullscreen','0','-screen-width',$case[1],'-screen-height',$case[2],'-logFile',(Join-Path $runPath 'player.log'))
    if($RecordMotion -and $case[0] -eq 'FinalStore390'){$caseArguments+='--record-shop-motion'}
    $fixProc=Start-Process -FilePath $Player -ArgumentList $caseArguments -WindowStyle Hidden -PassThru
    Write-Output ($runName+' PID='+$fixProc.Id)
    $fixProc.WaitForExit()
    Write-Output ($runName+' exit='+$fixProc.ExitCode)
}
