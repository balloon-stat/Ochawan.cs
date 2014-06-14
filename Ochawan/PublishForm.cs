using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Xml.Linq;
using System.Net;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Sgml;


namespace Ochawan
{
    public partial class PublishForm : Form
    {
        public bool isLive = false;
        string co_id = "co1508501";
        Live live;
        CookieContainer m_cc;       
        public PublishForm(CookieContainer cc, bool auto=false)
        {
            InitializeComponent();
            m_cc = cc;
            live = new Live();
            if (File.Exists("LiveColl.txt"))
            {
                live.Parse(File.ReadAllText("LiveColl.txt"));
            }
            foreach (var item in live.collection)
            {
                listBox1.Items.Add(item.name);
            }
            var collection = new string[] { "", "一般(その他)", "政治", "動物", "料理",
                "演奏してみた", "歌ってみた", "踊ってみた", "描いてみた", "講座", "ゲーム", "動画紹介", "R18"};
            foreach (var item in collection)
	        {
                comboBox1.Items.Add(item);
            }
            if (listBox1.Items.Count == 0)
            {
                var name = InsertName(0);
                live.collection.Add(new Live(name));
                return;
            }
            var index = 0;
            try
            {
                index = int.Parse(File.ReadAllText("selected_index.txt"));
                if (index < 0 || index >= listBox1.Items.Count)
                    index = -1;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                Debug.WriteLine("selected_index.txt is not exist");
            }
            listBox1.SelectedIndex = index;
            this.ActiveControl = listBox1;
            if (auto)
            {
                button1_Click(null, null);
            }
	    }

