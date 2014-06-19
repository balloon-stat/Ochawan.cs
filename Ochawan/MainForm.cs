using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Windows.Forms;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Automation;
using System.Runtime.InteropServices;
using Sgml;
namespace Ochawan
{
    public partial class MainForm : Form
    {
        #region member
        bool isPublish = false;
        string Bouyomi_path;
        string OBS_path;
        string NLE_path;
        string Sasara_path;
        string m_token = "";
        string m_liveID;
        static string m_count = ""; //コメント番号
        Dictionary<string,string> namedic = new Dictionary<string,string>();
        static CookieContainer m_cc = new CookieContainer();
        static List<string> m_comID = new List<string>(); //コミュID
        static List<string> m_pubID = new List<string>(); //生主ID

        //ログインAPI戻り値
        //static string m_ticket = ""; //認証用チケット

        //認証API(getalertstatus)戻り値
        static string m_addr = "";   //番組開始情報サーバのアドレス
        static string m_port = "";   //番組開始情報サーバのポート
        static string m_thread = ""; //スレッドID

        //番組詳細情報取得API(getplayerstatus)戻り値
        static string m_base_time = "";    //開始時刻
        static string m_userID = "";       //ユーザID
        static string m_ComSrvAddr = "";   //コメントサーバのアドレス
        static string m_ComSrvPort = "";   //コメントサーバのポート
        static string m_ComSrvThread = ""; //スレッドID

        //getpostkey戻り値
        static string m_postkey = "";

        static DateTime m_DateTimeStart; //開始時刻(ローカル)
        static string m_ComTicket = "";
        static string m_SrvTime = "";
        static string m_LiveComId;

        static bool m_stopped = true;
        
#endregion
        HotKey hotkey;
        IgoForm igoForm;
        GameForm gameForm;
        ViewForm viewForm;
        Sasara sasara;
        Ringin ringin = new Ringin();
        OBS obs = new OBS();
        Task listenTask;
        CancellationTokenSource ctokenSrc = new CancellationTokenSource();
        FNF.Utility.BouyomiChanClient bClient;

        #region init close


        private void say(string msg)
        {
            Debug.WriteLine(msg);
            toolStripStatusLabel1.Text = msg;
        }
        public MainForm()
        {
            InitializeComponent();
            hotkey = new HotKey(MOD_KEY.ALT | MOD_KEY.CONTROL, Keys.A);
            hotkey.HotKeyPush += aKey_HotKeyPush;

            setConfig();
        }
        private void MainForm_Load(object sender, EventArgs e)
        {
            int b_proc = Process.GetProcessesByName("BouyomiChan").Count();
            if (b_proc == 0)
            {
                checkBox1.CheckState = CheckState.Unchecked;
            }
            int proc = Process.GetProcessesByName("OBS").Count();
            if (proc == 0)
            {
                checkBox2.CheckState = CheckState.Unchecked;
            }
            sasaraPrepare();
        }
        void aKey_HotKeyPush(object sender, EventArgs e)
        {
            this.Activate();
            textBox2.Focus();
        }
        private void setConfig()
        {
            string browser;
            using(var sr = new StreamReader("RunConfig.txt"))
            {
                browser = sr.ReadLine();
                Bouyomi_path = sr.ReadLine();
                NLE_path = sr.ReadLine();
                OBS_path = sr.ReadLine();
                Sasara_path = sr.ReadLine();
            }
            m_cc.Add(CookieGetter.Get(browser));
        }

        private bool sasaraPrepare()
        {
            int proc = Process.GetProcessesByName( "CeVIO Creative Studio FREE").Count();
            if (proc == 0)
            {
                checkBox5.CheckState = CheckState.Unchecked;

                return false;
            }
            else
            {
                sasara = sasara ?? new Sasara();

                return sasara.IsReady;
            }

        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            //if (Form.ActiveForm != null)
            //    Form.ActiveForm.Hide();
            //var sirializar = new System.Xml.Serialization.XmlSerializer(typeof(Dictionary<string, string>));
            //var fs = new FileStream("names.xml", FileMode.Create);
            //sirializar.Serialize(fs, namedic);
            //fs.Close();
        }


        #endregion


        #region niconicoAPI


        private bool threadIsAlive()
        {
            if (listenTask == null)
            {
                return false;
            }
            Debug.WriteLine("TaskState : " + listenTask.Status.ToString());
            if (listenTask.Status == System.Threading.Tasks.TaskStatus.Running)
            {
                return true;
            }
            return false;
        }

