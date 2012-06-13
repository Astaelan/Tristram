using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace Tristram.Shared.Network
{
    public sealed class SRP
    {
        private static readonly SHA256Managed sHashAlgorithm = new SHA256Managed();

        private static readonly BigInteger sg = new BigInteger(2);
        private static readonly BigInteger sN = new BigInteger(new byte[]
        {
            0xAB, 0x24, 0x43, 0x63, 0xA9, 0xC2, 0xA6, 0xC3, 0x3B, 0x37, 0xE4, 0x61, 0x84, 0x25, 0x9F, 0x8B,
            0x3F, 0xCB, 0x8A, 0x85, 0x27, 0xFC, 0x3D, 0x87, 0xBE, 0xA0, 0x54, 0xD2, 0x38, 0x5D, 0x12, 0xB7,
            0x61, 0x44, 0x2E, 0x83, 0xFA, 0xC2, 0x21, 0xD9, 0x10, 0x9F, 0xC1, 0x9F, 0xEA, 0x50, 0xE3, 0x09,
            0xA6, 0xE5, 0x5E, 0x23, 0xA7, 0x77, 0xEB, 0x00, 0xC7, 0xBA, 0xBF, 0xF8, 0x55, 0x8A, 0x0E, 0x80,
            0x2B, 0x14, 0x1A, 0xA2, 0xD4, 0x43, 0xA9, 0xD4, 0xAF, 0xAD, 0xB5, 0xE1, 0xF5, 0xAC, 0xA6, 0x13,
            0x1C, 0x69, 0x78, 0x64, 0x0B, 0x7B, 0xAF, 0x9C, 0xC5, 0x50, 0x31, 0x8A, 0x23, 0x08, 0x01, 0xA1,
            0xF5, 0xFE, 0x31, 0x32, 0x7F, 0xE2, 0x05, 0x82, 0xD6, 0x0B, 0xED, 0x4D, 0x55, 0x32, 0x41, 0x94,
            0x29, 0x6F, 0x55, 0x7D, 0xE3, 0x0F, 0x77, 0x19, 0xE5, 0x6C, 0x30, 0xEB, 0xDE, 0xF6, 0xA7, 0x86, 0x00
        });

        public static byte[] RandomBytes(int pCount)
        {
            byte[] bytes = new byte[pCount];
            Random random = new Random();
            random.NextBytes(bytes);
            return bytes;
        }
        public static byte[] CalculateVerifier(string pEmail, string pPassword, byte[] pSalt)
        {
            string emailHash = BitConverter.ToString(sHashAlgorithm.ComputeHash(Encoding.ASCII.GetBytes(pEmail))).Replace("-", "").ToUpper();
            byte[] p = sHashAlgorithm.ComputeHash(Encoding.ASCII.GetBytes(emailHash + ":" + pPassword.ToUpper()));
            byte[] t = sHashAlgorithm.ComputeHash(pSalt.Concat(p).ToArray());
            Array.Resize(ref t, t.Length + 1);
            BigInteger x = new BigInteger(t);
            byte[] v = BigInteger.ModPow(sg, x, sN).ToByteArray();
            Array.Resize(ref v, 128);
            return v;
        }

        private string mEmail = null;
        private byte[] mSalt = null;
        private byte[] mVerifier = null;
        private BigInteger mb = BigInteger.Zero;
        private BigInteger mB = BigInteger.Zero;

        public SRP(string pEmail, byte[] pSalt, byte[] pVerifier)
        {
            mEmail = pEmail;
            mSalt = pSalt;
            mVerifier = pVerifier;
            string emailHash = BitConverter.ToString(sHashAlgorithm.ComputeHash(Encoding.ASCII.GetBytes(pEmail))).Replace("-", "").ToUpper();
            byte[] b = RandomBytes(128);
            Array.Resize(ref b, b.Length + 1);
            mb = new BigInteger(b);
            BigInteger gb = BigInteger.ModPow(sg, mb, sN);
            byte[] kHash = sHashAlgorithm.ComputeHash(sN.ToByteArray().Concat(sg.ToByteArray()).ToArray());
            Array.Resize(ref kHash, kHash.Length + 1);
            BigInteger k = new BigInteger(kHash);
            byte[] t = new byte[pVerifier.Length + 1];
            Array.Copy(pVerifier, t, pVerifier.Length);
            BigInteger v = new BigInteger(t);
            mB = BigInteger.Remainder((v * k) + gb, sN);

        }
    }
}
