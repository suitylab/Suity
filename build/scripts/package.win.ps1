Remove-Item -Path build\Suity.Agentic\*.pdb -Force
Compress-Archive -Path build\Suity.Agentic -DestinationPath "build\suity-agentic_${env:VERSION}.${env:RUNTIME}.zip" -Force