        static public XDocument ParseHtml(string url)
        {
            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                req.CookieContainer = m_cc;//取得済みのクッキーコンテナ
                WebResponse res = req.GetResponse();
                using (var resStream = res.GetResponseStream())
                using (var sr = new StreamReader(resStream, Encoding.UTF8))
                using (var sgmlReader = new SgmlReader { DocType = "HTML", CaseFolding = CaseFolding.ToLower })
                {
                    sgmlReader.InputStream = sr; // ↑の初期化子にくっつけても構いません
                    return XDocument.Load(sgmlReader);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return null;
            }
        }
        //GetProgramInfo()のための情報（m_addr,m_port,m_thread）を取得し、格納する
        static void GetAlertInfo()
        {
            try
            {
                var wc = new WebClient() { Encoding = Encoding.UTF8 };
                string xml = wc.DownloadString("http://live.nicovideo.jp/api/getalertinfo");
                wc.Dispose();

                Debug.WriteLine("○認証API(getalertinfo)レスポンス取得");
                Debug.WriteLine(xml);

                var xdoc = XDocument.Parse(xml);
                var ms = xdoc.Descendants("ms").Single();

                m_addr = ms.Element("addr").Value;
                m_port = ms.Element("port").Value;
                m_thread = ms.Element("thread").Value;

                Debug.WriteLine("○サーバ情報");
                Debug.WriteLine("  アドレス   " + m_addr);
                Debug.WriteLine("  ポート     " + m_port);
                Debug.WriteLine("  スレッドID " + m_thread);
                Debug.WriteLine("");
            }
            catch (Exception e)
            {
                Debug.WriteLine("Message    : " + e.Message);
                Debug.WriteLine("Type       : " + e.GetType().FullName);
                Debug.WriteLine("StackTrace : " + e.StackTrace.ToString());
                MessageBox.Show("GetAlertInfo() is error");
            }
        }
        //開始した番組の情報（LiveId、生主名、コミュ番号)を取得し、条件に一致した番組を開く
        void GetProgramInfo()
        {
            IPAddress hostadd = Dns.GetHostEntry(m_addr).AddressList[0];
            IPEndPoint ephost = new IPEndPoint(hostadd, int.Parse(m_port));
            Socket sock = new Socket(
                AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                sock.Connect(ephost);
                //リクエストメッセージを送信
                //注)最後に'\0'を挿入しないとレスポンスは返ってこない
                string param = String.Format("<thread thread=\"{0}\" version=\"20061206\" res_from=\"-1\"/>\0", m_thread);
                byte[] data = Encoding.UTF8.GetBytes(param);
                sock.Send(data, data.Length, SocketFlags.None);
                //受信する
                const int MAX_RECEIVE_SIZE = 1024 * 100;
                string prev = "";

                while (!m_stopped)
                {
                    byte[] resBytes = new byte[MAX_RECEIVE_SIZE];
                    int resSize = sock.Receive(resBytes, resBytes.Length, SocketFlags.None);
                    if (resSize == 0)
                    {
                        Debug.WriteLine("resSize is zero");
                        break;
                    }
                    string xml = prev + Encoding.UTF8.GetString(resBytes, 0, resSize);
                    prev = "";
                    say("GetProgramInfo()実行中");
                    Debug.WriteLine("○データ受信(" + resSize.ToString() + "byte)");
                    //Debug.WriteLine(xml);//全受信XML表示

                    //XML解析
                    //<thread hoge />\0<chat>情報</chat>\0<chat>情報</chat>\0の形で受信する
                    //<thread hoge />は捨て、<chat>情報</chat>から情報を取り出す
                    xml = xml.Replace('\0', '\n');
                    string[] lines = xml.Split('\n');
                    foreach (string line in lines)
                    {
                        if (m_stopped) { break; }
                        Debug.WriteLine(line);//受信XML表示(1行)
                        if (line != "<chat" && !line.EndsWith("/chat>"))
                        {
                            //MAX_RECEIVE_SIZEいっぱいに受信した場合等
                            //XMLが閉じていない場合は次回Receive時結合する
                            prev = line;
                            break;
                        }
                        if (line.StartsWith("<chat"))
                        {
                            //<chat>ここ</chat>を取り出す
                            var chat = XElement.Parse(line).Value;
                            string[] infos = chat.Split(',');
                            if (infos.Length != 3)
                            {
                                //番組通知が1分間行われなかったときに発行されるダミーデータ
                                //chatタグのテキスト部分に現在のUnixタイムが格納されるらしい
                                continue;
                            }
                            Debug.WriteLine("  放送   : " + infos[0]);
                            Debug.WriteLine("  コミュ : " + infos[1]);
                            Debug.WriteLine("  生主   : " + infos[2]);

                            //m_comID = "dummy";

                            //アラート判定
                            if (( m_comID.Any(x => x == infos[1]) || (m_pubID.Any(x => x == infos[2])))
                              || ((m_comID.Count == 0) && (m_pubID.Count == 0)))
                            {
                                //引っかかったら(または最初の放送をターゲットとする場合)ブラウザで開く
                                Process.Start("http://live.nicovideo.jp/watch/lv" + infos[0]);

                                //番組情報を取得
                                if (!GetPlayerStatus(infos[0]))
                                {
                                    break;
                                }
                                else
                                {
                                    //コメントサーバ接続
                                    GetComment();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Message    : " + e.Message);
                Debug.WriteLine("Type       : " + e.GetType().FullName);
                Debug.WriteLine("StackTrace : " + e.StackTrace.ToString());
                MessageBox.Show("GetProgramInfo() is error");
            }
            finally
            {
                if (sock.Connected)
                {
                    sock.Shutdown(SocketShutdown.Both);
                    sock.Close();

                    //Thread.ResetAbort();
                }
            }
        }
        //GetComment()のための情報（m_base_time,m_userid,m_ComSrvAddr,m_ComSrvPort,m_ComSrvThread）を取得し、格納する
        public bool GetPlayerStatus(string liveID)
        {
            XDocument xdoc = null;
            try
            {
                string url = "http://live.nicovideo.jp/api/getplayerstatus?v=lv" + liveID; 
                xdoc = ParseHtml(url);

                m_base_time = xdoc.Descendants("base_time").Single().Value;
                m_userID = xdoc.Descendants("user_id").Single().Value;
                var ms = xdoc.Descendants("ms").Single();
                m_ComSrvAddr = ms.Element("addr").Value;
                m_ComSrvPort = ms.Element("port").Value;
                m_ComSrvThread = ms.Element("thread").Value;

                var comId1 = xdoc.Descendants("default_community").Single().Value;
                var comId2 = xdoc.Descendants("room_label").Single().Value;
                Debug.WriteLine("default_community : " + comId1 + ", room_label : " + comId2);
                m_LiveComId = comId1.Replace("co","");

                say("Live ID : lv" + liveID + ", Commnity ID : " + comId1);

                Debug.WriteLine("");
                Debug.WriteLine("○ユーザ情報");
                Debug.WriteLine("  ユーザID   " + m_userID);
                Debug.WriteLine("○コメントサーバ情報");
                Debug.WriteLine("  アドレス   " + m_ComSrvAddr);
                Debug.WriteLine("  ポート     " + m_ComSrvPort);
                Debug.WriteLine("  スレッドID " + m_ComSrvThread);
            }
            catch (Exception e)
            {
                Debug.WriteLine("");
                Debug.WriteLine("×コメントサーバ情報");
                Debug.WriteLine("  情報取得失敗");
                Debug.WriteLine("Message    : " + e.Message);
                Debug.WriteLine("Type       : " + e.GetType().FullName);
                Debug.WriteLine("StackTrace : " + e.StackTrace.ToString());
                Debug.WriteLine(xdoc);
                //MessageBox.Show("GetPlayerStatus() is error");
                return false;
            }
            return true;
        }
        void GetComment()
        {
            m_count = "0";
            if (isPublish)
            {
                GetToken();
                ringin.Clear();
            }
            else
            {
                GetPostKey();
            }
            while (!getCommentProc())
                ;

            publishEnd();
        }
        bool getCommentProc()
        {
            var nullSendTimer = new System.Threading.Timer((state) => { AsyncClient.send("\0"); }, null, 20000, 20000);
            var sasaraTimer = new System.Threading.Timer((state) => {
                if (checkBox5.Checked)
                    sasara.speak();
            }, null, 2000, 1000);
            try
            {
                AsyncClient.start(m_ComSrvAddr, m_ComSrvPort);
                AsyncClient.send(String.Format("<thread thread=\"{0}\" version=\"20061206\" res_from=\"-100\"/>\0", m_ComSrvThread));
                AsyncClient.receive(resProc);
                while (!m_stopped) { Thread.Sleep(1000); }
                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine("Message    : " + e.Message);
                Debug.WriteLine("Type       : " + e.GetType().FullName);
                Debug.WriteLine("StackTrace : " + e.StackTrace.ToString());
                say("GetComment() is error");
                return false;
            }
            finally
            {
                nullSendTimer.Dispose();
                sasaraTimer.Dispose();
                AsyncClient.end();
            }
        }

        string resProc(string curr, string prev)
        {
            string xml;
            if (curr.StartsWith("<chat") && curr.EndsWith("</chat>"))
            {
                xml = curr;
            }
            else
            {
                xml = prev + curr;
                prev = "";
            }
            Debug.WriteLine("○データ受信(" + curr.Length.ToString() + "文字)");
            //<thread hoge />\0<chat>情報</chat>\0<chat>情報</chat>\0の形で受信する
            foreach (string line in xml.Split('\0'))
            {
                if (m_stopped) { break; }
                Debug.WriteLine(line);
                //<thread hoge="hoge">は初回送られてくる
                if (line.StartsWith("<thread"))
                {
                    //チケット、コメントサーバー時刻取得
                    //<thread ticket="チケット" server_time="サーバー時刻" hoge="hoge">を取得
                    var th = XElement.Parse(line);

                    m_ComTicket = th.Attribute("ticket").Value;
                    m_SrvTime = th.Attribute("server_time").Value;
                    //コメント処理開始時刻
                    m_DateTimeStart = DateTime.Now;
                    continue;
                }
                if (line.StartsWith("<chat_result"))
                {
                    Debug.WriteLine("(投稿応答)");
                    continue;
                }
                if (!line.EndsWith("</chat>"))
                {
                    //MAX_RECEIVE_SIZEいっぱいに受信した場合等
                    //XMLが閉じていない場合は次回Receive時結合する
                    return line;
                }
                if (line.StartsWith("<chat "))
                {
                    chatProcess(line);
                }
            }
            return prev;
        }

        string chatProcess(string chat_el)
        {
            var xelem = XElement.Parse(chat_el);
            var chat = WebUtility.HtmlDecode(xelem.Value);
            var id = xelem.Attribute("user_id").Value;
            var no = xelem.Attribute("no").Value;
            var premattr = xelem.Attribute("premium");
            string prem;
            if (premattr == null)
                prem = "0";
            else
                prem = premattr.Value;

            if (no != null)
            {
                if (int.Parse(no) <= int.Parse(m_count))
                    return "connect";
                m_count = no;
            }
            string name;
            if (chat.Contains('@') && !chat.StartsWith("Sound)") && !chat.StartsWith("Come"))
            {
                var nameMatch = Regex.Match(chat, "@.*");
                name = nameMatch.Value;
                Debug.WriteLine(name);
                namedic[id] = name;
            }
            else
            {
                name = userIdtoName(id);
            }
 
            dataGridView1.Invoke((Action<string, string, string>)((n, nam, cht) =>
            {
                dataGridView1.Rows.Insert(0, 1);
                dataGridView1.Rows[0].Cells[0].Value = n;
                dataGridView1.Rows[0].Cells[1].Value = cht;
                dataGridView1.Rows[0].Cells[2].Value = nam;
            }), no, name, chat);

            if (gameForm != null && !gameForm.IsDisposed)
            {
                if (chat.StartsWith("Sound)"))
                {
                    gameForm.Play(chat.Replace("Sound)", ""));
                    return "next";
                }
                if (chat.StartsWith("RegisterTheme)"))
                {
                    ringin.RegisterTheme(id, chat.Replace("RegisterTheme)", ""));
                    chat = "テーマを登録しました";
                }
                else
                {
                    ringin.PlayOnce(id);
                }
            }
            if (chat.StartsWith("Code ") && viewForm != null && !viewForm.IsDisposed)
            {
                viewForm.Exec(chat.Substring(5), false);
                return "next";
            }
            else if (chat.StartsWith("Come ") && viewForm != null && !viewForm.IsDisposed)
            {
                viewForm.Exec(chat.Substring(5), true);
                return "next";
            }
            else if (chat.StartsWith("CA ") && viewForm != null && !viewForm.IsDisposed)
            {
                viewForm.Exec("add" + chat.Substring(2), true);
                return "next";
            }
            //dataGridView1.FirstDisplayedCell = dataGridView1.Rows[0].Cells[0];
            if (checkBox5.Checked  && (!checkBox1.Checked || prem != "3"))
                sasara.enqueue(id, chat);
            if (bClient != null && checkBox1.Checked && (!checkBox5.Checked || prem == "3"))
            {
                bClient.AddTalkTask(chat);
            }
            foreach (var item in chat.Split(' '))
            {
                if (item.StartsWith("http"))
                {
                    comboBox1.Invoke((Action<string>)(x =>
                    {
                        if (!comboBox1.Items.Contains(x))
                        {
                            comboBox1.Items.Add(x);
                        }
                    }), item);
                }
            }
            if (igoForm != null && !igoForm.IsDisposed)
            {
                igoForm.Invoke((Action<string>)igoForm.chatToPut, chat);
            }

            if (chat == "/disconnect" && prem == "3")
            {
                obs.EndPublish();
                m_stopped = true;
                return "disconnect";
            }
            else
            {
                return "next";
            }
        }


        #endregion


        #region live functions


        void publishEnd()
        {
            if (!isPublish) return;
            if (this.IsAccessible)
                this.Invoke((Action)timer1.Stop);
            Debug.WriteLine("republish is " + checkBox4.Checked);
            if (checkBox4.Checked)
            {
                var form = new PublishForm(m_cc, true);
                form.Dispose();

                ListenPublish();
            }
            else
            {
                isPublish = false;
            }
        }
        public void AutoNextLive()
        {
            try
            {
                this.LoadDic();
                GetComment();
                while (setNextLive())
                {
                    GetComment();
                    if (m_stopped)
                    {
                        return;
                    }
                }
            }
            finally
            {
                this.SaveDic();
            }
        }
        public void OneLive()
        {
            //if (setNextLive())
            //{
                
            //    timer1.Interval = 29 * 60 * 1000;
            //    this.Invoke((Action)timer1.Start);
            //    say("timer is started");
            //}
            //else
            //{
            //    say("timer is stopped");
            //}

            try
            {
                this.LoadDic();
                GetComment();
            }
            finally
            {
                this.SaveDic();
            }
        }
        //繰り返しm_LiveComIdから放送URLの取得の試行を行う
        bool setNextLive()
        {
            const int interval = 20000;
            const int tryN = 10;
            
            for (int i = 1; i < tryN; i++)
            {
                if (m_stopped)
                {
                    return false;
                }
                var url = getLiveUrl(m_LiveComId);
                if (url != "")
                {
                    m_liveID = url.Replace("http://live.nicovideo.jp/watch/lv", "");
                    if (GetPlayerStatus(m_liveID))
                    {
                        return true;
                    }
                    else
                    {
                        say("GetPlayerStatus()失敗　m_liveID: " + m_liveID + " あと" + (tryN - i) + "回");
                        Thread.Sleep(interval);
                    }
                }
                else
                {
                    say("getLiveUrl()失敗　あと" + (tryN - i) + "回");
                    Thread.Sleep(interval);
                }
            }
            say("再試行を終わります");
            return false;
        }
        //Unixタイムに変換
        public static Int64 GetUnixTime(DateTime targetTime)
        {
            TimeSpan elapsedTime = targetTime.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return (Int64)elapsedTime.TotalSeconds;
        }
        bool SendComment(string comment)
        {
            //m_postkeyを取得
            if (!GetPostKey())
            {
                Debug.WriteLine("GetPostKey() is error");
                return false;
            }

            string anonymous = "";
            if (checkBox6.Checked)
            {
                anonymous = "mail=\"184\"";
            }

            //vpos(放送経過時間[sec]*100)を算出
            //コメントサーバ開始時間
            Int64 serverTimeSpan = Int64.Parse(m_SrvTime) - Int64.Parse(m_base_time);
            Int64 localTimeSpan = GetUnixTime(DateTime.Now) - GetUnixTime(m_DateTimeStart);
            string vpos = ((serverTimeSpan + localTimeSpan) * 100).ToString();

            string param = String.Format("<chat thread=\"{0}\" ticket=\"{1}\" vpos=\"{2}\" postkey=\"{3}\" user_id=\"{4}\" premium=\"1\" {5}>{6}</chat>\0"
                , m_ComSrvThread
                , m_ComTicket
                , vpos
                , m_postkey
                , m_userID
                , anonymous
                , comment);
            AsyncClient.send(param);
            Debug.WriteLine("○投稿します");
            Debug.WriteLine(param);
            return true;
        }
        static bool GetPostKey()
        {
            try
            {
                UInt32 block_no = UInt32.Parse(m_count) / 100;
                string url = String.Format("http://live.nicovideo.jp/api/getpostkey?thread={0}&block_no={1}",
                    m_ComSrvThread, block_no);

                var xdoc = ParseHtml(url);
                string text = xdoc.Root.Value.ToString();
                //応答はプレーンテキストで
                //postkey=ergerwhg54hy4wfwegrghg
                //のように返ってくる
                m_postkey = text.Substring(8, text.Length - 8);
                Debug.WriteLine("getpostkey: " + m_postkey);
            }
            catch (Exception e)
            {
                Debug.WriteLine("Message    : " + e.Message);
                Debug.WriteLine("Type       : " + e.GetType().FullName);
                Debug.WriteLine("StackTrace : " + e.StackTrace.ToString());
                return false;
            }
            return true;
        }
        //procスレッドを起動する
        public void liveStart(Action proc)
        {
            m_stopped = false;
            if (checkBox1.Checked)
            {
                int b_proc = Process.GetProcessesByName("BouyomiChan").Count();
                if (b_proc == 0)
                {
                    MessageBox.Show("棒読みちゃんが起動していません");
                    checkBox1.CheckState = CheckState.Unchecked;
                    return;
                }
                bClient = bClient ?? new FNF.Utility.BouyomiChanClient();

            }
            listenTask = Task.Factory.StartNew(proc, ctokenSrc.Token);
        }
        private string userIdtoName(string id)
        {
            string name;
            if (!namedic.TryGetValue(id, out name)) //辞書に登録してなければ
            {
                return id;
            }
            return name;
        }
        private string getUserName(string id)
        {
            try
            {
                int i;
                if (Int32.TryParse(id, out i)) //生IDならば
                {
                    string url = "http://www.nicovideo.jp/user/" + id;
                    var xdoc = ParseHtml(url);
                    //Debug.WriteLine(xdoc);
                    var ns = xdoc.Root.Name.Namespace;
                    var name = xdoc.Descendants(ns + "div")
                        .Where(el => el.Attribute("class") != null && el.Attribute("class").Value == "profile")
                        .Descendants(ns + "h2").Single().Value;
                    Debug.WriteLine("user name : " + name);
                    namedic[id] = name;
                    return name;
                }
                return id;
            }
            catch
            {
                say("ユーザ名の取得に失敗しました");
                return id;
            }
        }

        private string getLiveUrl(string coid = "1508501")
        {
            try
            {
                string co_url = "http://com.nicovideo.jp/community/co" + coid;
                XDocument xdoc;
                using (var sgml = new SgmlReader() { Href = co_url })
                {
                    xdoc = XDocument.Load(sgml);
                }
                //Debug.WriteLine(xdoc);
                var ns = xdoc.Root.Name.Namespace;
                var lv_url = xdoc.Descendants(ns + "a")
                    .Where(el => el.Attribute("class") != null && el.Attribute("class").Value == "community")
                    .Select(el => el.Attribute("href").Value).SingleOrDefault();

                string url = lv_url ?? "";

                if (url.Contains('?'))
                {
                    url = url.Remove(url.IndexOf('?'));
                }

                return url;
            }
            catch (Exception e)
            {
                say(e.ToString());
                return "";
            }
        }

        void GetToken()
        {
            var url = "http://live.nicovideo.jp/api/getpublishstatus?v=lv" + m_liveID;
            var xdoc = ParseHtml(url);

            if (xdoc.Descendants("getpublishstatus").Single().Attribute("status").Value == "ok")
                m_token = xdoc.Descendants("token").Single().Value;
            else
                m_token = "";
        }
        void SaveDic()
        {
            var sw = new StreamWriter(m_LiveComId + ".txt");
            for (var i = 0; i < namedic.Count; i++)
            {
                var key = namedic.Keys.ElementAt(i);
                var value = namedic.Values.ElementAt(i);
                sw.WriteLine(key + "=" + value);
            }
            sw.Close();
        }
        void LoadDic()
        {
            namedic.Clear();
            var dicfile = m_LiveComId + ".txt";
            if (!File.Exists(dicfile))
                return;
            var sr = new StreamReader(dicfile);
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                var dic = line.Split('=');
                namedic[dic[0]] = dic[1];
            }
            sr.Close();
        }

        void ListenPublish()
        {
            if (m_liveID != null && GetPlayerStatus(m_liveID))
            {
                isPublish = true;
                liveStart(OneLive);
            }
        }
        void SettingOBS()
        {
            var url_profile = @"http://watch.live.nicovideo.jp/api/getfmeprofile?v=lv" + m_liveID;
            var xml = ParseHtml(url_profile);
            var url = xml.Descendants("url").Single().Value;
            var stream = xml.Descendants("stream").Single().Value;
            Debug.WriteLine("url: " + url);
            Debug.WriteLine("stream: " + stream);
            var ini_path = @"C:\Users\hsho_000\AppData\Roaming\OBS\profiles\NicoLive.ini";
            if (!File.Exists(ini_path))
            {
                say("OBS profile - NicoLive.ini - is not found");
                return;
            }
            var ini_lines = File.ReadAllLines(ini_path);
            for (int i = 0; i < ini_lines.Length; i++)
            {
                if (ini_lines[i].StartsWith("URL="))
                    ini_lines[i] = "URL=" + url;
                if (ini_lines[i].StartsWith("PlayPath="))
                    ini_lines[i] = "PlayPath=" + stream;
            }
            File.WriteAllLines(ini_path, ini_lines);
        }


        #endregion


        #region UIevents


        private void BrowseButton_Click(object sender, EventArgs e)
        {
            foreach (string word in Clipboard.GetText().Split(' '))
            {
                if (word.StartsWith("http"))
                    Process.Start(word);
            }

        }
        private void getComment_Click(object sender, EventArgs e)
        {
            //コメント取得 Button
            if (threadIsAlive())
            {
                say("止まっていません");
                return;
            }

            isPublish = false;
            string url = Clipboard.GetText();
            if (url.StartsWith("http://com.nicovideo.jp/community/co"))
            {
                string coid = url.Replace("http://com.nicovideo.jp/community/co", "");
                url = getLiveUrl(coid);
            }
            if (url.StartsWith("http://live.nicovideo.jp/watch/"))
            {
                if (url.Contains('?'))
                {
                    url = url.Remove(url.IndexOf('?'));
                }
            }
            if (url.StartsWith("http://live.nicovideo.jp/watch/co"))
            {
                string coid = url.Replace("http://live.nicovideo.jp/watch/co", "");
                url = getLiveUrl(coid);
            }
            else if (!url.StartsWith("http://live.nicovideo.jp/watch/lv"))
            {
                url = getLiveUrl(); //自分のコミュのページへ生放送をやってないか見に行く
                isPublish = true;
            }
            //放送URLがあれば、その放送のコメントを取得していく
            if (url.StartsWith("http://live.nicovideo.jp/watch/lv"))
            {
                m_liveID = url.Replace("http://live.nicovideo.jp/watch/lv", "");
                if (GetPlayerStatus(m_liveID))
                {
                    liveStart(AutoNextLive);
                }
            }
            else
            {
                say("放送URLではないか、または放送していません。");
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if(checkBox1.Checked)
            {
                int b_proc = Process.GetProcessesByName("BouyomiChan").Count();
                if (b_proc == 1)
                    bClient = bClient ?? new FNF.Utility.BouyomiChanClient();
            }
            else
            {
                if (bClient != null)
                {
                    bClient.Dispose();
                    bClient = null;
                }
            }
        }
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            var url = comboBox1.Text;
            Process.Start(url);
            var title = "";
            if (url.EndsWith("html"))
            {
                var xdoc = ParseHtml(url);
                var ns = xdoc.Root.Name.Namespace;
                title = xdoc.Descendants(ns + "title")
                    .Single().Value;
            }

        }

        private void button8_Click(object sender, EventArgs e)
        {
            var form = new CommunityForm();
            form.ShowDialog();
            form.Dispose();
        }

        private async void textBox2_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.KeyCode != Keys.Enter)
                    return;
                //viewForm.Exec(textBox2.Text, true); return;
                //if (Regex.IsMatch(textBox2.Text, @"\w"))
                //    textBox2.Text = "";
                //return;
                if (m_stopped)
                    return;
                if (textBox2.Text == "")
                    return;
                e.SuppressKeyPress = true;


                Debug.WriteLine("isPublish: " + isPublish);
                if (isPublish)
                {
                    var send = "http://watch.live.nicovideo.jp/api/broadcast/lv" + m_liveID;
                    var body = Uri.EscapeDataString(WebUtility.HtmlEncode(textBox2.Text));
                    textBox2.Text = "";
                    var sendurl = send + "?body=" + body + "&token=" + m_token;
                    var req = (HttpWebRequest)WebRequest.Create(sendurl);
                    req.CookieContainer = m_cc;
                    var res = await req.GetResponseAsync();
                    say(new StreamReader(res.GetResponseStream()).ReadLine());
                }
                else
                {
                    SendComment(textBox2.Text);
                    textBox2.Text = "";
                }
            }
            catch
            {
                say("送信失敗しました");
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            Clipboard.SetText(dataGridView1.CurrentCell.Value.ToString());
        }

        private void 棒読みちゃんToolStrip_Click(object sender, EventArgs e)
        {
            if (!File.Exists(Bouyomi_path))
            {
                say("棒読みちゃんがみつかりませんでした");
                return;
            }
            int proc = Process.GetProcessesByName("BouyomiChan").Count();
            if (proc == 0)
            {
                Process.Start(Bouyomi_path);
                checkBox1.Checked = true;
            }
        }
        private void 新着放送ToolStrip_Click(object sender, EventArgs e)
        {
            m_comID.Clear();
            m_pubID.Clear();
            GetAlertInfo();
            if (!threadIsAlive())
            {
                liveStart(GetProgramInfo);
            }
            else
            {
                say("動いているスレッドがあります");
            }
        }

        private void 取得停止ToolStrip_Click(object sender, EventArgs e)
        {
            m_stopped = true;
            Debug.WriteLine("STOP Button is Clicked");
            say("コメント取得停止中");
        }
        private void 放送ToolStrip_Click(object sender, EventArgs e)
        {
            m_stopped = true;
            var form = new PublishForm(m_cc);
            form.ShowDialog();
            var isLive = form.isLive;
            form.Dispose();

            Debug.WriteLine(isLive);
            if (!isLive) return;
            var timer = new System.Timers.Timer(500);
            timer.Elapsed += (source, ev) =>
                {
                    var xdoc = ParseHtml("http://live.nicovideo.jp/my");

                    var ns = xdoc.Root.Name.Namespace;
                    var lurl = xdoc.Descendants(ns + "a")
                        .Where(el => el.Attribute("title") != null && el.Attribute("title").Value == "生放送ページへ戻る")
                        .Single().Attribute("href").Value;
                    m_liveID = lurl.Replace("http://live.nicovideo.jp/watch/lv", "").Replace("?ref=my_live", "");
                    Debug.WriteLine(m_liveID);
                    if (checkBox2.Checked)
                    {
                        SettingOBS();
                        obs.BeginPublish();
                    }
                    Process.Start(lurl);
                    ListenPublish();
                };
            timer.AutoReset = false;
            timer.Enabled = true;
            timer.Start();
        }
        
        private void 登録枠待ちToolStrip_Click(object sender, EventArgs e)
        {
            try
            {
                using (var sr = new StreamReader("CommnityFile.txt"))
                {
                    while (sr.Peek() != -1)
                    {
                        var item = sr.ReadLine().Split(',');
                        m_comID.Add(item[1]);
                        m_pubID.Add(item[2]);
                    }
                }
            }
            catch
            {
                say("CommnityFile.txtが存在しない、または壊れています");
            }

            GetAlertInfo();
            if (!threadIsAlive())
            {
                liveStart(GetProgramInfo);
            }
            else
            {
                say("動いているスレッドがあります");
            }
        }
        private void スレッド情報ToolStrip_Click(object sender, EventArgs e)
        {
            say(listenTask.Status.ToString());
        }
        private void toolStripSplitButton1_ButtonClick(object sender, EventArgs e)
        {
            if (listenTask != null)
                say(listenTask.Status.ToString());
        }

        private void 検索ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("https://www.google.co.jp/search?hl=ja&q=" + Uri.EscapeUriString(dataGridView1.CurrentCell.Value.ToString()));
        }
        private void 開くToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(dataGridView1.CurrentCell.Value.ToString());
            }
            catch
            {
                say(dataGridView1.CurrentCell.Value.ToString() + "を開けませんでした");
            }
        }
        private void コテハンToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var ibox = new InputBox("コテハンを入力してください");
            ibox.ShowDialog();
            var newname = ibox.Str;
            ibox.Dispose();
            var oldname = dataGridView1.CurrentCell.Value.ToString();

