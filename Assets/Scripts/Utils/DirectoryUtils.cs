using System;
using System.IO;

public static class DirectoryUtils
{
    public static bool IsFolderNameValid(string folderName)
    {
        if (string.IsNullOrWhiteSpace(folderName))
            return false;

        var invalidChars = Path.GetInvalidFileNameChars();
        if (folderName.IndexOfAny(invalidChars) >= 0)
            return false;

        var tempParentDir = Path.Combine(Path.GetTempPath(), "FolderValidation_" + Guid.NewGuid().ToString("N"));
        var testFolderPath = Path.Combine(tempParentDir, folderName);

        try {
            var dir = Directory.CreateDirectory(testFolderPath);

            return true;
        }
        catch {
            return false;
        }
        finally {
            try {
                if (Directory.Exists(tempParentDir)) {
                    Directory.Delete(tempParentDir, true);
                }
            }
            catch {

            }
        }
    }
}