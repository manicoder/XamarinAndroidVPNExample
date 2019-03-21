using Java.IO;
using Java.Nio.Channels;

namespace XamarinAndroidVPNExample.VPNService
{
    public class TCB : Java.Lang.Object
    {
        public Java.Lang.String ipAndPort;

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

        //        new LRUCache<>(MAX_CACHE_SIZE, new LRUCache.CleanupCallback<String, TCB>()
        //        {
        //        @Override
        //            public void cleanup(Map.Entry<String, TCB> eldest)
        //{
        //    eldest.getValue().closeChannel();
        //}


        public static TCB GetTCB(Java.Lang.String ipAndPort)
        {
            return (TCB)tcbCache.Get(ipAndPort);
        }

        public static void PutTCB(Java.Lang.String ipAndPort, TCB tcb)
        {
            tcbCache.Put(ipAndPort, tcb);
        }

        public TCB(Java.Lang.String ipAndPort, long mySequenceNum, long theirSequenceNum, long myAcknowledgementNum, long theirAcknowledgementNum,
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
            tcbCache.Remove(tcb.ipAndPort);
        }

        public static void CloseAll()
        {
            var enumerator = tcbCache.EntrySet().GetEnumerator();

            while (enumerator.MoveNext())
            {
                var o = (TCB)enumerator.Current;
                o.CloseChannel();
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
