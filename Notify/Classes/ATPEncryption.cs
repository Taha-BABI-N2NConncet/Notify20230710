using System.Text;
using System.Diagnostics;
using System.Security.Cryptography;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Paddings;
using Org.BouncyCastle.Crypto.Parameters;


namespace Encryption
{

    public static class AES
    {

        /// <summary>
        /// 
        /// AES Encrypt to Base64 String
        /// 
        /// </summary>
        /// <param name="Cmd"></param>
        /// <param name="Key"></param>
        /// <returns></returns>

        private static byte[] iv = new byte[16] { 1, 2, 3, 4, 5, 6, 7, 8, 31, 32, 33, 34, 35, 36, 37, 38 };
        private static Encoding m_Encoding = Encoding.GetEncoding(1252);
        private const string m_KeyPhrase = "C56FC07E-2A5C-4618-9933-1A2ED3D2CD62";

        public static byte[] GetSHA256(string text)
        {
            byte[] message = Encoding.ASCII.GetBytes(text);

            return SHA256.HashData(message);
        }

        public static string GetSHA256InHex(string text)
        {
            byte[] message = Encoding.ASCII.GetBytes(text);

            string hex = string.Empty;

            byte[] hashValue = SHA256.HashData(message);
            foreach (byte x in hashValue)
            {
                hex += String.Format("{0:x2}", x);
            }

            return hex;
        }

        public static string ATPEncrypt(string text)
        {
            byte[] lkey = GetSHA256(m_KeyPhrase);
            SymmetricAlgorithm algorithm = Aes.Create();
            algorithm.Mode = CipherMode.ECB;
            algorithm.Padding = PaddingMode.PKCS7;

            ICryptoTransform transform = algorithm.CreateEncryptor(lkey, iv);
            byte[] inputbuffer = m_Encoding.GetBytes(text);
            byte[] outputBuffer = transform.TransformFinalBlock(inputbuffer, 0, inputbuffer.Length);
            return Convert.ToBase64String(outputBuffer);
        }

        public static string ATPDecrypt(string text)
        {
            byte[] lkey = GetSHA256(m_KeyPhrase);
            SymmetricAlgorithm algorithm = Aes.Create();
            algorithm.Mode = CipherMode.ECB;
            algorithm.Padding = PaddingMode.PKCS7;

            ICryptoTransform transform = algorithm.CreateDecryptor(lkey, iv);
            byte[] inputbuffer = Convert.FromBase64String(text);
            byte[] outputBuffer = transform.TransformFinalBlock(inputbuffer, 0, inputbuffer.Length);
            return m_Encoding.GetString(outputBuffer);
        }

        /// <summary>
        /// 
        /// AES Encrypt.
        /// 
        /// </summary>
        /// <param name="ToEncrypt"></param>
        /// <param name="Key"></param>
        /// <returns></returns>

        public static byte[] Encrypt (byte[] ToEncrypt, byte[] Key)
        {
            Debug.Assert( ToEncrypt != null && Key != null );

            try
            {
                var Cipher = new PaddedBufferedBlockCipher( new AesEngine(), new Pkcs7Padding() );

                Cipher.Init( true, new KeyParameter( Key ) );

                return Cipher.DoFinal( ToEncrypt );
            }
            catch ( CryptoException ex )
            {
                throw new Exception( "Failed to AES Encrypt!", ex );
            }
        }


        /// <summary>
        /// AES Decrypt.
        /// </summary>
        /// <param name="ToDecrypt"></param>
        /// <param name="Key"></param>
        /// <returns></returns>

        public static byte[] Decrypt (byte[] ToDecrypt, byte[] Key)
        {
            Debug.Assert( ToDecrypt != null && Key != null );

            try
            {
                var Cipher = new PaddedBufferedBlockCipher( new AesEngine(), new Pkcs7Padding() );

                Cipher.Init( false, new KeyParameter( Key ) );

                return Cipher.DoFinal( ToDecrypt );
            }
            catch ( CryptoException ex )
            {
                throw new Exception( "Failed to AES Decrypt!", ex );
            }
        }
    }
}
