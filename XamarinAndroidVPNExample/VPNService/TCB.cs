using Java.IO;
using Java.Lang;
using Java.Nio.Channels;
using System.Collections.Generic;

namespace XamarinAndroidVPNExample.VPNService
{
    public class TCB : Object
    {
        public String ipAndPort;

        public long mySequenceNum, theirSequenceNum;
        public long myAcknowledgementNum, theirAcknowledgementNum;
        public TCBStatus status;

        // TCP has more states, but we need only these
        public enum TCBStatus
        {
            SYN_SENT,
            SYN_RECEIVED,
            ESTABLISHED,
            CLOSE_WAIT,
            LAST_ACK,
        }

        public Packet referencePacket;

        public SocketChannel channel;
        public bool waitingForNetworkData;
        public SelectionKey selectionKey;

        private const int MAX_CACHE_SIZE = 50; // XXX: Is this ideal?

        private static LRUCache<string, TCB> tcbCache = new LRUCache<string, TCB>(MAX_CACHE_SIZE);

        public static TCB GetTCB(String ipAndPort)
        {
            lock (tcbCache)
            {
                return (TCB)tcbCache.Get(ipAndPort);
            }
        }

        public static void PutTCB(String ipAndPort, TCB tcb)
        {
            lock (tcbCache)
            {
                tcbCache.Put(ipAndPort, tcb);
            }
        }

        public TCB(String ipAndPort, long mySequenceNum, long theirSequenceNum, long myAcknowledgementNum, long theirAcknowledgementNum,
                   SocketChannel channel, Packet referencePacket)
        {
            this.ipAndPort = ipAndPort;

            this.mySequenceNum = mySequenceNum;
            this.theirSequenceNum = theirSequenceNum;
            this.myAcknowledgementNum = myAcknowledgementNum;
            this.theirAcknowledgementNum = theirAcknowledgementNum;

            this.channel = channel;
            this.referencePacket = referencePacket;
        }

        public static void CloseTCB(TCB tcb)
        {
            tcb.CloseChannel();
            lock (tcbCache)
            {
                tcbCache.Remove(tcb.ipAndPort);
            }
        }

        public static void CloseAll()
        {
            lock (tcbCache)
            {
                var jdictionaryFromHashMap = new Android.Runtime.JavaDictionary<string, TCB>(tcbCache.Handle, Android.Runtime.JniHandleOwnership.DoNotRegister);

                foreach (KeyValuePair<string, TCB> item in jdictionaryFromHashMap)
                {
                    item.Value.CloseChannel();
                    tcbCache.Remove(item.Key, item.Value);
                }
            }
        }

        private void CloseChannel()
        {
            try
            {
                channel.Close();
            }
            catch (IOException e)
            {
                // Ignore
            }
        }
    }
}