        private void button1_Click(object sender, EventArgs e)
        {
            string default_community = co_id;
            string title = SelectLive.title;
            string description = SelectLive.description.Replace(Environment.NewLine, "<br />");
            string tags = SelectLive.tags;
            List<string> livetag = SelectLive.livetag;
            List<bool> taglock = SelectLive.taglock;
            #region request
            string editstreamurl = "http://live.nicovideo.jp/editstream";
            Encoding enc = Encoding.GetEncoding("utf-8");
            string boundary = System.Environment.TickCount.ToString(); // 区切り文字

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(editstreamurl);
            req.Method = "POST";
            req.ContentType = "multipart/form-data; boundary=" + boundary;
            req.CookieContainer = m_cc;

            string postData = "", postDataStep2 = "", postDataStep1 = "", postDataStep1p = "", postDataDes = "", postDataStep2b = "";

            postData = "--" + boundary + "\r\n" +
                "Content-Disposition: form-data; name=\"is_wait\"\r\n\r\n" +
                "\r\n" +
                "--" + boundary + "\r\n" +
                "";
            string tag = "";
            for (var i = 0; i < 10; i++)
            {
                if (livetag[i] != "")
                {
                    tag += "Content-Disposition: form-data; name=\"livetags" + (i + 1) + "\"\r\n\r\n" +
                           livetag[i] + "\r\n" +
                           "--" + boundary + "\r\n";
                }
                if (taglock[i])
                {
                    tag += "Content-Disposition: form-data; name=\"taglock" + (i + 1) + "\"\r\n\r\n" +
                           "ロックする\r\n" +
                           "--" + boundary + "\r\n";
                }
            }

            #region postData
            postDataStep1p = "" +
                   "Content-Disposition: form-data; name=\"usecoupon\"\r\n\r\n" +
                   "\r\n" +
                   "--" + boundary + "\r\n" +
                   "Content-Disposition: form-data; name=\"title\"\r\n\r\n" +
                   title + " \r\n" +
                   "--" + boundary + "\r\n" +
                   "Content-Disposition: form-data; name=\"default_community\"\r\n\r\n" +
                   default_community + "\r\n" +
                   "--" + boundary + "\r\n" +
                   "Content-Disposition: form-data; name=\"tags[]\"\r\n\r\n" +
                   tags + "\r\n" +
                   "--" + boundary + "\r\n" +

                   tag +

                   "Content-Disposition: form-data; name=\"all_remain_point\"\r\n\r\n" +
                   "off\r\n" +
                   "--" + boundary + "\r\n" +
                   "Content-Disposition: form-data; name=\"reserve_start_ymd\"\r\n\r\n" +
                   "off\r\n" +
                   "--" + boundary + "\r\n" +
                   "Content-Disposition: form-data; name=\"reserve_start_h\"\r\n\r\n" +
                   "off\r\n" +
                   "--" + boundary + "\r\n" +
                   "Content-Disposition: form-data; name=\"reserve_start_i\"\r\n\r\n" +
                   "off\r\n" +
                   "--" + boundary + "\r\n" +
                   "Content-Disposition: form-data; name=\"reserve_stream_time\"\r\n\r\n" +
                   "off\r\n" +
                   "--" + boundary + "\r\n" +
                //"Content-Disposition: form-data; name=\"face\"\r\n\r\n" +
                //"off\r\n" +
                //"--" + boundary + "\r\n" +
                //"Content-Disposition: form-data; name=\"totue\"\r\n\r\n" +
                //"off\r\n" +
                //"--" + boundary + "\r\n" +
                //"Content-Disposition: form-data; name=\"reserved\"\r\n\r\n" +
                //"\r\n" +
                //"--" + boundary + "\r\n" +
                //"Content-Disposition: form-data; name=\"community_only\"\r\n\r\n" +
                //"\r\n" +
                //"--" + boundary + "\r\n" +
                   "Content-Disposition: form-data; name=\"timeshift_enabled\"\r\n\r\n" +
                   "1\r\n" +
                   "--" + boundary + "\r\n" +
                //"name=\"timeshift_disabled\"\r\n\r\n" +
                //"\r\n" +
                //"--" + boundary + "\r\n" +
                   "Content-Disposition: form-data; name=\"twitter_disabled\"\r\n\r\n" +
                   "0\r\n" +
                   "--" + boundary + "\r\n" +
                   "Content-Disposition: form-data; name=\"input_twitter_tag\"\r\n\r\n" +
                   "\r\n" +
                   "--" + boundary + "\r\n" +
                   "Content-Disposition: form-data; name=\"twitter_tag\"\r\n\r\n" +
                //"#co1508501\r\n" +
                   "\r\n" +
                   "--" + boundary + "\r\n" +
                   "Content-Disposition: form-data; name=\"ad_enable\"\r\n\r\n" +
                   "0\r\n" +
                   "--" + boundary + "\r\n" +
                   "Content-Disposition: form-data; name=\"rights[0][code]\"\r\n\r\n" +
                   "0\r\n" +
                   "--" + boundary + "\r\n" +
                   "Content-Disposition: form-data; name=\"rights[0][title]\"\r\n\r\n" +
                   "0\r\n" +
                   "--" + boundary + "\r\n" +
                   "Content-Disposition: form-data; name=\"rights[0][artist]\"\r\n\r\n" +
                   "0\r\n" +
                   "--" + boundary + "\r\n" +
                   "Content-Disposition: form-data; name=\"rights[0][lyric]\"\r\n\r\n" +
                   "0\r\n" +
                   "--" + boundary + "\r\n" +
                   "Content-Disposition: form-data; name=\"rights[0][composer]\"\r\n\r\n" +
                   "0\r\n" +
                   "--" + boundary + "\r\n" +
                   "Content-Disposition: form-data; name=\"rights[0][time]\"\r\n\r\n" +
                   "0\r\n" +
                   "--" + boundary + "\r\n" +
                   "";
            postDataStep2b = "" +
                "Content-Disposition: form-data; name=\"public_status\"\r\n\r\n" +
                "0\r\n" +
                "--" + boundary + "\r\n" +
                "Content-Disposition: form-data; name=\"kiyaku\"\r\n\r\n" +
                "true\r\n" +
                "--" + boundary + "\r\n" +
                "";
            #endregion
            postDataDes = "" +
                   "Content-Disposition: form-data; name=\"description\"\r\n\r\n" +
                   description + "\r\n" +
                   "--" + boundary + "\r\n";
            postDataStep1 = postDataDes + postDataStep1p;
            //Debug.WriteLine(postData + postDataStep1);

            byte[] startData = enc.GetBytes(postData + postDataStep1);
            string postDataStep0 = "\r\n--" + boundary + "--\r\n";
            byte[] endData = enc.GetBytes(postDataStep0);

            Stream reqStream = req.GetRequestStream();
            reqStream.Write(startData, 0, startData.Length);
            reqStream.Write(endData, 0, endData.Length);
            reqStream.Close();

            HttpWebResponse res = (HttpWebResponse)req.GetResponse();
            Stream resStream = res.GetResponseStream();
            //StreamReader smr = new StreamReader(resStream, enc);
            //File.WriteAllText("temp.html", smr.ReadToEnd());
            //Process.Start(@"C:\Users\Yuya\AppData\Local\Google\Chrome\Application\chrome.exe",
            //    Directory.GetCurrentDirectory() + "/temp.html");
            //smr.Close();
            //return;

            try
            {
                //XDocument xml;
                using (var sgmlReader = new SgmlReader { DocType = "HTML", CaseFolding = CaseFolding.ToLower })
                {
                    StreamReader sr = new StreamReader(resStream, enc);

                    sgmlReader.InputStream = sr;
                    var xml = XDocument.Load(sgmlReader);
                    XNamespace ns = xml.Root.Name.Namespace;
                    var confirm = xml.Descendants(ns + "input")
                        .Where(el => el.Attribute("name") != null && el.Attribute("name").Value == "confirm")
                        .Select(es => es.Attribute("value").Value).SingleOrDefault();

                    Debug.WriteLine(confirm);
                    postDataStep2 = "" +

                        //Content-Disposition: form-data; name="confirm"
                        //ulck_759107906
                        //-----------------------------7db5d34804e4
                        //Content-Disposition: form-data; name="back"

                        //false
                        "Content-Disposition: form-data; name=\"confirm\"\r\n\r\n" +
                        //"\r\n" +
                        //"ulck_759107906\r\n--" + boundary + "\r\n" +
                        confirm + "\r\n" +
                        "--" + boundary + "\r\n" +

                        "Content-Disposition: form-data; name=\"back\"\r\n\r\n" +
                        "false\r\n" +
                        "--" + boundary + "\r\n" +
                        //"Content-Disposition: form-data; name=\"back\"\r\n\r\n" +
                        //"\r\n" +
                        //"false--" + boundary + "\r\n" +
                        "";
                    description = xml.Descendants(ns + "input")
                        .Where(el => el.Attribute("name") != null && el.Attribute("name").Value == "description")
                        .Select(es => es.Attribute("value").Value).SingleOrDefault();

                    postDataDes = "" +
                            "Content-Disposition: form-data; name=\"description\"\r\n\r\n" +
                            description + "\r\n" +
                            "--" + boundary + "\r\n";
                    postDataStep1 = postDataDes + postDataStep1p;

                    //foreach (var item in xml.Descendants(ns + "input"))
                    //{
                    //    Debug.WriteLine(item.Value);
                    //}
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                throw;
            }
            //Debug.WriteLine(postData + postDataStep2 + postDataStep1 + postDataStep2b);

            startData = enc.GetBytes(postData + postDataStep2 + postDataStep1 + postDataStep2b);
            //string postDataStep0 = "\r\n--" + boundary + "--\r\n";
            //byte[] endData = enc.GetBytes(postDataStep0);

            req = (HttpWebRequest)WebRequest.Create(editstreamurl);
            req.Method = "POST";
            req.ContentType = "multipart/form-data; boundary=" + boundary;
            req.CookieContainer = m_cc;

            reqStream = req.GetRequestStream();
            reqStream.Write(startData, 0, startData.Length);
            reqStream.Write(endData, 0, endData.Length);
            reqStream.Close();

            res = (HttpWebResponse)req.GetResponse();
            resStream = res.GetResponseStream();
            XDocument xdoc;
            using (var srr = new StreamReader(resStream, enc))
            using (var sgmlReader = new SgmlReader { DocType = "HTML", CaseFolding = CaseFolding.ToLower })
            {
                sgmlReader.InputStream = srr;
                xdoc = XDocument.Load(sgmlReader);
            }
            #endregion
            XNamespace xns = xdoc.Root.Name.Namespace;
            var doc_title = xdoc.Descendants(xns + "title")
                                .Select(el => el.Value).Single();
            if (!doc_title.StartsWith(title))
            {
                Debug.WriteLine(doc_title);
                Debug.WriteLine("枠の取得に失敗しました");
                File.WriteAllText("temp.html", xdoc.ToString());
                Process.Start(Directory.GetCurrentDirectory() + "/temp.html");
                //Process.Start(editstreamurl);
                return;
            }

            this.isLive = true;

            this.Close();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex == -1)
                return;
            var selected = this.SelectLive;
            textBox1.Text = selected.title;
            textBox2.Text = selected.description;
            comboBox1.SelectedItem = selected.tags;
            textBox3.Text = selected.livetag[0];
            checkBox1.Checked = selected.taglock[0];

            textBox4.Text = selected.livetag[1];
            checkBox2.Checked = selected.taglock[1];

            textBox5.Text = selected.livetag[2];
            checkBox3.Checked = selected.taglock[2];
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var index = listBox1.SelectedIndex;
            var name = InsertName(index);
            live.collection.Insert(index, new Live(name));
            listBox1.SelectedItem = name;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (listBox1.Items.Count == 1)
            {
                return;
            }
            var sel = listBox1.SelectedIndex;
            live.collection.RemoveAt(sel);
            listBox1.Items.RemoveAt(sel);
            if (sel > listBox1.Items.Count - 1)
                sel = sel - 1;
            listBox1.SelectedIndex = sel;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            var ix = listBox1.SelectedIndex;
            ix++;
            var name = InsertName(ix);
            if (name.StartsWith("放送"))
            {
                listBox1.Items.RemoveAt(ix);
                name = SelectLive.name;
                name = Regex.Replace(name, "[0-9]+", "");
                var i = 1;
                while(listBox1.Items.Contains(name+i))
                    i++;
                name = name + i;
                listBox1.Items.Insert(ix, name);
            }
            live.Clone(ix - 1, name);
            listBox1.SelectedIndex = ix;
        }

