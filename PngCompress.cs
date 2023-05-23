using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Editor
{
    public class PngCompress : EditorWindow
    {
        private readonly GUIContent _imageTypeContent = new("Media Type:");
        private readonly GUIContent _actionTypeContent = new("Features:");
        private readonly GUIContent _confirmTypeContent = new("Add Filter:");
        private readonly GUIContent _pythonVersionContent = new("Python Version:");

        private ImageType _imageType = ImageType.All;
        private ActionType _actionType = ActionType.All;
        private ExecCondition _execCondition = ExecCondition.NoFilters;
#if UNITY_EDITOR_OSX
        private PythonVersion _pythonVersion = PythonVersion.Python3;
#else
        private PythonVersion _pythonVersion = PythonVersion.Python2;
#endif


        private string selectedFolderPath;
        private Vector2 scrollPosition;
        private string textContent;
        private bool isRunningProcess;
        private string inputImagesFolder;

        private int _scanLargeWidth = 128;
        private int _scanLargeHeight = 128;
        private int _conditionMinWidth = 128;
        private int _conditionMinHeight = 128;
        private int _outputWidth = 128;
        private int _outputHeight = 128;
        private int _outputQuality = 100;

        private enum PythonVersion
        {
            Python3,
            Python2
        }

        private enum ImageType
        {
            All,
            PNG,
            JPG,
            TGA
        }

        private enum ActionType
        {
            ScanOnly,
            ResizeAndCompress,
            ResizeOnly,
            CompressOnly
        }

        private enum ExecCondition
        {
            NoFilters,
            SizeFilter
        }

        [MenuItem("Tools/PngKnife")]
        private static void ShowWindow()
        {
            var window = GetWindow<PngCompress>();
            window.titleContent = new GUIContent("PngKnife");
            window.Show();
            window.minSize = new Vector2(480, 480);
        }

        private string OpenFolderDialog()
        {
            return EditorUtility.OpenFolderPanel("Please choose a directory...", "", "");
        }

        private void ShowToast(string message)
        {
            ShowNotification(new GUIContent(message));
        }

        private async void CopyDir(string sourceDirName, string destDirName, bool copySubDirs)
        {
            isRunningProcess = true;
            await Task.Run(delegate { DirectoryCopy(sourceDirName, destDirName, copySubDirs); });
            AppendLog("Backup Success!" + destDirName);
            inputImagesFolder = destDirName;
            isRunningProcess = false;
        }

        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();

            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string tempPath = Path.Combine(destDirName, file.Name);
                file.CopyTo(tempPath, true);
            }

            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string tempPath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, tempPath, copySubDirs);
                }
            }
        }


        private void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            GUILayout.Space(10f);
            GUILayout.Label(Tips);
            GUILayout.Space(10f);
            if (GUILayout.Button("Choose the images folder"))
            {
                if (isRunningProcess)
                {
                    ShowToast("Waiting...");
                    return;
                }

                selectedFolderPath = OpenFolderDialog();
            }

            if (!string.IsNullOrEmpty(selectedFolderPath))
            {
                GUILayout.Label(selectedFolderPath);
            }

            GUILayout.Space(10f);
            if (GUILayout.Button("Backup Firstly"))
            {
                if (isRunningProcess)
                {
                    ShowToast("Waiting...");
                    return;
                }

                var backupPath = OpenFolderDialog();
                if (!string.IsNullOrEmpty(backupPath))
                {
                    if (backupPath.Equals(selectedFolderPath))
                    {
                        ShowToast("Invalid path.");
                    }
                    else
                    {
                        AppendLog("Waiting...");
                        DirectoryInfo directoryInfo = new DirectoryInfo(selectedFolderPath);
                        var dirName = directoryInfo.Name;
                        var toDir = Path.Combine(backupPath, dirName);
                        CopyDir(selectedFolderPath, toDir, true);
                    }
                }
            }

            if (!string.IsNullOrEmpty(inputImagesFolder))
            {
                GUILayout.Label(inputImagesFolder);
            }

            GUILayout.Space(20f);
            _pythonVersion = (PythonVersion)EditorGUILayout.EnumPopup(_pythonVersionContent, _pythonVersion);
            GUILayout.Space(4f);
            _imageType = (ImageType)EditorGUILayout.EnumPopup(_imageTypeContent, _imageType);
            GUILayout.Space(4f);
            _actionType = (ActionType)EditorGUILayout.EnumPopup(_actionTypeContent, _actionType);
            if (_actionType == ActionType.ScanOnly)
            {
                _scanLargeWidth = EditorGUILayout.IntField("> width:", _scanLargeWidth);
                _scanLargeHeight = EditorGUILayout.IntField("> height:", _scanLargeHeight);
            }
            else
            {
                switch (_actionType)
                {
                    case ActionType.ResizeOnly:
                        _outputWidth = EditorGUILayout.IntField("Output width:", _outputWidth);
                        _outputHeight = EditorGUILayout.IntField("Output height:", _outputHeight);
                        break;
                    case ActionType.CompressOnly:
                        _outputQuality = EditorGUILayout.IntField("Quality(0-100):", _outputQuality);
                        break;
                    case ActionType.ResizeAndCompress:
                        _outputWidth = EditorGUILayout.IntField("Output width:", _outputWidth);
                        _outputHeight = EditorGUILayout.IntField("Output height:", _outputHeight);
                        _outputQuality = EditorGUILayout.IntField("Quality(0-100):", _outputQuality);
                        break;
                }

                GUILayout.Space(4f);
                _execCondition = (ExecCondition)EditorGUILayout.EnumPopup(_confirmTypeContent, _execCondition);
                if (_execCondition == ExecCondition.SizeFilter)
                {
                    _conditionMinWidth = EditorGUILayout.IntField("> width:", _conditionMinWidth);
                    _conditionMinHeight = EditorGUILayout.IntField("> height:", _conditionMinHeight);
                }

                _outputWidth = Math.Max(1, _outputWidth);
                _outputHeight = Math.Max(1, _outputHeight);
                _outputQuality = Math.Clamp(_outputQuality, 0, 100);
            }

            GUILayout.Space(20f);
            if (GUILayout.Button("Installing libraries"))
            {
                textContent = "";
                if (isRunningProcess)
                {
                    ShowToast("Waiting...");
                    return;
                }

                CheckPythonEnv();
            }

            if (GUILayout.Button("Execute"))
            {
                if (isRunningProcess)
                {
                    ShowToast("Waiting...");
                    return;
                }

                textContent = "";
                if (CheckPass())
                {
                    DoAction();
                }
            }

            GUILayout.Space(4f);
            if (GUILayout.Button("Clear logs"))
            {
                if (isRunningProcess)
                {
                    ShowToast("Waiting...");
                    return;
                }

                textContent = "";
            }

            GUILayout.Space(10f);
            GUILayout.Label("Logs:");
            GUILayout.Space(4f);
            if (!string.IsNullOrEmpty(textContent))
            {
                scrollPosition = GUILayout.BeginScrollView(scrollPosition);
                textContent = GUILayout.TextArea(textContent, GUILayout.ExpandHeight(true));
                GUILayout.EndScrollView();
            }

            EditorGUILayout.EndVertical();
        }

        private async void CheckPythonEnv()
        {
            await InstallPILOnMacOS();
        }

        private async void DoAction()
        {
            await RunPythonScript();
        }

        private bool CheckPass()
        {
            if (string.IsNullOrEmpty(selectedFolderPath))
            {
                ShowToast("Invalid path.");
                return false;
            }

            return true;
        }

        private async Task RunPythonScript()
        {
            string pythonScriptPath = Application.dataPath + "/Python/pngcompress.py";
            if (File.Exists(pythonScriptPath))
            {
                isRunningProcess = true;
                await Task.Run(() =>
                {
                    AppendLog("Executing ...");
                    try
                    {
                        using Process process = new Process();
#if UNITY_EDITOR_OSX
                        SetMacProcess(process, pythonScriptPath);
#else
                        SetWindowsProcess(process,pythonScriptPath);
#endif
                        process.StartInfo.UseShellExecute = false;
                        process.StartInfo.RedirectStandardOutput = true;
                        process.StartInfo.RedirectStandardError = true;
                        process.StartInfo.CreateNoWindow = true;
                        process.Start();
                        process.BeginOutputReadLine();
                        process.BeginErrorReadLine();
                        process.OutputDataReceived += OutReceiveData;
                        process.ErrorDataReceived += ProcessOnErrorDataReceived;
                        process.WaitForExit();
                        process.Dispose();
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
                    }
                    finally
                    {
                        isRunningProcess = false;
                        AppendLog("Complete!");
                    }
                });
            }
            else
            {
                ShowToast("Execute fail.");
            }
        }

        private void UpdateCommand(StringBuilder command)
        {
            string inputDir = inputImagesFolder;
            if (string.IsNullOrEmpty(inputDir))
            {
                inputDir = selectedFolderPath;
            }

            string mediaType = "-a";
            switch (_imageType)
            {
                case ImageType.All:
                    break;
                case ImageType.JPG:
                    mediaType = "jpg";
                    break;
                case ImageType.PNG:
                    mediaType = "png";
                    break;
                case ImageType.TGA:
                    mediaType = "tga";
                    break;
            }

            switch (_actionType)
            {
                case ActionType.ResizeOnly:
                case ActionType.ResizeAndCompress:
                    if (_actionType == ActionType.ResizeOnly)
                    {
                        _outputQuality = 100;
                    }

                    command.Append(mediaType)
                        .Append(" ")
                        .Append(inputDir)
                        .Append(" ")
                        .Append("-s")
                        .Append(" ")
                        .Append(_outputWidth)
                        .Append(" ")
                        .Append(_outputHeight)
                        .Append(" ")
                        .Append("-q")
                        .Append(" ")
                        .Append(_outputQuality);
                    break;
                case ActionType.CompressOnly:
                    command.Append(mediaType)
                        .Append(" ")
                        .Append(inputDir)
                        .Append(" ")
                        .Append("-q")
                        .Append(" ")
                        .Append(_outputQuality);
                    break;
                case ActionType.ScanOnly:
                    command.Append("scan")
                        .Append(" ")
                        .Append(inputDir)
                        .Append(" ")
                        .Append(_scanLargeWidth)
                        .Append(" ")
                        .Append(_scanLargeHeight);
                    break;
            }

            if (_actionType != ActionType.ScanOnly)
            {
                switch (_execCondition)
                {
                    case ExecCondition.SizeFilter:
                        command.Append(" ")
                            .Append("-w")
                            .Append(" ")
                            .Append(_conditionMinWidth)
                            .Append(" ")
                            .Append(_conditionMinHeight);
                        break;
                    default:
                        command.Append(" ")
                            .Append("-w")
                            .Append(" ")
                            .Append(0)
                            .Append(" ")
                            .Append(0);
                        break;
                }
            }
        }

        private void SetMacProcess(Process process, string pythonScriptPath)
        {
            StringBuilder command = new StringBuilder();
            switch (_pythonVersion)
            {
                case PythonVersion.Python2:
                    command.Append("python");
                    break;
                case PythonVersion.Python3:
                    command.Append("python3");
                    break;
            }

            command.Append(" ").Append(pythonScriptPath).Append(" ");
            UpdateCommand(command);
            process.StartInfo.FileName = "/bin/bash";
            process.StartInfo.Arguments = $"-c \"{command}\"";
        }

        private void SetWindowsProcess(Process process, string pythonScriptPath)
        {
            StringBuilder command = new StringBuilder();
            command.Append(pythonScriptPath).Append(" ");
            UpdateCommand(command);
            switch (_pythonVersion)
            {
                case PythonVersion.Python2:
                    process.StartInfo.FileName = "python";
                    break;
                case PythonVersion.Python3:
                    process.StartInfo.FileName = "python3";
                    break;
            }

            process.StartInfo.Arguments = command.ToString();
        }

        private void ProcessOnErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            AppendLog(e.Data);
        }

        private void AppendLog(string log)
        {
            textContent += log + "\n";
        }

        private async Task InstallPILOnMacOS()
        {
            AppendLog("Installing ...");
            isRunningProcess = true;
            await Task.Run(() =>
            {
                using Process process = new Process();
#if UNITY_EDITOR_OSX
                process.StartInfo.FileName = "/bin/bash";
                switch (_pythonVersion)
                {
                    case PythonVersion.Python2:
                        process.StartInfo.Arguments = "-c \"pip install pillow\"";
                        break;
                    case PythonVersion.Python3:
                        process.StartInfo.Arguments = "-c \"pip3 install pillow\"";
                        break;
                }
#else
                process.StartInfo.FileName = "python.exe";
                process.StartInfo.Arguments = "-m pip install pillow";
#endif

                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.CreateNoWindow = true;
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.OutputDataReceived += OutReceiveData;
                process.WaitForExit();
                process.Dispose();
                isRunningProcess = false;
                AppendLog("Install Successfully !");
            });
        }

        private void OutReceiveData(object sender, DataReceivedEventArgs e)
        {
            AppendLog(e.Data);
        }
    }
}
