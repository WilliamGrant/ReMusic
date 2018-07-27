using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;
using Windows.Storage;

namespace ReMusic.BaseServices
{
    public static class ApplicationSettingsHelper
    {
        /// <summary>
        /// 加载应用本地数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">本地数据键值名</param>
        /// <param name="defaultValue">键值</param>
        /// <returns></returns>
        public static T LoadSetttingFromLocalSettings<T>(string key, T defaultValue)
        {
            T obj = default(T);
            try
            {
                if (ApplicationData.Current.LocalSettings.Values.ContainsKey(key))
                {
                    obj = (T)ApplicationData.Current.LocalSettings.Values[key];
                }
            }
            catch (Exception)
            {
                return default(T);
            }
            return obj;
        }

        /// <summary>
        /// 获取存储数值并删除占用的存储空间
        /// </summary>
        /// <param name="key">键值名</param>
        /// <returns></returns>
        public static object ReadResetSettingsValue(string key)
        {
            Debug.WriteLine(key);
            if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(key))
            {
                Debug.WriteLine("null, returned");
                return null;
            }
            else
            {
                var value = ApplicationData.Current.LocalSettings.Values[key];
                ApplicationData.Current.LocalSettings.Values.Remove(key);
                Debug.WriteLine("value is found");
                return value;
            }
        }


        /// <summary>
        /// 保存对应键值对的信息到本地（原本存在对应键值名，则更新键值，否则添加）
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void SaveSettingToLocalSettings(string key, object value)
        {
            try
            {
                if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(key))
                {
                    ApplicationData.Current.LocalSettings.Values.Add(key, value);
                }
                else
                {
                    ApplicationData.Current.LocalSettings.Values[key] = value;
                }
            }
            catch (Exception) { }
        }

        /// <summary>
        /// 是否存在对应
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool IsSettingExisited(string key)
        {
            return ApplicationData.Current.LocalSettings.Values.ContainsKey(key);
        }
    }
}
