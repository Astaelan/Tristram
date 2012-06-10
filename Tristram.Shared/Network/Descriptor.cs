using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Tristram.Shared;

namespace Tristram.Shared.Network
{
    public abstract class Descriptor
    {
        private Socket mSocket = null;
        private string mHost = null;
        private int mDisconnecting = 0;

        private byte[] mReceiveBuffer = null;
        private int mReceiveStart = 0;
        private int mReceiveLength = 0;
        private ConcurrentQueue<ByteArraySegment> mSendSegments = new ConcurrentQueue<ByteArraySegment>();
        private int mSending = 0;

        protected Descriptor(Socket pSocket, int pMaxReceiveBuffer)
        {
            mSocket = pSocket;
            mHost = ((IPEndPoint)mSocket.RemoteEndPoint).Address.ToString();
            mReceiveBuffer = new byte[pMaxReceiveBuffer];
        }

        public string Host { get { return mHost; } }

        protected abstract void OnConnected();
        public void Connected()
        {
            OnConnected();
            BeginReceive();
        }

        protected abstract void OnDisconnect();
        public void Disconnect()
        {
            if (Interlocked.CompareExchange(ref mDisconnecting, 1, 0) == 0)
            {
                OnDisconnect();
            }
        }

        protected abstract void OnDisconnected();
        public void Disconnected()
        {
            mSocket.Close();
            OnDisconnected();
        }

        private void BeginReceive()
        {
            if (mDisconnecting != 0) return;
            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.Completed += (s, a) => EndReceive(a);
            args.SetBuffer(mReceiveBuffer, mReceiveStart + mReceiveLength, mReceiveBuffer.Length - (mReceiveStart + mReceiveLength));
            try { if (!mSocket.ReceiveAsync(args)) EndReceive(args); }
            catch (ObjectDisposedException) { }
        }
        protected abstract int OnDataReceived(byte[] pBuffer, int pStart, int pLength);
        private void EndReceive(SocketAsyncEventArgs pArguments)
        {
            if (mDisconnecting != 0) return;
            if (pArguments.BytesTransferred <= 0)
            {
                Disconnect();
                return;
            }
            mReceiveLength += pArguments.BytesTransferred;

            int used = OnDataReceived(mReceiveBuffer, mReceiveStart, mReceiveLength);
            if (used > 0)
            {
                mReceiveStart += used;
                mReceiveLength -= used;
            }

            if (mReceiveLength == 0) mReceiveStart = 0;
            else if (mReceiveStart > 0)
            {
                Buffer.BlockCopy(mReceiveBuffer, mReceiveStart, mReceiveBuffer, 0, mReceiveLength);
                mReceiveStart = 0;
            }
            if (mReceiveLength == mReceiveBuffer.Length) Disconnect();
            else BeginReceive();
        }

        protected void Send(byte[] pBuffer, int pStart = 0, int pLength = 0)
        {
            if (pLength == 0) pLength = pBuffer.Length;
            if (mDisconnecting != 0) return;
            mSendSegments.Enqueue(new ByteArraySegment(pBuffer, pStart, pLength));
            if (Interlocked.CompareExchange(ref mSending, 1, 0) == 0) BeginSend();
        }
        private void BeginSend()
        {
            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.Completed += (s, a) => EndSend(a);
            ByteArraySegment segment = null;
            if (mSendSegments.TryPeek(out segment))
            {
                args.SetBuffer(segment.Buffer, segment.Start, segment.Length);
                try { if (!mSocket.SendAsync(args)) EndSend(args); }
                catch (ObjectDisposedException) { }
            }
        }
        private void EndSend(SocketAsyncEventArgs pArguments)
        {
            if (mDisconnecting != 0) return;
            if (pArguments.BytesTransferred <= 0)
            {
                Disconnect();
                return;
            }
            ByteArraySegment segment = null;
            if (mSendSegments.TryPeek(out segment))
            {
                if (segment.Advance(pArguments.BytesTransferred)) mSendSegments.TryDequeue(out segment);
                if (mSendSegments.TryPeek(out segment)) BeginSend();
                else mSending = 0;
            }
            else mSending = 0;
        }
    }
}
