"%ProgramFiles%\Microsoft SDKs\Windows\v6.0A\bin\TlbExp.exe" %1
echo return code=%ERRORLEVEL%

echo Registering assembly...
rem "%windir%\Microsoft.NET\Framework\v2.0.50727\regasm.exe" /codebase %1
"%windir%\Microsoft.NET\Framework\v2.0.50727\regasm.exe" %1
echo return code=%ERRORLEVEL%

echo Adding registry keys...
reg add HKLM\SOFTWARE\Microsoft\VisualStudio\9.0\Generators\{FAE04EC1-301F-11d3-BF4B-00C04F79EFBC}\CSharpMacroExpander /f
reg add HKLM\SOFTWARE\Microsoft\VisualStudio\9.0\Generators\{FAE04EC1-301F-11d3-BF4B-00C04F79EFBC}\CSharpMacroExpander /ve /d "CSharp Macros code generator" /f
if ERRORLEVEL 1 echo return code=%ERRORLEVEL%
reg add HKLM\SOFTWARE\Microsoft\VisualStudio\9.0\Generators\{FAE04EC1-301F-11d3-BF4B-00C04F79EFBC}\CSharpMacroExpander /v CLSID /d {03EEAE74-3099-4b77-BAB1-DD6693DFC88F} /f
if ERRORLEVEL 1 echo return code=%ERRORLEVEL%
reg add HKLM\SOFTWARE\Microsoft\VisualStudio\9.0\Generators\{FAE04EC1-301F-11d3-BF4B-00C04F79EFBC}\CSharpMacroExpander /v GeneratesDesignTimeSource /t REG_DWORD /d 1 /f
if ERRORLEVEL 1 echo return code=%ERRORLEVEL%

echo Copying grammar.xml...
if not exist %2\CSharpGrammar.xml (
	%3\GrammarGenerator.exe
	copy %3\CSharpGrammar.xml $(TargetDir)
)

echo Done.