# 📄 PptxMonitorUploader

A lightweight Windows background app that monitors PowerPoint presentations being opened on a machine, copies the files locally, and silently uploads them to a shared **Google Drive folder** using a **service account** — with no user interaction required.

---

## ✅ Features

- 🎯 Monitors `.pptx`, `.ppt`, `.ppsx`, `.pps`, `.potx`, and `.pot` files opened via PowerPoint
- 📁 Copies detected files to a secure local folder
- ☁️ Uploads each file to a pre-configured **Google Drive** folder
- 🔐 Uses a **Google service account** — no login prompts or browser windows
- 🧼 Runs silently in the background (only visible in Task Manager)
- 💼 Ideal for **conference**,**meeting**, **hospital**, **corporate**, or **kiosk** environments

---

## 🚀 How It Works

1. The app watches for PowerPoint (`POWERPNT.EXE`) processes and extracts opened file paths.
2. Each new file is copied to a folder like `C:\CopiedPresentations`.
3. Files are then uploaded to a specific Google Drive folder using a service account key.
4. The service account needs access to the target Drive folder (via "Share with...").

---

## 🛠 Requirements

- [.NET 6.0+ SDK](https://dotnet.microsoft.com/download)
- Windows OS
- A Google Cloud project with:
  - ✅ Google Drive API enabled
  - ✅ A service account with a `.json` key
  - ✅ Access to the shared Drive folder

---

## 🔐 Setup Instructions

1. Clone this repository
2. Place your `service_account.json` file in the output folder
3. Update the Drive **folder ID** inside `UploadToDrive()` in `Program.cs`
4. Share the target Google Drive folder with the service account email
5. Build and publish:

   ```bash
   dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true
