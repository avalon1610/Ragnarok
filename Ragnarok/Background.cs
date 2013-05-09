using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ragnarok
{
    public partial class MainWindow
    {
        private void DoTask(Action myTask, Action<Task> callback)
        {
            Task.Factory.StartNew(myTask).ContinueWith(callback, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void DoTask(Func<string> function, Action<Task<string>> callback)
        {
            Task.Factory.StartNew<string>(function).ContinueWith(callback, TaskScheduler.FromCurrentSynchronizationContext());  
        }

        private void DoTask(Func<int> function,Action<Task<int>> callback)
        {
            Task.Factory.StartNew<int>(function).ContinueWith(callback,TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void doLogin()
        {
            string qq = this.QQ.Text;
            DoTask(() => { WEBQQ.Login(qq); }, task => { this.Login_Tab.IsSelected = true; });
            string pwd = this.Pwd.Password;
            DoTask(() => { return WEBQQ.PrintPwd(pwd); }, antecendent => { this.Login_Tab.Header = antecendent.Result; });
        }
    }


    class WEBQQ
    {
        public static string PrintPwd(string pwd)
        {
            return pwd;
        }

        public static WebClient wc = new WebClient();

        public static readonly string GET = "GET";
        public static readonly string PUT = "PUT";
        public static void getVerifyCode(string qq)
        {
            Random rand = new Random();
            string url = "http://check.ptlogin2.qq.com/check?uin=" + qq + "&appid=1003903&r=" + rand.Next(0, 1);
            createJS(url, code =>
            {
                Console.WriteLine(Encoding.ASCII.GetString(code));
            });
        }

        public static void createJS(string src, Action<byte[]> callback)
        {
            httpRequest(GET, src, null, false, callback);
        }


        public static void Login(string qq)
        {
            Console.WriteLine("login begin..");
            getVerifyCode(qq);
            Thread.Sleep(2000);
        }

        //private static readonly string DefaultUserAgent = "Mozilla/5.0 (Windows NT 6.2; WOW64) AppleWebKit/537.31 (KHTML, like Gecko) Chrome/26.0.1410.64 Safari/537.31";
        private static void httpRequest(string method, string action, string query, bool urlencoded, Action<byte[]> callback, int timeout = 0)
        {
            string url = "GET" == method ? (query == null ? action + "?" + query : action) : action;
            byte[] buffer = { 0 };
            if (method == "GET")
            {
                buffer = wc.DownloadData(url);
            }
            if (callback != null)
                callback.Invoke(buffer);
        }
    }
}
