using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Psycho.Networking
{
    public sealed class PsychoProtocolClient : MonoBehaviour
    {
        private const string RsaModulus = "92714122021553179775366524500108363230742886412562460353371834211639950528415299652617724152900849886190308772072042859622861358879128131094768196236228636841986869596552685912224862654196445056305117750007439275285163469435632717305809526968825980718429695794961208950600403227465945624833677031718369278551";
        private const string RsaExponent = "65537";

        [Header("Server")]
        [SerializeField] private string host = "127.0.0.1";
        [SerializeField] private int port = 13377;
        [SerializeField] private int clientVersion = 55;

        [Header("Manual smoke test")]
        [SerializeField] private bool loginOnStart;
        [SerializeField] private string username = "";
        [SerializeField] private string password = "";

        public event Action<int> LoginResponseReceived;

        private TcpClient tcpClient;
        private NetworkStream stream;
        private IsaacCipher outboundCipher;
        private IsaacCipher inboundCipher;

        private async void Start()
        {
            if (!loginOnStart)
            {
                return;
            }

            int response = await LoginAsync(username, password, false, CancellationToken.None);
            Debug.Log($"Psycho Unity login response: {response}");
        }

        private void OnDestroy()
        {
            Disconnect();
        }

        public async Task<int> LoginAsync(string accountName, string accountPassword, bool reconnecting, CancellationToken cancellationToken)
        {
            Disconnect();

            tcpClient = new TcpClient();
            await tcpClient.ConnectAsync(host, port);
            stream = tcpClient.GetStream();

            await WriteAsync(new byte[] { 14 }, 1, cancellationToken);
            int response = await ReadByteAsync(cancellationToken);

            if (response == 0)
            {
                byte[] seedBytes = await ReadExactAsync(8, cancellationToken);
                long serverSeed = RsBuffer.ReadLongBigEndian(seedBytes, 0);
                int[] seed = CreateSeed(serverSeed);

                RsBuffer payload = BuildRsaPayload(accountName, accountPassword, seed);
                RsBuffer login = BuildLoginPacket(payload, reconnecting);

                outboundCipher = new IsaacCipher(seed);
                int[] inboundSeed = new int[seed.Length];
                Array.Copy(seed, inboundSeed, seed.Length);
                for (int i = 0; i < inboundSeed.Length; i++)
                {
                    inboundSeed[i] += 50;
                }

                inboundCipher = new IsaacCipher(inboundSeed);
                await WriteAsync(login.Data, login.Position, cancellationToken);
                response = await ReadByteAsync(cancellationToken);
            }

            LoginResponseReceived?.Invoke(response);
            return response;
        }

        public void Disconnect()
        {
            stream?.Dispose();
            tcpClient?.Close();
            stream = null;
            tcpClient = null;
            outboundCipher = null;
            inboundCipher = null;
        }

        private RsBuffer BuildRsaPayload(string accountName, string accountPassword, int[] seed)
        {
            RsBuffer buffer = new RsBuffer();
            buffer.PutByte(10);
            buffer.PutInt(seed[0]);
            buffer.PutInt(seed[1]);
            buffer.PutInt(seed[2]);
            buffer.PutInt(seed[3]);
            buffer.PutInt(23);
            buffer.PutString(NormalizeName(accountName));
            buffer.PutString(accountPassword);
            buffer.PutString(SystemInfo.deviceUniqueIdentifier);
            buffer.PutString(SystemInfo.deviceName);
            buffer.EncryptRsaContent(RsaModulus, RsaExponent);
            return buffer;
        }

        private RsBuffer BuildLoginPacket(RsBuffer payload, bool reconnecting)
        {
            RsBuffer login = new RsBuffer();
            login.PutByte(reconnecting ? 18 : 16);
            login.PutByte(payload.Position + 1 + 1 + 2);
            login.PutByte(255);
            login.PutUnsignedShort(clientVersion);
            login.PutByte(QualitySettings.GetQualityLevel() <= 1 ? 1 : 0);
            login.PutBytes(payload.Data, payload.Position);
            return login;
        }

        private static int[] CreateSeed(long serverSeed)
        {
            System.Random random = new System.Random();
            return new[]
            {
                random.Next(0, 99999999),
                random.Next(0, 99999999),
                (int)(serverSeed >> 32),
                (int)serverSeed
            };
        }

        private static string NormalizeName(string value)
        {
            return (value ?? string.Empty).Trim().Replace('_', ' ');
        }

        private async Task WriteAsync(byte[] bytes, int length, CancellationToken cancellationToken)
        {
            await stream.WriteAsync(bytes, 0, length, cancellationToken);
            await stream.FlushAsync(cancellationToken);
        }

        private async Task<int> ReadByteAsync(CancellationToken cancellationToken)
        {
            byte[] bytes = await ReadExactAsync(1, cancellationToken);
            return bytes[0] & 0xff;
        }

        private async Task<byte[]> ReadExactAsync(int length, CancellationToken cancellationToken)
        {
            byte[] bytes = new byte[length];
            int offset = 0;
            while (offset < length)
            {
                int read = await stream.ReadAsync(bytes, offset, length - offset, cancellationToken);
                if (read <= 0)
                {
                    throw new InvalidOperationException("Server closed the socket.");
                }

                offset += read;
            }

            return bytes;
        }
    }
}
