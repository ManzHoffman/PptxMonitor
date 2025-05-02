using System;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Threading;
using System.Collections.Generic;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using File = Google.Apis.Drive.v3.Data.File;

class Program
{
    static string localCopyDir = @"C:\CopiedPresentations";
    static string[] pptExtensions = { ".pptx", ".ppt", ".ppsx", ".pps", ".potx", ".pot" };

    static void Main()
    {
        if (!Directory.Exists(localCopyDir))
            Directory.CreateDirectory(localCopyDir);

        Console.WriteLine("Monitoring PowerPoint file openings...");

        while (true)
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher(
                    "SELECT ProcessId, Name, CommandLine FROM Win32_Process WHERE Name = 'POWERPNT.EXE'"))
                {
                    foreach (ManagementObject proc in searcher.Get())
                    {
                        string cmdLine = proc["CommandLine"]?.ToString();
                        if (!string.IsNullOrWhiteSpace(cmdLine))
                        {
                            foreach (string part in cmdLine.Split('\"'))
                            {
                                string ext = Path.GetExtension(part)?.ToLower();
                                if (Array.Exists(pptExtensions, e => e == ext) && System.IO.File.Exists(part))
                                {
                                    string fileName = Path.GetFileName(part);
                                    string localPath = Path.Combine(localCopyDir, fileName);

                                    if (!System.IO.File.Exists(localPath))
                                    {
                                        System.IO.File.Copy(part, localPath);
                                        Console.WriteLine($"Copied: {fileName}");

                                        UploadToDrive(localPath, fileName);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            Thread.Sleep(5000);
        }
    }

    static void ListFilesFromServiceAccount()
{
    try
    {
        GoogleCredential credential;
        using (var stream = new FileStream("service_account.json", FileMode.Open, FileAccess.Read))
        {
            credential = GoogleCredential.FromStream(stream)
                .CreateScoped(DriveService.Scope.DriveFile);
        }

        var service = new DriveService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = "PptxMonitorUploader"
        });

        var request = service.Files.List();
        request.Fields = "files(id, name, parents)";
        var result = request.Execute();

        Console.WriteLine("📄 Files visible to the service account:");
        foreach (var file in result.Files)
        {
            Console.WriteLine($"- {file.Name} (ID: {file.Id}) | Parents: {string.Join(",", file.Parents ?? new List<string>())}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Failed to list files: {ex.Message}");
    }
}

static void UploadToDrive(string localFilePath, string remoteFileName)
{
    string ApplicationName = "PptxMonitorUploader";

    // Google Drive folder ID
    string folderId = "1bK3SNwzJzI_kVh6ZPZOWM_1hZe0QHIoT";

    try
    {
        Console.WriteLine("🔄 Starting upload to Google Drive...");

        GoogleCredential credential;
        using (var stream = new FileStream("service_account.json", FileMode.Open, FileAccess.Read))
        {
            credential = GoogleCredential.FromStream(stream)
                .CreateScoped(DriveService.Scope.DriveFile);
        }

        var service = new DriveService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = ApplicationName,
        });

        Console.WriteLine("🔐 Authenticated with service account.");
        Console.WriteLine($"📂 Target folder ID: {folderId}");

        var fileMetadata = new File()
        {
            Name = remoteFileName,
            Parents = new List<string> { folderId }
        };

        using (var fs = new FileStream(localFilePath, FileMode.Open))
        {
            var request = service.Files.Create(fileMetadata, fs,
                "application/vnd.openxmlformats-officedocument.presentationml.presentation");

            request.Fields = "id";

            Console.WriteLine($"📤 Uploading file: {remoteFileName}...");
            request.Upload();

            var uploadedFile = request.ResponseBody;

            if (uploadedFile != null && !string.IsNullOrEmpty(uploadedFile.Id))
            {
                Console.WriteLine($"✅ Uploaded: https://drive.google.com/file/d/{uploadedFile.Id}/view");
            }
            else
            {
                Console.WriteLine("⚠️ Upload completed but returned null or empty response.");
            }

            //ListFilesFromServiceAccount();

        }

        Console.WriteLine($"✅ Upload process complete for: {remoteFileName}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Drive Upload Failed: {ex.GetType().Name} - {ex.Message}");
        Console.WriteLine($"📄 StackTrace:\n{ex.StackTrace}");
    }
}

}
