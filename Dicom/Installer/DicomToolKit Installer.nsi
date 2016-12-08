; DicomToolKit Installer.nsi
;

; It will install DicomToolKit binaries
; into a directory that the user selects.

;--------------------------------

; The name of the installer
Name "DicomToolKit Installer"


; The file to write
!searchparse /file "..\DicomToolKit\Properties\AssemblyInfo.cs" '[assembly: AssemblyVersion("' VERSION '")]'
OutFile "DicomToolKit ${VERSION} Installer.exe"

; The default installation directory
InstallDir "C:\Program Files\Kodak\CaptureConsole\bin"

; Registry key to check for directory (so if you install again, it will 
; overwrite the old one automatically)
InstallDirRegKey HKLM "Software\DicomToolKit" "Install_Dir"

; Request application privileges for Windows Vista
RequestExecutionLevel admin

;--------------------------------

; Pages

Page components
Page directory
Page instfiles

UninstPage uninstConfirm
UninstPage instfiles

;--------------------------------

; The stuff to install
Section "DicomToolKit binaries (required)"

  SectionIn RO
  
  SetOutPath $INSTDIR

  File "..\DicomToolKit\bin\Release\EK.Capture.Dicom.DicomToolKit.dll"
  File "..\Tools\DicomEcho\bin\Release\DicomEcho.exe"
  File "..\Tools\DicomEditor\bin\Release\DicomEditor.exe"
  File "..\Tools\DicomTouch\bin\Release\DicomTouch.exe"
  File "..\Tools\DicomViewer\bin\Release\DicomViewer.exe"
  File "..\Tools\DicomDiff\bin\Release\DicomDiff.exe"
  File "..\Tools\DicomTags\bin\Release\DicomTags.exe"
  File "..\Tools\DicomPipe\bin\Release\DicomPipe.exe"

  File "..\Tools\DicomPipe\Samples\delay.cs"  
  File "..\Tools\DicomPipe\Samples\dump.cs"
  File "..\Tools\DicomPipe\Samples\example.cs"
  File "..\Tools\DicomPipe\Samples\readme.txt"

  ; Write the installation path into the registry
  WriteRegStr HKLM SOFTWARE\DicomToolKit "Install_Dir" "$INSTDIR"
  
  ; Write the uninstall keys for Windows
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\DicomToolKit" "DisplayName" "DicomToolKit Installer"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\DicomToolKit" "UninstallString" '"$INSTDIR\Uninstall DicomToolKit.exe"'
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\DicomToolKit" "NoModify" 1
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\DicomToolKit" "NoRepair" 1
  WriteUninstaller "Uninstall DicomToolKit.exe"

SectionEnd



;--------------------------------

; Uninstaller

Section "Uninstall"
  
  ; Remove registry keys
  DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\DicomToolKit"
  DeleteRegKey HKLM SOFTWARE\DicomToolKit

  ; Remove files and uninstaller

  Delete $INSTDIR\EK.Capture.Dicom.DicomToolKit.dll
  Delete $INSTDIR\DicomEcho.exe
  Delete $INSTDIR\DicomEditor.exe
  Delete $INSTDIR\DicomTouch.exe
  Delete $INSTDIR\DicomViewer.exe
  Delete $INSTDIR\DicomDiff.exe
  Delete $INSTDIR\DicomTags.exe
  Delete $INSTDIR\DicomPipe.exe

  Delete $INSTDIR\delay.cs
  Delete $INSTDIR\dump.cs
  Delete $INSTDIR\example.cs
  Delete $INSTDIR\readme.txt

  Delete $INSTDIR\dtk.log
  ;Delete $INSTDIR\*.settings.xml

  Delete "$INSTDIR\Uninstall DicomToolKit.exe"

  RMDIR $INSTDIR

SectionEnd