        private Live SelectLive
        {
            get
            {
                return live.collection[listBox1.SelectedIndex];
            }
        }

        private string InsertName(int index)
        {
            var ibox = new InputBox("項目名を入力してください");
            ibox.ShowDialog();
            string name = ibox.Str;
            ibox.Dispose();
            if (listBox1.Items.Contains(name))
            {
                var i = 1;
                while (listBox1.Items.Contains(name+i))
                {
                    i++;
                }
                name = name + i;
            }
            listBox1.Items.Insert(index, name);
            return name;
        }
        #region input
        private void textBox1_Leave(object sender, EventArgs e)
        {
            SelectLive.title = textBox1.Text;
        }

        private void textBox2_Leave(object sender, EventArgs e)
        {
            var count = textBox2.Text.Replace(Environment.NewLine, "<br />").Length;
            if (count > 1000)
                MessageBox.Show("番組詳細は最大1000文字です。現在" + count + "文字");
            SelectLive.description = textBox2.Text;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectLive.tags = comboBox1.Text;
        }
        
        private void textBox3_Leave(object sender, EventArgs e)
        {
            SelectLive.livetag[0] = textBox3.Text;
        }
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            SelectLive.taglock[0] = checkBox1.Checked;
        }
        private void textBox4_Leave(object sender, EventArgs e)
        {
            SelectLive.livetag[1] = textBox4.Text;
        }
        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            SelectLive.taglock[1] = checkBox2.Checked;
        }
        private void textBox5_Leave(object sender, EventArgs e)
        {
            SelectLive.livetag[2] = textBox5.Text;
        }
        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            SelectLive.taglock[2] = checkBox3.Checked;
        }
        #endregion
        private void PublishForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            File.WriteAllText("selected_index.txt", listBox1.SelectedIndex.ToString());
            var sw = new StreamWriter("LiveColl.txt");
            foreach (var item in live.collection)
            {
                sw.WriteLine("### name");
                sw.WriteLine(item.name);
                sw.WriteLine("*title");
                sw.WriteLine(item.title);
                sw.WriteLine("*description");
                sw.WriteLine(item.description);
                sw.WriteLine("*tags");
                sw.WriteLine(item.tags);
                for (var i = 0; item.livetag[i] != ""; i++)
                {
                    sw.WriteLine("*livetag");
                    sw.WriteLine(item.livetag[i]);
                    if (item.taglock[i])
                    {
                        sw.WriteLine("ロックする");
                    }
                    else
                    {
                        sw.WriteLine("ロックしない");
                    }
                }
            }
            sw.Close();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            var xdoc = MainForm.ParseHtml("http://live.nicovideo.jp/my");
            var ns = xdoc.Root.Name.Namespace;
            var lastliveurl = xdoc.Descendants(ns + "a")
                .Where(el => el.Attribute("href") != null && el.Attribute("href").Value.StartsWith("http://live.nicovideo.jp/editstream/lv"))
                .First().Attribute("href").Value;
            var url = "http://live.nicovideo.jp/editstream?reuseid=";
            xdoc = MainForm.ParseHtml(url + Regex.Match(lastliveurl, "[0-9]+").Value);
            
            var title = ChoseInput(xdoc, "title");
            var description = xdoc.Descendants(ns + "textarea").Single().Value.Replace("<br />", Environment.NewLine);
            var default_community = ChoseSelect(xdoc, "default_community");
            var tags = ChoseSelect(xdoc, "tags[]");
            var livetag = new List<string>();
            var taglock = new List<bool>();
            for (var i = 0; i < 10; i++)
            {
                if (xdoc.Descendants(ns + "input")
                      .Any(el => el.Attribute("name") != null && el.Attribute("name").Value == "livetags" + (i + 1)))
                {
                    livetag.Add(ChoseInput(xdoc, "livetags" + (i + 1)));
                    taglock.Add(xdoc.Descendants(ns + "input")
                        .Where(el => el.Attribute("name") != null && el.Attribute("name").Value == "taglock" + (i + 1))
                        .Single().Attribute("checked") != null);
                }
                else
                {
                    livetag.Add("");
                    taglock.Add(false);
                }
            }
            SelectLive.title = title;
            SelectLive.description = description;
            SelectLive.tags = tags;
            SelectLive.livetag = livetag;
            SelectLive.taglock = taglock;
            var idx = listBox1.SelectedIndex;
            listBox1.SelectedIndex = -1;
            listBox1.SelectedIndex = idx;
        }
        private string ChoseInput(XDocument xdoc, string name)
        {
            var ns = xdoc.Root.Name.Namespace;
            return xdoc.Descendants(ns + "input")
                .Where(el => el.Attribute("name") != null && el.Attribute("name").Value == name)
                .Select(el => el.Attribute("value").Value).Single();
        }
        private string ChoseSelect(XDocument xdoc, string name)
        {
            var ns = xdoc.Root.Name.Namespace;
            return xdoc.Descendants(ns + "select")
                .Where(el => el.Attribute("name") != null && el.Attribute("name").Value == name)
                .Descendants(ns + "option")
                .Where(el => el.Attribute("selected") != null)
                .Single().Attribute("value").Value;
        }

    }

    class Live
    {
        public List<Live> collection { get; set; }

        public string name;
        public string title = "";
        public string description = "";
        public string tags = "";
        public List<string> livetag = new List<string>();
        public List<bool> taglock = new List<bool>();
        
        public Live()
        {
            collection = new List<Live>();
        }
        public Live(string name)
        {
            this.name = name;
            for (var i = 0; i < 10; i++)
            {
                livetag.Add("");
                taglock.Add(false);
            }
        }
        public void Clone(int index, string name)
        {
            var souce = collection.ElementAt(index);
            collection.Insert(index+1, new Live(name)
            {
                title = souce.title,
                description = souce.description,
                tags = souce.tags,
                livetag = souce.livetag,
                taglock = souce.taglock
            });
        }
        public void Parse(string file_str)
        {
            var nl = Environment.NewLine;
            var col = Regex.Split(file_str, "### name" + nl).Skip(1);
            foreach (var item in col)
            {
                var name = item.Remove(item.IndexOf(nl));
                string title = "", description = "", tags = "";
                List<string> livetag = new List<string>();
                List<bool> taglock = new List<bool>();
                foreach (var it in item.Split('*'))
                {
                    title = partOf(it, "title") ?? title;
                    description = partOf(it, "description") ?? description;
                    tags = partOf(it, "tags") ?? tags;
                    var tag = partOf(it, "livetag");
                    if (tag != null)
                    {
                        livetag.Add(tag.Remove(tag.IndexOf(nl)));
                        taglock.Add(tag.EndsWith("ロックする"));
                    }
                }
                var cnt = livetag.Count;
                for (var i = 0; cnt + i < 10; i++)
                {
                    livetag.Add("");
                    taglock.Add(false);
                }
                collection.Add(new Live(name)
                {
                    title = title,
                    description = description,
                    tags = tags,
                    livetag = livetag,
                    taglock = taglock
                });
            }
        }
        string partOf(string str, string part)
        {
            var nl = Environment.NewLine;
            if (str.StartsWith(part))
            {
                if (str.EndsWith(nl))
                {
                    str = str.Remove(str.Length - 2);
                }
                return str.Replace(part + nl, "");
            }
            else
            {
                return null;
            }
        }
    }
}
