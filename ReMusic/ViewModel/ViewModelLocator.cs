using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Practices.ServiceLocation;
using GalaSoft.MvvmLight.Ioc;
using Windows.UI.Xaml;
using GalaSoft.MvvmLight.Views;
using ReMusic.Services;

namespace ReMusic.ViewModel
{
    public class ViewModelLocator
    {
        private static ViewModelLocator _instance;
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);
            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<PlayerSessionService>(true);
        }

        public static ViewModelLocator Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = (Application.Current.Resources["locator"] as ViewModelLocator);
                }
                return _instance;
            }
        }

        public MainViewModel Main
        {
            get
            {
                return ServiceLocator.Current.GetInstance<MainViewModel>();
            }
        }

        private INavigationService CreateNavigationService()
        {
            var navigationService = new NavigationService();
            navigationService.Configure("MainPage", typeof(MainPage));
            return navigationService;
        }

        public static void Clearup()
        {
           //清理ViewModels
        }
    }

}
