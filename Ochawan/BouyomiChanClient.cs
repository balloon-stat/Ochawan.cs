//棒読みちゃんに接続して読み上げを行うためのクラスです。
//ご自由にお使いください。
using System;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;

namespace FNF.Utility {

    /// <summary>
    /// 声の種類。
    /// </summary>
    public enum VoiceType { Default = 0, Female1 = 1, Female2 = 2, Male1 = 3, Male2 = 4, Imd1 = 5, Robot1 = 6, Machine1 = 7, Machine2 = 8 }

    /// <summary>
    /// 棒読みちゃんへ接続するためのクラス。
    /// </summary>
    public class BouyomiChanClient : IDisposable {

        private IpcClientChannel    ClientChannel;
        private BouyomiChanRemoting RemotingObject;

        /// <summary>
        /// オブジェクト生成。
        /// 利用後にはDispose()で開放してください。
        /// </summary>
        public BouyomiChanClient()
        {
            var chs = ChannelServices.RegisteredChannels;
            foreach (var ch in chs)
            {
                System.Windows.Forms.MessageBox.Show(ch.ChannelName + "が既にあります");
                return;
            }
            ClientChannel = new IpcClientChannel();
            ChannelServices.RegisterChannel(ClientChannel, false);
            RemotingObject = (BouyomiChanRemoting)Activator.GetObject(typeof(BouyomiChanRemoting), "ipc://BouyomiChan/Remoting");
        }

        /// <summary>
        /// オブジェクト開放。
        /// </summary>
        public void Dispose() {
            if (ClientChannel != null) {
                ChannelServices.UnregisterChannel(ClientChannel);
                ClientChannel = null;
            }
        }

        /// <summary>
        /// 棒読みちゃんに音声合成タスクを追加します。
        /// </summary>
        /// <param name="sTalkText">喋らせたい文章</param>
        public void AddTalkTask(string sTalkText) {
            RemotingObject.AddTalkTask(sTalkText);
        }

        /// <summary>
        /// 棒読みちゃんに音声合成タスクを追加します。
        /// </summary>
        /// <param name="sTalkText">喋らせたい文章</param>
        /// <param name="iSpeed"   >再生速度。(-1で棒読みちゃん側の画面で選んでいる再生速度)</param>
        /// <param name="iVolume"  >音量。(-1で棒読みちゃん側の画面で選んでいる音量)</param>
        /// <param name="vType"    >声の種類。(Defaultで棒読みちゃん側の画面で選んでいる声)</param>
        public void AddTalkTask(string sTalkText, int iSpeed, int iVolume, VoiceType vType) {
            RemotingObject.AddTalkTask(sTalkText, iSpeed, iVolume, (int)vType);
        }
    }

    /// <summary>
    /// .NET Remotingのためのクラス。
    /// </summary>
    public class BouyomiChanRemoting : MarshalByRefObject {
        public void AddTalkTask(string sTalkText) { }
        public void AddTalkTask(string sTalkText, int iSpeed, int iVolume, int vType) { }
    }
}
