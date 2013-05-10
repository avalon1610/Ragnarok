using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Ragnarok
{
    public class MyTask
    {
        public static void DoTask(Action myTask, Action<Task> callback)
        {
            Task.Factory.StartNew(myTask).ContinueWith(callback, TaskScheduler.FromCurrentSynchronizationContext());
        }

        public static void DoTask(Func<string> function, Action<Task<string>> callback)
        {
            Task.Factory.StartNew<string>(function).ContinueWith(callback, TaskScheduler.FromCurrentSynchronizationContext());
        }

        public static void DoTask(Func<int> function, Action<Task<int>> callback)
        {
            Task.Factory.StartNew<int>(function).ContinueWith(callback, TaskScheduler.FromCurrentSynchronizationContext());
        }
    }

    public partial class PopupWindow
    {
        private void Submit()
        {
            if (VerifyCodeTextBox.Text.Length != 0)
            {
                WEBQQ._verifyCode = VerifyCodeTextBox.Text;
                MyTask.DoTask(() => { WEBQQ.login(WEBQQ._Password, WEBQQ._State); }, t => { (this.Owner as MainWindow).Login_Tab.IsSelected = true; });
                Close();
            }
        }
    }

    public partial class MainWindow
    {
        private void doLogin()
        {
            string qq = this.QQ.Text;
            string pwd = this.Pwd.Password;
            MyTask.DoTask(() => { return WEBQQ.TryLogin(qq, pwd, 1); }, task =>
            {
                if (task.Result.Length != 0)
                {
                    // needVerifyCodeImg
                    WEBQQ.getVerifyCodeImg(this, task.Result);
                }
                //this.Login_Tab.IsSelected = true; 
            });

            //DoTask(() => { return WEBQQ.PrintPwd(pwd); }, antecendent => { this.Login_Tab.Header = antecendent.Result; });
        }
    }


    class WEBQQ
    {
        public static string PrintPwd(string pwd)
        {
            return pwd;
        }

        public static MyWebClient wc = new MyWebClient();

        public static readonly string GET = "GET";
        public static readonly string POST = "POST";

        private static string[] split(string input, string pattern)
        {
            return Regex.Split(input, pattern, RegexOptions.IgnoreCase);
        }

        public static bool needVerifyCodeImg = false;
        public static string getVerifyCode(string qq)
        {
            if (qq.Length == 0)
                return "";
            _qq = qq;
            Random rand = new Random();
            string url = "http://check.ptlogin2.qq.com/check?uin=" + qq + "&appid=1003903&r=" + rand.NextDouble();
            createJS(url, code =>
            {
                var query = split(split(split(Encoding.ASCII.GetString(code), @"\('")[1], @"'\)")[0], @"','");
                checkVerifyCode(query[0], query[1], query[2]);
            });

            if (needVerifyCodeImg)
                return _verifyCode;
            return "";
        }

        public static string _uin;
        public static string _verifyCode;

        private static void checkVerifyCode(string stateCode, string verifyCode, string uin)
        {
            List<string> temp = new List<string>();
            for (int i = 2; i < uin.Length; i += 4)
            {
                temp.Add(uin.Substring(i, 2));
            }
            var temp_uin = string.Join("", temp.ToArray());
            uin = hexChar2Bin(temp_uin);
            _uin = uin;
            _verifyCode = verifyCode;
            if ("0" == stateCode)
            {
                login(_Password, _State);
            }
            else if ("1" == stateCode)
            {
                //MyTask.DoTask(() => { ;}, (task) => getVerifyCodeImg(verifyCode));
                needVerifyCodeImg = true;
            }
        }

        public static void getVerifyCodeImg(MainWindow main, string verifyCode)
        {
            PopupWindow popup = new PopupWindow();
            popup.Owner = main;
            popup.Show();
        }


        private static string _skey = "";
        private static int _QQ = 0;            //HTML5.qq 
        public static void login(string password, int status)
        {
            if (_qq.Length == 0)
                return;
            encodePassword(password);
            createJS("http://ptlogin2.qq.com/login?u=" + _qq + "&p=" + _encodedPassword + "&verifycode=" + _verifyCode.ToUpper() + "&webqq_type=40&remember_uin=1&login2qq=1&aid=1003903&u1=http%3A%2F%2Fweb.qq.com%2Floginproxy.html%3Flogin2qq%3D1%26webqq_type%3D40&h=1&ptredirect=0&ptlang=2052&from_ui=1&pttype=1&dumy=&fp=loginerroralert&action=4-3-2475914&mibao_css=m_webqq&t=1&g=1", code =>
                {
                    string result = Encoding.UTF8.GetString(code);
                    if (result.IndexOf("登录成功") != -1)
                    {
                        getCookie("http://qq.com", "skey", cookie_value =>
                            {
                                _skey = cookie_value;
                            });
                        getCookie("http://qq.com", "ptwebqq", cookie_value =>
                            {
                                getPsessionid(cookie_value, status);
                            });
                        getCookie("http://qq.com", "uin", cookie_value =>
                            {
                                _QQ = Convert.ToInt32(cookie_value.Substring(0, 1), 10);
                            });
                    }
                    else
                    {
                        //errorMsg
                    }
                });
        }

        private static string _ptwebqq = "";
        private static int _clientid = new Random().Next(10000000, 99999999);
        private static string _vfwebqq = "";
        private static string _psessionid = "";
        private static string _status = "";
        private static void getPsessionid(string ptwebqq, int status)
        {
            _ptwebqq = ptwebqq;
            var r = "{\"status\":\"" + status + "\",\"ptwebqq\":\"" + _ptwebqq + "\",\"passwd_sig\":\"\",\"clientid\":\"" + _clientid + "\",\"psessionid\":null}";
            httpRequest(POST, "https://d.web2.qq.com/channel/login2", "r=" + r + "&clientid=" + _clientid + "&psessionid=null", true, code =>
                {
                    string result = Encoding.ASCII.GetString(code);
                    dynamic parseObject = JsonConvert.DeserializeObject(result);
                    result = parseObject.result;
                    _vfwebqq = parseObject.vfwebqq;
                    _psessionid = parseObject.psessionid;
                    _status = parseObject.status;


                    //to do getMyInfo();
                });
        }

        private static void getCookie(string url, string name, Action<string> callback)
        {
            var cookies = wc.Cookies.GetCookies(new Uri(url));
            foreach (Cookie cookie in cookies)
            {
                if (cookie.Name == name)
                {
                    if (callback != null)
                        callback.Invoke(cookie.Value);
                    break;
                }
            }
        }

        private static string _encodedPassword = "";
        private static void encodePassword(string password)
        {
            if (Convert.ToChar(password.Substring(0, 1)) != (char)16)
            {
                if (password.Length > 16)
                    password = password.Substring(0, 16);
                _Password = md5(password);
                _encodedPassword = md5(md5(hexChar2Bin(_Password) + _uin + _verifyCode.ToUpper()));
                if (localStorage.password.Length != 0)
                    localStorage.password = Convert.ToString((char)16) + md5(hexChar2Bin(_Password) + _uin);
            }
            else
                _encodedPassword = md5(password.Substring(1) + _verifyCode.ToUpper());
        }

        static string md5(string input)
        {
            // Create a new instance of the MD5CryptoServiceProvider object.
            MD5 md5Hasher = MD5.Create();

            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }

        private static string hexChar2Bin(string str)
        {
            var arr = "";
            var temp = 0;
            for (var i = 0; i < str.Length; i = i + 2)
            {
                temp = Convert.ToInt32(str.Substring(i, 2), 16);
                if (temp == 0)
                    continue;
                arr += (char)temp;

            }
            return arr;
        }

        public static void createJS(string src, Action<byte[]> callback)
        {
            httpRequest(GET, src, null, false, callback);
        }

        public static string _Account = "";
        public static string _Password = "";
        public static int _State = 0;
        public static string _qq = "";

        public static string TryLogin(string account, string password, int state)
        {
            Console.WriteLine("login begin..");
            _Account = account;
            _Password = password;
            _State = state;
            if (account.Length != 0)
                return getVerifyCode(account);
            return "";
        }

        //private static readonly string DefaultUserAgent = "Mozilla/5.0 (Windows NT 6.2; WOW64) AppleWebKit/537.31 (KHTML, like Gecko) Chrome/26.0.1410.64 Safari/537.31";
        private static void httpRequest(string method, string action, string query, bool urlencoded, Action<byte[]> callback, int timeout = 0)
        {
            //string url = "GET" == method ? (query == null ? action + "?" + query : action) : action;
            string url = action;
            byte[] buffer = { 0 };
            if (method == "GET")
            {
                buffer = wc.DownloadData(url);
            }
            else if (method == "POST")
            {
                byte[] postData = Encoding.ASCII.GetBytes(query);
                wc.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                buffer = wc.UploadData(url, postData);
            }
            else return;
            if (callback != null)
                callback.Invoke(buffer);
        }
    }

    public class localStorage
    {
        public static string password = "";
    }

    public class MyWebClient : WebClient
    {
        CookieContainer cookies = new CookieContainer();
        public CookieContainer Cookies { get { return cookies; } }
        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest request = base.GetWebRequest(address);
            if (request.GetType() == typeof(HttpWebRequest))
                ((HttpWebRequest)request).CookieContainer = cookies;
            return request;
        }
    }
}
