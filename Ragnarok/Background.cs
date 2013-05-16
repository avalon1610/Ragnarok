using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Cache;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

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
                    var mainWindow = this.Owner as MainWindow;
                    if (task.Result == false)
                    {
                        mainWindow.ErrorMsg.Text = WEBQQ.error_msg;
                        mainWindow.ErrorMsg_tab.IsSelected = true;
                    }
                    else
                        mainWindow.showContact();

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
            //for test
            //WEBQQ.hash("494787284", "dd0c6037243e3bd4b83f060ebbe0f293e56e4d6d6fd4752e72ad32aaf70ddeea");
            //           
            string qq = this.QQ.Text;
            string pwd = this.Pwd.Password;
            //bool loginDirectly = false;
            MyTask.DoTask(() => { return WEBQQ.TryLogin(qq, pwd, states[2]); }, task =>
            {
                if (task.Result.Length != 0)
                {
                    // needVerifyCodeImg
                    WEBQQ.getVerifyCodeImg(this, task.Result);
                }
                else
                {
                    loginDirectly();
                }
            });
        }

        private void loginDirectly()
        {

            MyTask.DoTask(() => { return WEBQQ.login(WEBQQ._Password, WEBQQ._State); }, task =>
            {
                if (task.Result == false)
                {
                    this.ErrorMsg.Text = WEBQQ.error_msg;
                    this.ErrorMsg_tab.IsSelected = true;
                }
                else
                    showContact();
            });

            //DoTask(() => { return WEBQQ.PrintPwd(pwd); }, antecendent => { this.Login_Tab.Header = antecendent.Result; });
        }

        public void showContact()
        {
            viewmodel.BindingToUI();
            Avatar_Image.Source = LoadImageFromUrl(WEBQQ._face);
            Avatar.Visibility = Visibility.Visible;
            Contact_tab.Visibility = Visibility.Visible;
            Recent_tab.Visibility = Visibility.Visible;
            Group_tab.Visibility = Visibility.Visible;
            Login_Tab.Visibility = Visibility.Hidden;
            Login_Tab.Header = "";
            Nick_Text.Text = WEBQQ._info["nick"].ToString();
            Contact_tab.IsSelected = true;
        }
    }


    class WEBQQ
    {
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
                //login(_Password, _State);
                needVerifyCodeImg = false;
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
                        _QQ = Convert.ToInt32(cookie_value.Substring(1), 10);
                    });
                }
                else
                {
                    errorMsg(split(result, ",")[4].Substring(1, split(result, ",")[4].Length - 2));
                    success = false;
                }
            });
            if (success)
                MainWindow.viewmodel.SetData();
            return success;
        }

        private static void errorMsg(string message)
        {
            localStorage.logout = "true";
            error_msg = message;
        }

        private static string _ptwebqq = "";
        private static int _clientid = new Random().Next(10000000, 99999999);
        public static string _vfwebqq = "";
        private static string _psessionid = "";
        private static string _status = "";
        private static void getPsessionid(string ptwebqq, string status)
        {
            _ptwebqq = ptwebqq;
            var r = "{\"status\":\"" + status + "\",\"ptwebqq\":\"" + _ptwebqq + "\",\"passwd_sig\":\"\",\"clientid\":\"" + _clientid + "\",\"psessionid\":null}";
            //wc.Headers.Add("Referer", "http://d.web2.qq.com/proxy.html?v=20110331002");//or you get error,ret_code 103
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

        public static string _face = "";
        public static dynamic _info = "";
        private static void getMyInfo()
        {
            var face = "http://face" + new Random().Next(1, 10) + ".qun.qq.com/cgi/svr/face/getface?cache=1&type=1&fid=0&uin=" + _qq + "&vfwebqq=" + _ptwebqq + "&t=" + now();
            _face = face;
            var info = "http://s.web2.qq.com/api/get_friend_info2?tuin=" + _qq + "&verifysession=&code=&vfwebqq=" + _vfwebqq + "&t=" + now();
            //wc.Headers.Add("Referer", "http://d.web2.qq.com/proxy.html?v=20110331002");
            httpRequest(GET, info, null, false, code =>
            {
                _info = getJSON(code);

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

        public static string hash(string uin, string ptwebqq)
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
                j.Add(Convert.ToString((int)s[d] ^ (int)a[d]));
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

        public static FriendInfo friendInfo = new FriendInfo();
        private static void getFriendsInfo()
        {
            var http_info = "http://s.web2.qq.com/api/get_user_friends2";
            var r = "{\"h\":\"hello\",\"hash\":\"" + hash(_qq + "", _ptwebqq) + "\",\"vfwebqq\":\"" + _vfwebqq + "\"}";
            httpRequest(POST, http_info, "r=" + r, true, code =>
            {
                dynamic result = getJSON(code);
                string test = result.ToString();
                if (result["categories"].Count == 0)
                    friendInfo.Categories.Add(new Category(0, "我的好友", 0));
                else
                {
                    if (result["categories"][0]["index"].ToString() != "0")
                        friendInfo.Categories.Add(new Category(0, "我的好友", 0));
                    foreach (var cate in result["categories"])
                    {
                        friendInfo.Categories.Add(new Category(Convert.ToInt32(cate["index"].ToString()),
                                                               cate["name"].ToString(),
                                                               Convert.ToInt32(cate["sort"].ToString())));
                    }
                }

                foreach (var friend in result["friends"])
                {
                    Friend f = new Friend();
                    f.uin = friend["uin"].ToString();
                    f.category = Convert.ToInt32(friend["categories"].ToString());
                    f.flag = Convert.ToInt32(friend["flag"].ToString());
                    friendInfo.Friends.Add(f);
                    Category c = friendInfo.FindCategory(f.category);
                    if (c != null)
                        c.Friends.Add(f);
                }

                foreach (var info in result["info"])
                {
                    Friend f = friendInfo.FindFriend(info["uin"].ToString());
                    if (f == null)
                        continue;
                    f.face = info["face"].ToString();
                    f.nick = info["nick"].ToString();
                    f.face_flag = info["flag"].ToString();
                }

                foreach (var markname in result["marknames"])
                {
                    Friend f = friendInfo.FindFriend(markname["uin"].ToString());
                    if (f == null)
                        continue;
                    f.markname = markname["markname"].ToString();
                    f.markname_type = markname["type"].ToString();
                }

                foreach (var vipinfo in result["vipinfo"])
                {
                    Friend f = friendInfo.FindFriend(vipinfo["u"].ToString());
                    if (f == null)
                        continue;
                    f.is_vip = vipinfo["is_vip"].ToString();
                    f.vip_level = vipinfo["vip_level"].ToString();
                }

                getGroupsInfo();
            });
        }

        private static dynamic _groupsInfo;
        private static void getGroupsInfo()
        {
            var info = "http://s.web2.qq.com/api/get_group_name_list_mask2";
            var r = "\"vfwebqq\":\"" + _vfwebqq + "\"}";
            httpRequest(POST, info, "r=" + r, true, code =>
            {
                _groupsInfo = getJSON(code);

                getOnlineList();
            });
        }

        private static dynamic _onlineList;
        private static void getOnlineList()
        {
            var url = "http://d.web2.qq.com/channel/get_online_buddies2?clientid=" + _clientid + "&psessionid=" + _psessionid;
            httpRequest(GET, url, null, false, code =>
            {
                _onlineList = getJSON(code);

                getPersonal();
            });
        }

        private static string _personal = "";
        private static void getPersonal()
        {
            if (_onlineList.Count == 0)
            {
                //personal =
                getRecentList();
                return;
            }

            List<string> list = new List<string>();
            foreach (var o in _onlineList)
            {
                list.Add(o["uin"].ToString());
            }

            string onlinelist = "[" + String.Join(",", list.ToArray()) + "]";
            var url = "http://s.web2.qq.com/api/get_long_nick?tuin=" + onlinelist + "&vfwebqq=" + _vfwebqq + "&t=" + now();
            httpRequest(GET, url, null, false, code =>
            {
                _personal = getJSON(code).ToString();

                getRecentList();
            });
        }

        private static string _recentList = "";
        private static void getRecentList()
        {
            var url = "http://d.web2.qq.com/channel/get_recent_list2";
            var r = "{\"vfwebqq\":\"" + _vfwebqq + "\",\"clientid\":\"" + _clientid + "\",\"psessionid\":\"" + _psessionid + "\"}";
            httpRequest(POST, url, "r=" + r + "&clientid=" + _clientid + "&psessionid=" + _psessionid, true, code =>
            {
                _recentList = getJSON(code).ToString();

                poll();
                finish();
            });
        }

        private static void poll()
        {
            var url = "http://d.web2.qq.com/channel/poll2";
            var r = "{\"clientid\":\"" + _clientid + "\",\"psessionid\":\"" + _psessionid + "\",\"key\":0,\"ids\":[]}";
            httpRequest(POST, url, "r=" + r + "&clientid=" + _clientid + "&psessonid=" + _psessionid, true, code =>
            {
                string result = Encoding.UTF8.GetString(code);
                if (result.Length != 0)
                {
                    poll();
                    try
                    {
                        dynamic parseObject = JsonConvert.DeserializeObject(result);
                        dynamic resultObject = parseObject["result"];
                        int retcode = Convert.ToInt32(parseObject["retcode"].ToString());
                        switch (retcode)
                        {
                            case 0:
                                foreach (var res in resultObject)
                                {
                                    switch (res["poll_type"].ToString() as String)
                                    {
                                        case "buddies_status_change":
                                            updateOnlineList(res["value"].ToString());
                                            break;
                                        case "message":
                                            break;
                                    }
                                }

                                break;
                            case 121:
                                // disconnect
                                break;
                        }
                    }
                    catch (System.Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            },true,90000);
        }

        private static void finish()
        {

        }

        private static void updateOnlineList(string value)
        {

        }

        private static dynamic getJSON(byte[] code)
        {
            string result = Encoding.UTF8.GetString(code);
            dynamic parseObject = JsonConvert.DeserializeObject(result);
            string retcode = parseObject["retcode"].ToString();
            if (retcode != "0")
                MessageBox.Show("error:{0}", retcode);
            return parseObject["result"];
        }

        private static void getCookie(string url, string name, Action<string> callback)
        {
            MyWebClient wc = WEBQQ.wc_primary.IsBusy ? new MyWebClient(WEBQQ.wc_primary.Cookies) : WEBQQ.wc_primary;
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
            _Account = account;
            _Password = password;
            _State = state;
            if (account.Length != 0)
                return getVerifyCode(account);
            return "";
        }
        public static MyWebClient wc_primary = new MyWebClient();
        public static MyWebClient wc_copy = null;

        private static readonly string GET = "GET";
        private static readonly string POST = "POST";
        //private static readonly string DefaultUserAgent = "Mozilla/5.0 (Windows NT 6.2; WOW64) AppleWebKit/537.31 (KHTML, like Gecko) Chrome/26.0.1410.64 Safari/537.31";
        private static void httpRequest(string method, string action, string query, bool urlencoded, Action<byte[]> callback, bool async = false, int timeout = 3000)
        {
            MyWebClient wc;
            if (wc_primary.IsBusy)
                wc = new MyWebClient(wc_primary.Cookies);
            else wc = wc_primary;

            wc.timeout = timeout;
            if (async == true)
            {
                wc.callback = callback;
            }

            string url = action;
            byte[] buffer = { 0 };
            wc.Headers.Add("Referer", "http://d.web2.qq.com/proxy.html?v=20110331002");

            try
            {
                if (method == GET)
                {
                    if (async)
                    {
                        wc.DownloadDataCompleted += OnGetCompleted;
                        wc.DownloadDataAsync(new Uri(url));
                    }
                    else
                        buffer = wc.DownloadData(url);
                }
                else if (method == POST)
                {
                    byte[] postData = Encoding.ASCII.GetBytes(query);
                    wc.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                    if (async)
                    {
                        wc.UploadDataCompleted += OnPostCompleted;
                        wc.UploadDataAsync(new Uri(url), postData);
                    }
                    else
                        buffer = wc.UploadData(url, postData);
                }
                else return; 
                if (callback != null && async == false)
                    callback.Invoke(buffer);
            }
            catch (WebException e)
            {
                if (e.Status == WebExceptionStatus.Timeout)
                    error_msg = "请求超时...";
            }     
        }

        private static void OnGetCompleted(Object sender, DownloadDataCompletedEventArgs e)
        {
            MyWebClient mwc = sender as MyWebClient;
            byte[] buffer = e.Result;
            if (mwc.callback != null)
                mwc.callback.Invoke(buffer);
        }

        private static void OnPostCompleted(object sender, UploadDataCompletedEventArgs e)
        {
            MyWebClient mwc = sender as MyWebClient;
            byte[] buffer = e.Result;
            if (mwc.callback != null)
                mwc.callback.Invoke(buffer);
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
            this.CachePolicy = new RequestCachePolicy(RequestCacheLevel.Default);
        }

        public MyWebClient(CookieContainer cookies)
        {
            this.cookieContainer = cookies;
            this.CachePolicy = new RequestCachePolicy(RequestCacheLevel.Default);
        }

        public CookieContainer Cookies
        {
            get { return this.cookieContainer; }
            set { this.cookieContainer = value; }
        }

        public Action<byte[]> callback = null;
        public int timeout = 0;
        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest request = base.GetWebRequest(address);
            if (request is HttpWebRequest)
            {
                HttpWebRequest httpRequest = request as HttpWebRequest;
                httpRequest.CookieContainer = cookieContainer;

                // this timeout only work for synchronous,not asynchronous 
                if (timeout == 0)
                    timeout = Timeout.Infinite;
                httpRequest.Timeout = timeout;
                httpRequest.ReadWriteTimeout = timeout;
            }
            return request;
        }
    }

    public class FriendInfo
    {
        public List<Category> Categories { get; set; }
        public List<Friend> Friends { get; set; }

        public FriendInfo()
        {
            Categories = new List<Category>();
            Friends = new List<Friend>();
        }

        public Friend FindFriend(string uin)
        {
            foreach (var friend in Friends)
            {
                if (friend.uin == uin)
                    return friend;
            }
            return null;
        }

        public Category FindCategory(int index)
        {
            foreach (var category in Categories)
            {
                if (category.index == index)
                    return category;
            }
            return null;
        }
    }

    public class Category
    {
        public int index { get; set; }
        public string name { get; set; }
        public int sort { get; set; }
        public List<Friend> Friends { get; set; }

        public Category()
        {
            Friends = new List<Friend>();
        }

        public Category(int i, string n, int s)
        {
            Friends = new List<Friend>();
            index = i;
            name = n;
            sort = s;
        }
    }

    public class Friend
    {
        public string uin { get; set; }
        public int category { get; set; }
        public int flag { get; set; }
        public string face { get; set; }
        public string nick { get; set; }
        public string face_flag { get; set; }
        public string markname { get; set; }
        public string markname_type { get; set; }
        public string is_vip { get; set; }
        public string vip_level { get; set; }
    }
}


