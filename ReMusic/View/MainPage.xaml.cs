using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using ReMusic.ViewModel;
using GalaSoft.MvvmLight.Messaging;
using Windows.UI;
using ReMusic.UserControls;
using ReMusic.BaseServices;

//“空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 上有介绍

namespace ReMusic
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {

        public MainViewModel Main => (MainViewModel)DataContext;
        public MainPage()
        {
            this.InitializeComponent();
            InitializeMessenger();

        }

        public void InitializeMessenger()
        {
            Messenger.Default.Register<string>(this, "MainPage", OpenSettingView); //注册一个主页消息
        }
        private void OpenSettingView(string msg)
        {
            if (msg != "OpenSettingView")
                return;
            SettingsFlyout settings = new SettingsFlyout();
            settings.HeaderBackground = new SolidColorBrush(Colors.Orange);
            settings.Content = new SettingControl();
            //显示设置面板
            settings.Show();
        }

        /// <summary>
        /// 当此页面即将显示在框架中时调用。
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ApplicationSettingsHelper.LoadSetttingFromLocalSettings(ApplicationSettingsConstants.AppState, AppState.Active.ToString());//本地配置文件加载并设置为激活状态
        }

        private void Hambeger_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            MySplitView.IsPaneOpen = !MySplitView.IsPaneOpen;
        }
    }
}