            foreach (var kvp in namedic)
	        {
                if (oldname == kvp.Value)
                {
                    namedic[kvp.Key] = "@" + newname;
                    return;
                }
	        }

        }
        private void スレッド停止ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ctokenSrc.Cancel();
            say("listenTask is cancelling");
        }
        private void ユーザ名の表示ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var kote = dataGridView1.CurrentCell.Value.ToString();
            
            try
            {
                string name = "??";
                foreach (var kvp in namedic)
                {
                    if (kote == kvp.Value)
                    {
                        string url = "http://www.nicovideo.jp/user/" + kvp.Key;
                        var xdoc = ParseHtml(url);
                        //Debug.WriteLine(xdoc);
                        var ns = xdoc.Root.Name.Namespace;
                        name = xdoc.Descendants(ns + "div")
                            .Where(el => el.Attribute("class") != null && el.Attribute("class").Value == "profile")
                            .Descendants(ns + "h2").Single().Value;
                    }
                }
                say(kote + " のユーザ名は " + name + "です");
            }
            catch
            {
                say(kote + " のユーザ名を探せませんでした");
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            var send = "http://watch.live.nicovideo.jp/api/broadcast/lv" + m_liveID;
            var body = Uri.EscapeDataString("【放送がそろそろ終了します】");

            var sendurl = send + "?body=" + body + "&token=" + m_token;
            var xdoc = ParseHtml(sendurl);
            //Debug.WriteLine(xdoc);
            timer1.Stop();
            say("timer is stopping");
        }

        private void 開く放送設定ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("LiveColl.txt");
        }
        private void 開くコミュニティ設定ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("CommnityFile.txt");
        }

        private void プロフィールToolStripMenuItem_Click(object sender, EventArgs e)
        {
            for (var i = 0; i < namedic.Count; i++)
            {
                var key = namedic.Keys.ElementAt(i);
                if (namedic[key] == dataGridView1.CurrentCell.Value.ToString())
                    Process.Start("http://www.nicovideo.jp/user/" + key);
            }
        }

        private void 開く起動設定ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("RunConfig.txt");
        }

        private void クッキー再取得ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            setConfig();
            if (m_cc != null)
                MessageBox.Show("クッキーが取得できたみたいです");
        }

        private void nLEToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!File.Exists(NLE_path))
            {
                say("NLE が見つかりませんでした");
                return;
            }
            Process.Start(NLE_path);
        }

        private void sasaraToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!File.Exists(Sasara_path))
            {
                say("CeVIO Creative Studio が見つかりませんでした");
                return;
            }
            int proc = Process.GetProcessesByName("CeVIO Creative Studio FREE").Count();
            if (proc == 0)
            {
                Process.Start(Sasara_path);
                checkBox5.CheckState = CheckState.Unchecked;
            }
        }

        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {
            int proc = Process.GetProcessesByName( "CeVIO Creative Studio FREE").Count();
            if (checkBox5.Checked)
            {
                if (proc == 0)
                    checkBox5.CheckState = CheckState.Unchecked;
                else if (!sasaraPrepare() || !sasara.init())
                    checkBox5.CheckState = CheckState.Unchecked;
            }
        }
        private void textBox2_TextChanged(object sender, EventArgs e)
        {
        }
        private void プリセットの登録ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            sasara.manualRegisterPreset();
        }
        
        private void 囲碁ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (igoForm == null || igoForm.IsDisposed)
            {
                igoForm = new IgoForm();
                igoForm.Show();
            }
        }

        private void nicoTacticsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (gameForm == null || gameForm.IsDisposed)
            {
                gameForm = new GameForm();
                gameForm.Show();
                ringin.Init(gameForm);
            }
        }

        private void MainForm_Click(object sender, EventArgs e)
        {
        }

        private void dataGridView1_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            var row = e.RowIndex;
            var col = e.ColumnIndex;
            if (col != 2)
                return;
            var cell = dataGridView1.Rows[row].Cells[col].Value.ToString();
            dataGridView1.Rows[row].Cells[col].Value = getUserName(cell);
        }

        private void リセットToolStripMenuItem_Click(object sender, EventArgs e)
        {
            dataGridView1.Rows.Clear();
        }


        #endregion

        private void checkBox6_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void oBSToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!File.Exists(OBS_path))
            {
                say("OBSがみつかりませんでした");
                return;
            }
            int proc = Process.GetProcessesByName("OBS").Count();
            if (proc == 0)
            {
                Process.Start(OBS_path);
                checkBox2.Checked = true;
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked && obs.init())
                obs.BeginPublish();
        }

        private void 全て起動ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            棒読みちゃんToolStrip_Click(null, null);
            sasaraToolStripMenuItem_Click(null, null);
            if (checkBox2.Checked)
                oBSToolStripMenuItem_Click(null, null);
            else
                nLEToolStripMenuItem_Click(null, null);
        }

        private void codeViewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (viewForm == null || viewForm.IsDisposed)
            {
                viewForm = new ViewForm();
                viewForm.Show();
            }
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
                return;

            if (viewForm != null)
                viewForm.Exec(textBox1.Text, true);

            //if (Regex.IsMatch(textBox2.Text, @"\w"))
            //    textBox2.Text = "";
            //return;
        }

    }

    class Interop
    {
        [DllImport("KeyHook.dll")]
        public static extern bool StartHook();
        [DllImport("KeyHook.dll")]
        public static extern bool EndHook();

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        //[DllImport("user32.dll", SetLastError = true)]
        //static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);
        //[StructLayout(LayoutKind.Sequential)]  // アンマネージ DLL 対応用 struct 記述宣言
        //struct INPUT
        //{
        //    public int type;  // 0 = INPUT_MOUSE(デフォルト), 1 = INPUT_KEYBOARD
        //    public MOUSEINPUT mi;
        //}

        //[StructLayout(LayoutKind.Sequential)]  // アンマネージ DLL 対応用 struct 記述宣言
        //struct MOUSEINPUT
        //{
        //    public int dx;
        //    public int dy;
        //    public int mouseData;  // amount of wheel movement
        //    public int dwFlags;
        //    public int time;  // time stamp for the event
        //    public IntPtr dwExtraInfo;
        //}
        //const int MOUSEEVENTF_MOVED = 0x0001;
        //const int MOUSEEVENTF_LEFTDOWN = 0x0002;  // 左ボタン Down
        //const int MOUSEEVENTF_LEFTUP = 0x0004;  // 左ボタン Up
    }

    class Sasara
    {
        int volume = 100;
        string registerPresetName = null;
        int currentCount = 0;
        int speakCount = 0;
        AutomationElement sasaraWindow;
        AutomationElement presetComboBox;
        ValuePattern speechText;
        RangeValuePattern[] voiceAttrs = new RangeValuePattern[7];
        Dictionary<string, string> presetDict = new Dictionary<string, string>();
        Queue<Tuple<string, string>> spQueue = new Queue<Tuple<string, string>>();
        Regex clapPattern = new Regex(@"8{3,}|８{3,}");
        Regex wwwPattern = new Regex("(w|ｗ)+");
        Regex urlPattern = new Regex(@"http://(?:\w|[./?=#])+");
        Regex canSpeak = new Regex(@"\w");

        public Sasara()
        {
            init();
            Debug.WriteLine("Sasara is create");
        }
        public bool init()
        {
            var appName = "CeVIO Creative Studio FREE";
            // root から findfirst で探すとなぜか棒読みちゃんの ipc client が登録されて例外をはいてしまう
            var hwnd = Process.GetProcessesByName(appName).Single().MainWindowHandle;
            sasaraWindow = AutomationElement.FromHandle(hwnd);
            if (sasaraWindow == null) return false;

            var grid = sasaraWindow.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.AutomationIdProperty, "dataGrid"));
            if (grid == null) return false;
            var gridPatt = (GridPattern)grid.GetCurrentPattern(GridPattern.Pattern);
            var cell = gridPatt.GetItem(0, 2);
            var cellval = (ValuePattern)cell.GetCurrentPattern(ValuePattern.Pattern);
            speechText = cellval;

            //事前にセルを選択する　選択するとスライダーが変わる
            speechText.SetValue("プリセット");

            var voiceEditor = sasaraWindow.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.ClassNameProperty, "VoiceEditor"));
            var sliders = voiceEditor.FindAll(TreeScope.Descendants, new PropertyCondition(AutomationElement.ClassNameProperty, "Slider"));
            for (int i = 0; i < 7; i++)
                voiceAttrs[i] = (RangeValuePattern)sliders[i].GetCurrentPattern(RangeValuePattern.Pattern);

            presetComboBox = voiceEditor.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.ClassNameProperty, "ComboBox"));

            return true;
        }

        public bool IsReady { get { return sasaraWindow != null; } }

        public void speak()
        {
            if (spQueue.Count > 0 && currentCount > speakCount)
            {
                var hwnd = Interop.GetForegroundWindow();
                var comment = spQueue.Dequeue();
                var id = comment.Item1;
                var chat = comment.Item2;
                chat = chat.Replace('（', '(').Replace('）', ')');
                currentCount = 0;

                Interop.StartHook();
                try
                {
                    int[] attrs;
                    if (commandProc(id, ref chat, out attrs))
                    {
                        speakCount = chat.Length / 4;
                        selectPresetProc("init");
                        selectPresetProc(presetIs(id));
                        speakProc(chat, attrs);
                    }
                    Interop.SetForegroundWindow(hwnd);
                }
                finally
                {
                    Interop.EndHook();
                }
            }
            //Debug.WriteLine(currentCount);
            currentCount++;
        }
        void speakProc(string chat, int[] attrs)
        {
            if (chat.Length >= 100)
                chat = chat.Remove(0, 96) + "以下略";
           
            speechText.SetValue(chat);
            double size;
            if (volume != 100)
                size = volume;
            else
                size = voiceAttrs[0].Current.Value;
            voiceAttrs[0].SetValue(0);
            if (attrs != null)
                for (int i = 0; i < voiceAttrs.Count(); i++)
                    voiceAttrs[i].SetValue(attrs[i]);
            else
                voiceAttrs[0].SetValue(size);
            if (registerPresetName != null)
            {
                registerPresetProc();
                registerPresetName = null;
            }
            volume = 100;
        }

        bool commandProc(string id, ref string chat, out int[] attrs)
        {
            attrs = null;
            var size = regexMatch(ref chat, @"音量\((\d{1,3})\)");
            var atts = regexMatch(ref chat, @"Register\(((\d|:){7})\)");
            var pres = regexMatch(ref chat, @"Select\((\w{1,14})\)");
            var test = regexMatch(ref chat, @"Voice\(((\d|:){7})\)");
            if (size != null)
                voiceSizeCommand(size);
            else if (atts != null)
                attrs = registerPresetCommand(atts, ref chat);
            else if (pres != null)
                presetDict[id] = pres;
            else if (test != null)
                attrs = testPresetCommand(test);

            return (chat != String.Empty);
        }

        string presetIs(string id)
        {
            if (presetDict.ContainsKey(id))
                return presetDict[id];
            else
                return "default";
        }

        void voiceSizeCommand(string size)
        {
            var vl = int.Parse(size);
            if (vl >= 0 && vl <= 100)
                volume = vl;
        }

        int[] registerPresetCommand(string atts, ref string chat)
        {
            if (presetIsExists(chat))
            {
                chat += "は既に登録されています。";
            }
            else
            {
                registerPresetName = chat;
                chat += "プリセットを登録します。";
            }
            return atts.Select((attr) => { return (attr - '0') * 10; }).ToArray();
        }

        int[] testPresetCommand(string atts)
        {
            registerPresetName = "test";
            return atts.Select((attr) => { return (attr - '0') * 10; }).ToArray();
        }

        void selectPresetProc(string pres)
        {
            var presets = (ExpandCollapsePattern)presetComboBox.GetCurrentPattern(ExpandCollapsePattern.Pattern);
            presets.Expand();
            foreach (AutomationElement ps in presetComboBox.FindAll(TreeScope.Children, PropertyCondition.TrueCondition))
            {
                var text = TreeWalker.RawViewWalker.GetFirstChild(ps);
                if (text.Current.Name == pres)
                {
                    var preset = (SelectionItemPattern)ps.GetCurrentPattern(SelectionItemPattern.Pattern);
                    preset.Select();
                    return;
                }
            }
        }

        bool presetIsExists(string pres)
        {
            foreach (AutomationElement ps in presetComboBox.FindAll(TreeScope.Children, PropertyCondition.TrueCondition))
            {
                var text = TreeWalker.RawViewWalker.GetFirstChild(ps);
                if (text.Current.Name == pres)
                    return true;
            };
            return false;
        }
        void registerPresetProc()
        {
            //（追加）を選択するとダイアログが開きフォーカスできなくて落ちる
            //selectPresetProc("(追加)");
            //var addpreWindow = sasaraWindow.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.ClassNameProperty, "Window"));
            //var textbox = addpreWindow.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.AutomationIdProperty, "textBox"));
            //var preset = (ValuePattern)textbox.GetCurrentPattern(ValuePattern.Pattern);
            //preset.SetValue(registerPresetName);
            //var btn = addpreWindow.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.AutomationIdProperty, "okButton"));
            //var okbtn = (InvokePattern)btn.GetCurrentPattern(InvokePattern.Pattern);
            //okbtn.Invoke();
        }

        public void manualRegisterPreset()
        {
            var addpreWindow = sasaraWindow.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.ClassNameProperty, "Window"));
            var textbox = addpreWindow.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.AutomationIdProperty, "textBox"));
            var preset = (ValuePattern)textbox.GetCurrentPattern(ValuePattern.Pattern);
            preset.SetValue(registerPresetName);
            var btn = addpreWindow.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.AutomationIdProperty, "okButton"));
            var okbtn = (InvokePattern)btn.GetCurrentPattern(InvokePattern.Pattern);
            okbtn.Invoke();
        }

        string regexMatch(ref string chat, string pattern)
        {
            var mc = Regex.Match(chat, pattern);
            if (mc.Success)
            {
                chat = chat.Replace(mc.Value, "");
                return convNarrow(mc.Groups[1].Value);
            }
            else
            {
                return null;
            }
        }
        string convNarrow(string nstr)
        {
            var unibytes = Encoding.Unicode.GetBytes(nstr);
            
            for (var i = 0; i < nstr.Length; i++)
            {
                if (unibytes[i * 2] >= 0x10 && unibytes[i * 2] <= 0x1a)
                {
                    unibytes[i * 2 + 0] += 0x20;
                    unibytes[i * 2 + 1] -= 0xff;
                }
            }
            return Encoding.Unicode.GetString(unibytes);   
        }
        public void enqueue(string id, string chat)
        {

            chat = urlPattern.Replace(chat, "URLです");
            chat = wwwPattern.Replace(chat, "エヘヘ");
            chat = clapPattern.Replace(chat, "パチパチ");
            chat = chat.Replace("_", "").Replace("ᴗ", "");
            if (!canSpeak.IsMatch(chat))
                chat = "読めませんよ？";

            spQueue.Enqueue(Tuple.Create<string, string>(id, chat));
        }
    }

    class Ringin
    {
        GameForm gameForm;
        HashSet<string> first = new HashSet<string>();
        Dictionary<string, string> theme;
        const string themefile = "RinginThemes.txt";

        public Ringin()
        {
            theme = readThemeDict();
        }

        public void Init(GameForm form)
        {
            gameForm = form;
        }
        public void Clear()
        {
            first.Clear();
        }

        public void PlayOnce(string id)
        {
            if (first.Add(id) && theme.ContainsKey(id))
                gameForm.Play(theme[id]);
        }

        public void RegisterTheme(string id, string theme)
        {
            this.theme[id] = theme;
        }

        Dictionary<string, string> readThemeDict()
        {
            var dict = new Dictionary<string, string>();
            if (File.Exists(themefile))
                File.ReadAllLines(themefile).ToList().ForEach(x =>
                {
                    var dic = x.Split('=');
                    dict.Add(dic[0], dic[1]);
                });

            return dict;
        }

        public void WriteThemeDict()
        {
            File.WriteAllLines(themefile,
                theme.Select(x => x.Key + "=" + x.Value));
        }
    }

    class OBS
    {
        IntPtr hWnd;
        AutomationElement obsWindow;

        public OBS()
        {
            var appName = "OBS";
            var proc = Process.GetProcessesByName(appName).Count();
            if (proc != 0)
                init();
        }
        
        public bool init()
        {
            var appName = "OBS";
            // root から findfirst で探すとなぜか棒読みちゃんの ipc client が登録されて例外をはいてしまう
            var proc = Process.GetProcessesByName(appName);
            if (proc.Count() == 0)
                return false;
            hWnd = proc.Single().MainWindowHandle;
            obsWindow = AutomationElement.FromHandle(hWnd);
            if (obsWindow == null)
                return false;

            Debug.WriteLine("OBS is exist");
            return true;
        }

        public void BeginPublish()
        {
            if (obsWindow == null) return;
            Interop.SetForegroundWindow(hWnd);
            //Thread.Sleep(500);

            SendKeys.SendWait("%"); // Alt
            SendKeys.SendWait("p"); // P
            var backmenu = obsWindow.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.NameProperty, "Backup"));
            if (backmenu == null)
            {
                Debug.WriteLine("SendKey to OBS is failed");
                return;
            }
            var back = (InvokePattern)backmenu.GetCurrentPattern(InvokePattern.Pattern);
            back.Invoke();

            SendKeys.SendWait("%"); // Alt
            SendKeys.SendWait("p"); // P
            var nicomenu = obsWindow.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.NameProperty, "NicoLive"));
            var nico = (InvokePattern)nicomenu.GetCurrentPattern(InvokePattern.Pattern);
            nico.Invoke();

            var startBtn = obsWindow.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.NameProperty, "配信開始"));
            var start = (InvokePattern)startBtn.GetCurrentPattern(InvokePattern.Pattern);
            start.Invoke();
        }

        public void EndPublish()
        {
            if (obsWindow == null) return;
            var stopBtn = obsWindow.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.NameProperty, "配信停止"));
            if (stopBtn == null) return;
            var stop = (InvokePattern)stopBtn.GetCurrentPattern(InvokePattern.Pattern);
            stop.Invoke();
        }

        public bool IsReady
        {
            get
            {
                return obsWindow != null;
            }
        }
    }

    class AsyncClient
    {
        static Socket client;
        static bool isStopped;
        static string prevResponse = "";

        public static void start(string address, string port)
        {
            try
            {
                IPAddress hostaddr = Dns.GetHostEntry(address).AddressList[0];
                IPEndPoint ephost = new IPEndPoint(hostaddr, int.Parse(port));
                client = new Socket(AddressFamily.InterNetwork,
                    SocketType.Stream, ProtocolType.Tcp);

                client.Connect(ephost);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
        }

        public static void end()
        {
            isStopped = true;
            if (client != null)
                client.Disconnect(false);
        }

        public static void receive(Func<string, string, string> func)
        {
            try
            {
                isStopped = false;
                var evt = new SocketAsyncEventArgs();
                evt.Completed += new EventHandler<SocketAsyncEventArgs>(
                    (o, e) => { ReceiveCompleted(e, func); });
                evt.SetBuffer(new byte[1024], 0, 1024);

                if (!client.ReceiveAsync(evt))
                {
                    ReceiveCompleted(evt, func);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        static void ReceiveCompleted(SocketAsyncEventArgs e, Func<string, string, string> func)
        {
            if (e.LastOperation == SocketAsyncOperation.Receive)
            {
                if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
                {
                    var res = Encoding.UTF8.GetString(e.Buffer, e.Offset, e.BytesTransferred);
                    prevResponse = func(res, prevResponse);
                    Debug.WriteLine(e.BytesTransferred + " byte receive");
                }
                else
                {
                    Debug.WriteLine(e.BytesTransferred + " byte received");
                    Debug.WriteLine(e.SocketError);
                }
            }

            if (isStopped) return;
            if (!client.ReceiveAsync(e))
            {
                ReceiveCompleted(e, func);
            }
        }

        public static void send(String data)
        {
            byte[] byteData = Encoding.UTF8.GetBytes(data);

            client.Send(byteData, 0, byteData.Length, 0);
        }
    }
}
