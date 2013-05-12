using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
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

        public static void DoTask(Func<bool> function, Action<Task<bool>> callback)
        {
            Task.Factory.StartNew<bool>(function).ContinueWith(callback, TaskScheduler.FromCurrentSynchronizationContext());
        }
    }

    public partial class PopupWindow
    {
        private void Submit()
        {
            if (VerifyCodeTextBox.Text.Length != 0)
            {
                WEBQQ._verifyCode = VerifyCodeTextBox.Text;
                MyTask.DoTask(() => { return WEBQQ.login(WEBQQ._Password, WEBQQ._State); }, task =>
                {
                    if (task.Result == false)
                    {
                        var mainWindow = this.Owner as MainWindow;
                        mainWindow.ErrorMsg.Text = WEBQQ.error_msg;
                        mainWindow.ErrorMsg_tab.IsSelected = true;
                    }

                });
                Close();
            }
        }
    }

    public partial class MainWindow
    {
        string[] states = { "online", "callme", "hidden", "busy", "away", "silent" };

        private void doLogin()
        {
            string qq = this.QQ.Text;
            string pwd = this.Pwd.Password;
            MyTask.DoTask(() => { return WEBQQ.TryLogin(qq, pwd, states[0]); }, task =>
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
        public static string _uin_source;
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
            _uin_source = temp_uin;
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
        public static string error_msg = "";
        public static bool login(string password, string status)
        {
            if (_qq.Length == 0)
            {
                error_msg = "Account field is empty.";
                return false;
            }
            bool success = true;
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
                        errorMsg(split(result, ",")[4].Substring(1, split(result, ",")[4].Length - 2));
                        success = false;
                    }
                });
            return success;
        }

        private static void errorMsg(string message)
        {
            localStorage.logout = "true";
            error_msg = message;
        }

        private static string _ptwebqq = "";
        private static int _clientid = new Random().Next(10000000, 99999999);
        private static string _vfwebqq = "";
        private static string _psessionid = "";
        private static string _status = "";
        private static void getPsessionid(string ptwebqq, string status)
        {
            _ptwebqq = ptwebqq;
            var r = "{\"status\":\"" + status + "\",\"ptwebqq\":\"" + _ptwebqq + "\",\"passwd_sig\":\"\",\"clientid\":\"" + _clientid + "\",\"psessionid\":null}";
            wc.Headers.Add("Referer", "http://d.web2.qq.com/proxy.html?v=20110331002");//or you get error,ret_code 103
            httpRequest(POST, "https://d.web2.qq.com/channel/login2", "r=" + r + "&clientid=" + _clientid + "&psessionid=null", true, code =>
            {
                dynamic resultObject = getJSON(code);
                _vfwebqq = resultObject["vfwebqq"].ToString();
                _psessionid = resultObject["psessionid"].ToString();
                _status = resultObject["status"].ToString();

                getMyInfo();
            });
        }

        private static string now()
        {
            DateTime dt = DateTime.Parse("01/01/1970");
            TimeSpan ts = DateTime.Now - dt;
            long sec = Convert.ToInt64(Math.Truncate(ts.TotalMilliseconds)); // 秒数
            return Convert.ToString(sec, 10);
        }

        private static string _face = "";
        private static string _info = "";
        private static void getMyInfo()
        {
            var face = "http://face" + new Random().Next(1, 10) + ".qun.qq.com/cgi/svr/face/getface?cache=1&type=1&fid=0&uin=" + _qq + "&vfwebqq=" + _ptwebqq + "&t=" + now();
            _face = face;
            var info = "http://s.web2.qq.com/api/get_friend_info2?tuin=" + _qq + "&verifysession=&code=&vfwebqq=" + _vfwebqq + "&t=" + now();
            httpRequest(GET, info, null, false, code =>
            {
                _info = getJSON(code).ToString();

                getMyLevel();
            });
        }

        private static string _levelInfo = "";
        private static void getMyLevel()
        {
            var url = "http://s.web2.qq.com/api/get_qq_level2?tuin=" + _qq + "&vfwebqq=" + _vfwebqq + "&t=" + now();
            httpRequest(GET, url, null, false, code =>
            {
                _levelInfo = getJSON(code).ToString();

                getMyPersonal();
            });
        }

        private static string _myPersonal = "";
        private static void getMyPersonal()
        {
            var url = "http://s.web2.qq.com/api/get_single_long_nick2?tuin=" + _qq + "&vfwebqq=" + _vfwebqq + "&t=" + now();
            httpRequest(GET, url, null, false, code =>
            {
                _myPersonal = getJSON(code)[0]["lnick"].ToString();

                getFriendsInfo();
            });
        }

        private static string hash(string uin, string ptwebqq)
        {
            var b = uin;
            var i = ptwebqq;
            string s = "";
            string a = i + "password error";
            while (true)
            {
                if (s.Length <= a.Length)
                {
                    s = s + b;
                    if (s.Length == a.Length)
                        break;
                }
                else
                {
                    s = s.Substring(0, a.Length);
                    break;
                }
            }
            ArrayList j = new ArrayList();
            for (var d = 0; d < s.Length; d++)
                j.Add((int)s[d] ^ (int)a[d]);
            string[] _a = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "A", "B", "C", "D", "E", "F" };
            string[] _j = (string[])j.ToArray(typeof(string));
            string _s = "";
            for (var d = 0; d < _j.Length; d++)
            {
                _s += _a[Convert.ToInt32(_j[d]) >> 4 & 15];
                _s += _a[Convert.ToInt32(_j[d]) & 15];
            }
            return _s;
        }

        private static void getFriendsInfo()
        {
            var info = "http://s.web2.qq.com/api/get_user_friends2";
            var r = "{\"h\":\"hello\",\"hash\":\"" + hash(_qq + "", _ptwebqq) + "\",\"vfwebqq\":\"" + _vfwebqq + "\"}";
        }

        private static dynamic getJSON(byte[] code)
        {
            string result = Encoding.ASCII.GetString(code);
            dynamic parseObject = JsonConvert.DeserializeObject(result);
            return parseObject["result"];
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
                _encodedPassword = md5(md5(String2Byte(_Password + _uin_source)) + _verifyCode.ToUpper());
                if (localStorage.password.Length != 0)
                    localStorage.password = Convert.ToString((char)16) + md5(hexChar2Bin(_Password) + _uin);
            }
            else
                _encodedPassword = md5(password.Substring(1) + _verifyCode.ToUpper());
        }

        private static byte[] String2Byte(string input)
        {
            byte[] result = new byte[0x18]; // (32/2+8) 
            string temp = "";
            for (int i = 0, j = 0; i < input.Length; i = i + 2, j++)
            {
                temp = input.Substring(i, 2);
                result[j] = Convert.ToByte(input.Substring(i, 2), 16);
            }
            return result;
        }

        private static string md5(string input)
        {
            return md5(Encoding.ASCII.GetBytes(input));
        }

        private static string md5(byte[] input)
        {
            // Create a new instance of the MD5CryptoServiceProvider object.
            MD5 md5Hasher = MD5.Create();

            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hasher.ComputeHash(input);

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
            return sBuilder.ToString().ToUpper();
        }

        private static string hexChar2Bin(string str)
        {
            string arr = "";
            int temp = 0;
            for (var i = 0; i < str.Length; i = i + 2)
            {
                temp = Convert.ToInt32(str.Substring(i, 2), 16);
                arr = arr + (char)temp;
            }
            return arr;
        }

        public static void createJS(string src, Action<byte[]> callback)
        {
            httpRequest(GET, src, null, false, callback);
        }

        public static string _Account = "";
        public static string _Password = "";
        public static string _State = "";
        public static string _qq = "";

        public static string TryLogin(string account, string password, string state)
        {
            Console.WriteLine("login begin..");
            _Account = account;
            _Password = password;
            _State = state;
            if (account.Length != 0)
                return getVerifyCode(account);
            return "";
        }
        public static MyWebClient wc = new MyWebClient();

        public static readonly string GET = "GET";
        public static readonly string POST = "POST";
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
        public static string logout = "";
    }

    public class MyWebClient : WebClient
    {
        private CookieContainer cookieContainer;

        public MyWebClient()
        {
            this.cookieContainer = new CookieContainer();
        }

        public MyWebClient(CookieContainer cookies)
        {
            this.cookieContainer = cookies;
        }

        public CookieContainer Cookies
        {
            get { return this.cookieContainer; }
            set { this.cookieContainer = value; }
        }

        //CookieContainer cookies = new CookieContainer();
        //public CookieContainer Cookies { get { return cookies; } }
        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest request = base.GetWebRequest(address);
            if (request is HttpWebRequest)
            {
                HttpWebRequest httpRequest = request as HttpWebRequest;
                httpRequest.CookieContainer = cookieContainer;
            }
            return request;
        }

    }
}
