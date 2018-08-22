using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsHider
{
    public partial class MainForm : Form
    {
        private readonly Dictionary<IntPtr, string> _hideforms = new Dictionary<IntPtr, string>();

        private short _ctrlShiftH;
        private IntPtr _lastHander;
   
        public MainForm()
        {
            InitializeComponent();
        }

        private void HideWindow()
        {
            var hWnd = Win32Api.GetForegroundWindow();
            if (!Win32Api.ShowWindow(hWnd, Win32Api.SW_HIDE))
            {
                return;
            }
            var length = Win32Api.GetWindowTextLength(hWnd);
            var sb = new StringBuilder(length + 1);
            Win32Api.GetWindowText(hWnd, sb, sb.Capacity);
            _hideforms.Add(hWnd, sb.ToString());
        }

        private void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                //还原窗体显示    
                WindowState = FormWindowState.Normal;
                //激活窗体并给予它焦点
                this.Activate();
                //任务栏区显示图标
                this.ShowInTaskbar = true;
                //托盘区图标隐藏
                ((NotifyIcon) sender).Visible = false;

                ReRegisterHideKey();
            }
        }

        private void MainForm_SizeChanged(object sender, EventArgs e)
        {
            //判断是否选择的是最小化按钮
            if (WindowState == FormWindowState.Minimized)
            {
                //隐藏任务栏区图标 
                //会导致窗体句柄改变
                this.ShowInTaskbar = false;
                //图标显示在托盘区
                notifyIcon.Visible = true;

                ReRegisterHideKey();
            }
        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case HotKey.WM_HOTKEY:
                    ProcessHotkey(m);
                    break;
                default:
                    break;
            }
            base.WndProc(ref m);
        }

        private void ProcessHotkey(Message m)
        {
            var sid = m.WParam.ToInt32();

            if (_ctrlShiftH == sid)
            {
                System.Diagnostics.Debug.WriteLine("按下Alt+S");
                HideWindow();
            }
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            UnRegisterHideKey();

            foreach (var form in _hideforms)
            {
                Win32Api.ShowWindow(form.Key, Win32Api.SW_SHOW);
            }
            _hideforms.Clear();
        }

        private void tsmiShow_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Normal;
        }

        private void tsmiExit_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(@"是否确认退出程序？", @"退出", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK)
            {
                return;
            }
            // 关闭所有的线程
            this.Dispose();
            this.Close();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            RegisterHideKey();
        }

        private void RegisterHideKey()
        {
            //隐藏
            _ctrlShiftH = HotKey.GlobalAddAtom("Ctrl-shift-H");
            var reg = HotKey.RegisterHotKey(Handle, _ctrlShiftH, HotKey.KeyModifiers.Ctrl | HotKey.KeyModifiers.Shift, Keys.H);
            _lastHander = Handle;
            System.Diagnostics.Debug.WriteLine(reg ? "RegisterHotKey success!" : "RegisterHotKey fail!");
        }

        private void UnRegisterHideKey()
        {
            if (_ctrlShiftH == 0) return;
            var reg = HotKey.UnregisterHotKey(_lastHander, _ctrlShiftH);
            if (!reg)
            {
                var err = Win32Api.GetLastError();
                System.Diagnostics.Debug.WriteLine("LastError {0}", err);
                if (err == 1400)
                {
                    System.Diagnostics.Debug.WriteLine(@"无效的窗口句柄");
                }
            }
            System.Diagnostics.Debug.WriteLine(reg ? "UnRegisterHotKey success!" : "UnRegisterHotKey fail!");
            HotKey.GlobalDeleteAtom(_ctrlShiftH);
        }

        private void ReRegisterHideKey()
        {
            UnRegisterHideKey();
            RegisterHideKey();
        }
    }
}
