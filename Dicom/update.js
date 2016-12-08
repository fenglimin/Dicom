
var fso = new ActiveXObject("Scripting.FileSystemObject");
var shell = WScript.CreateObject("WScript.Shell");

var drives = new Enumerator(fso.Drives);
for (; !drives.atEnd(); drives.moveNext())
{
    var drive = drives.item();
    var folder = drive.Path + "\\Tools\\Dicom";

    if (drive.DriveType == 1 && fso.FolderExists(folder))
    {
        shell.Run("%comspec% /c copy /Y DicomToolKit\\bin\\Debug\\EK.Capture.Dicom.DicomToolKit.dll " + folder, 0, true);
        shell.Run("%comspec% /c copy /Y Tools\\DicomEcho\\bin\\Debug\\DicomEcho.exe " + folder, 0, true);
        shell.Run("%comspec% /c copy /Y Tools\\DicomEditor\\bin\\Debug\\DicomEditor.exe " + folder, 0, true);
        shell.Run("%comspec% /c copy /Y Tools\\DicomTouch\\bin\\Debug\\DicomTouch.exe " + folder, 0, true);
        shell.Run("%comspec% /c copy /Y Tools\\DicomViewer\\bin\\Debug\\DicomViewer.exe " + folder, 0, true);
        shell.Run("%comspec% /c copy /Y Tools\\DicomDiff\\bin\\Debug\\DicomDiff.exe " + folder, 0, true);
        shell.Run("%comspec% /c copy /Y Tools\\DicomTags\\bin\\Debug\\DicomTags.exe " + folder, 0, true);
        shell.Run("%comspec% /c copy /Y Tools\\DicomExplorer\\bin\\Debug\\DicomExplorer.exe " + folder, 0, true);
        shell.Run("%comspec% /c copy /Y Tools\\DicomPipe\\bin\\Debug\\DicomPipe.exe " + folder, 0, true);
        shell.Run("%comspec% /c copy /Y Tools\\DicomPipe\\Samples\\*.* " + folder, 0, true);
    }
}
