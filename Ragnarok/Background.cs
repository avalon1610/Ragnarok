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
        private void DoTask(Action myTask,Action<object> callback)
        {
            Task BgTask = Task.Factory.StartNew(myTask);
            Task UITask = BgTask.ContinueWith(callback,TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void doLogin()
        {
    
            DoTask(() => { HttpRequest.Login(); }, obj => {this.Login_Tab.IsSelected = true;});
        }  
    }

    
    class HttpRequest
    {
        public static WebClient wc;
        HttpRequest()
        {
            if (wc == null)
                wc = new WebClient();
        }

        public static void getVerifyCode(string qq)
        {
            string uri = "http://check.ptlogin2.qq.com/check?uin="+qq+"&appid=1003903&r="+Math.random()
            //wc.OpenRead()
        }

        public static void Login()
        {
            Console.WriteLine("login begin..");
            Thread.Sleep(2000);
        }
    }
}
