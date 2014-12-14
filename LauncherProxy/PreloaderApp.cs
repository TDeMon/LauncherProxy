using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Resources;

namespace LauncherProxy
{
    public partial class PreloaderApp : Form
    {
        /// <summary>
        /// Путь к текущей сборке.
        /// </summary>
        string ExecutingAssembly = null;

        /// <summary>
        /// Аргументы запуска приложения
        /// </summary>
        string[] _startArgs = null;

        /// <summary>
        /// Версия текущего загруженного приложения
        /// </summary>
        Version CurrentVersion;

        /// <summary>
        /// Основной конструктор класса
        /// </summary>
        /// <param name="startArgs">аргументы для передачи в запускаемое приложение</param>
        public PreloaderApp(params string[] startArgs)
        {
            InitializeComponent();

            this.Visible = false;
            //this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;

            _startArgs = startArgs;

            this.Visible = false;            
            //timer1.Interval = 5 * 60 * 1000;
            //timer1.Interval = 1 * 60 * 1000;
            timer1.Interval = 30 * 1000;
            timer1.Enabled = true;



            ExecutingAssembly = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            Application.ApplicationExit += Application_ApplicationExit;

            LoadMainApp();


        }

        void Application_ApplicationExit(object sender, EventArgs e)
        {
            if (CurrentProc!=null)
            {
                if (!CurrentProc.HasExited)
                {
                    CurrentProc.Kill();
                    CurrentProc.WaitForExit(30 * 1000);
                }
            }
        }


        ResourceManager LocRM = new ResourceManager("LauncherProxy.LauncherProxyStrings", typeof(PreloaderApp).Assembly);


        /// <summary>
        /// Загрузка основного приложения
        /// </summary>
        public void LoadMainApp()
        {
            var pathes = GetAvailablePathes();
            if (pathes.Count() > 0)
            {
                var maxVersion = pathes.Keys.Max();

                StartNewInstance(maxVersion, pathes[maxVersion]);
            }
            else
            {
                MessageBox.Show(LocRM.GetString("MessageBoxExecutableAbsentText"), LocRM.GetString("MessageBoxExecutableAbsentCaption"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                //this.Close();
                Application.Exit();
                return;
            }

        }

        /// <summary>
        /// Получить список версий-путей к исполняемым файлам приложения
        /// </summary>
        /// <returns>Словарь путей исполняемых файлов</returns>
        private Dictionary<Version, string> GetAvailablePathes()
        {
            var dirNames = System.IO.Directory.GetDirectories(ExecutingAssembly);

            var pathes = new Dictionary<Version, string>();

            foreach (var name in dirNames)
            {
                Version v = null;                
                if (Version.TryParse(System.IO.Path.GetFileName(name), out v))
                {
                    var fileNames = System.IO.Directory.GetFiles(name, LocRM.GetString("ExternalAppPath"));
                    if (fileNames != null && fileNames.Count() == 1)
                    {
                        pathes.Add(v, fileNames[0]);
                    }
                }
            }

            return pathes;
        }

        /// <summary>
        /// Текущий запущеный процесс
        /// </summary>
        System.Diagnostics.Process CurrentProc = null;

        /// <summary>
        /// Запускает новый экземпляр приложения
        /// </summary>
        /// <param name="ver">верся нового экземпляра</param>
        /// <param name="path">путь к исполняемому файлу с новой версией</param>
        private void StartNewInstance(Version ver, string path)
        {
            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.StartInfo = new System.Diagnostics.ProcessStartInfo(path, string.Join(" ", _startArgs));
            proc.StartInfo.WorkingDirectory = System.IO.Path.GetDirectoryName(path);
            proc.StartInfo.UseShellExecute = false;
            proc.EnableRaisingEvents = true;
            proc.Exited += proc_Exited;

            //System.Diagnostics.Process oldProc = null;
            if (CurrentProc!=null)
            {
                //oldProc = CurrentProc;
                CurrentProc.EnableRaisingEvents = false;
                CurrentProc.Kill();
                CurrentProc.WaitForExit(30 * 1000);
                NextCheckAllowed = true;
            }
            CurrentProc = proc;
            

            CurrentVersion = ver;
            proc.Start();
            //if (oldProc !=null)
            //{
            //    oldProc.Kill();
            //}
            
        }

        /// <summary>
        /// Событие завершения запускаемого приложения - должно вызвать завершение текущего приложения.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void proc_Exited(object sender, EventArgs e)
        {
            Application.Exit();
        }


        /// <summary>
        /// Флаг, разрешающий новую проверку доступных версий.
        /// </summary>
        bool NextCheckAllowed = true;
        //KeyValuePair<Version, string> AvailableVersionInfo;

        /// <summary>
        /// Проверка доступных обновлений
        /// </summary>
        public void CheckForUpdates()
        {
            if (NextCheckAllowed)
            {
                var pathes = GetAvailablePathes();
                if (pathes != null && pathes.Count > 0)
                {
                    var maxVersion = pathes.Keys.Max();
                    if (CurrentVersion < maxVersion)
                    {
                        //доступна новая версия - спросить пользователя.                    
                        var rslt = MessageBox.Show(LocRM.GetString("MessageBoxUpdateAvailableText"), LocRM.GetString("MessageBoxUpdateAvailableCaption") + " " + maxVersion.ToString(), MessageBoxButtons.YesNo, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                        //MessageBox.Show("", "", null, null, null, MessageBoxOptions.)
                        if (rslt == System.Windows.Forms.DialogResult.Yes)
                        {
                            StartNewInstance(maxVersion, pathes[maxVersion]);
                        }
                        else
                        {
                            ShowNotifyPopup = true;
                            NextCheckAllowed = false;
                        }

                    }
                }
            }
        }

        private Form NotifyPopup;
        private bool _showNotifyPopup;
        private bool ShowNotifyPopup
        {
            get
            {
                return _showNotifyPopup;
            }
            set
            {
                if (_showNotifyPopup != value)
                {
                    _showNotifyPopup = value;
                    if (_showNotifyPopup)
                    {
                        if (NotifyPopup == null)
                        {
                            NotifyPopup = new NotifyForm(LoadMainApp);
                        }
                        NotifyPopup.Show();
                    }
                    else
                    {
                        if (NotifyPopup != null)
                        {
                            NotifyPopup.Hide();
                        }
                    }
                }
            }
        }


        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                timer1.Enabled = false;
                if (CurrentProc != null)
                {
                    if (CurrentProc.HasExited)
                    {
                        Application.Exit();
                        return;
                    }
                }

                CheckForUpdates();
            }
            finally
            {                
                timer1.Enabled = true;
            }
        }

        private void PreloaderApp_Load(object sender, EventArgs e)
        {

        }
    }
}
