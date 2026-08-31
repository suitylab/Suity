using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Crypto.Client.Impl
{
    public class RsaPkcs8CryptoUtil : IRsaCryptoUtil
    {
        public RsaKey GenerateKeys()
        {
            using (var rsa = new RSACryptoServiceProvider(2048))
            {
                var key = new RsaKey
                {
                    Private = ExportPrivateKey(rsa),
                    Public = ExportPublicKey(rsa)
                };
                return key;
            }
        }

        public byte[] Sign(byte[] bytes, string privateKey)
        {
            var parameters = DecodePrivateKey(privateKey);
            using (var rsa = new RSACryptoServiceProvider(parameters.Modulus.Length * 8))
            {
                rsa.ImportParameters(parameters);
                return rsa.SignData(bytes, new MD5CryptoServiceProvider());
            }
        }

        public bool Verify(byte[] bytes, byte[] signature, string publicKey)
        {
            var parameters = DecodePublicKey(publicKey);
            using (var rsa = new RSACryptoServiceProvider(parameters.Modulus.Length * 8))
            {
                rsa.ImportParameters(parameters);
                return rsa.VerifyData(bytes, new MD5CryptoServiceProvider(), signature);
            }
        }

        public byte[] Encrypt(byte[] plainBytes, string publicKey)
        {
            var parameters = DecodePublicKey(publicKey);
            using (var rsa = new RSACryptoServiceProvider(parameters.Modulus.Length * 8))
            {
                rsa.ImportParameters(parameters);
                return rsa.Encrypt(plainBytes, false);
            }
        }

        public byte[] Decrypt(byte[] encryptedBytes, string privateKey)
        {
            var parameters = DecodePrivateKey(privateKey);
            using (var rsa = new RSACryptoServiceProvider(parameters.Modulus.Length * 8))
            {
                rsa.ImportParameters(parameters);
                return rsa.Decrypt(encryptedBytes, false);
            }
        }

        private static string ExportPrivateKey(RSACryptoServiceProvider rsa)
        {
            var p = rsa.ExportParameters(true);
            var ms = new MemoryStream();
            var w = new Asn1Writer(ms);
            w.WriteSEQUENCE(v =>
            {
                v.WriteINTEGER(0);
                v.WriteINTEGER(p.Modulus);
                v.WriteINTEGER(p.Exponent);
                v.WriteINTEGER(p.D);
                v.WriteINTEGER(p.P);
                v.WriteINTEGER(p.Q);
                v.WriteINTEGER(p.DP);
                v.WriteINTEGER(p.DQ);
                v.WriteINTEGER(p.InverseQ);
            });
            var rsaKeyDer = ms.ToArray();

            ms = new MemoryStream();
            w = new Asn1Writer(ms);
            w.WriteSEQUENCE(v =>
            {
                v.WriteINTEGER(0);
                v.WriteSEQUENCE(v2 =>
                {
                    v2.WriteOID("1.2.840.113549.1.1.1");
                    v2.WriteNULL();
                });
                v.WriteOCTET_STRING(rsaKeyDer);
            });
            return ToPem("PRIVATE KEY", ms.ToArray());
        }

        private static string ExportPublicKey(RSACryptoServiceProvider rsa)
        {
            var p = rsa.ExportParameters(false);
            var ms = new MemoryStream();
            var w = new Asn1Writer(ms);
            w.WriteSEQUENCE(v =>
            {
                v.WriteINTEGER(p.Modulus);
                v.WriteINTEGER(p.Exponent);
            });
            var rsaKeyDer = ms.ToArray();

            ms = new MemoryStream();
            w = new Asn1Writer(ms);
            w.WriteSEQUENCE(v =>
            {
                v.WriteSEQUENCE(v2 =>
                {
                    v2.WriteOID("1.2.840.113549.1.1.1");
                    v2.WriteNULL();
                });
                v.WriteBIT_STRING(rsaKeyDer);
            });
            return ToPem("PUBLIC KEY", ms.ToArray());
        }

        private static RSAParameters DecodePrivateKey(string pem)
        {
            var der = FromPem(pem, "PRIVATE KEY");
            var r = new Asn1Reader(der);
            r.ReadSEQUENCE();
            r.ReadINTEGER();
            r.SkipSEQUENCE();
            var octetString = r.ReadOCTET_STRING();
            var r2 = new Asn1Reader(octetString);
            r2.ReadSEQUENCE();
            r2.ReadINTEGER();
            var modulus = r2.ReadINTEGER();
            var exponent = r2.ReadINTEGER();
            var d = r2.ReadINTEGER();
            var p = r2.ReadINTEGER();
            var q = r2.ReadINTEGER();
            var dp = r2.ReadINTEGER();
            var dq = r2.ReadINTEGER();
            var inverseQ = r2.ReadINTEGER();
            return new RSAParameters
            {
                Modulus = modulus,
                Exponent = exponent,
                D = d,
                P = p,
                Q = q,
                DP = dp,
                DQ = dq,
                InverseQ = inverseQ
            };
        }

        private static RSAParameters DecodePublicKey(string pem)
        {
            var der = FromPem(pem, "PUBLIC KEY");
            var r = new Asn1Reader(der);
            r.ReadSEQUENCE();
            r.ReadSEQUENCE();
            r.ReadOID();
            r.ReadNULL();
            var rsaKeyDer = r.ReadBIT_STRING();
            var r2 = new Asn1Reader(rsaKeyDer);
            r2.ReadSEQUENCE();
            var modulus = r2.ReadINTEGER();
            var exponent = r2.ReadINTEGER();
            return new RSAParameters
            {
                Modulus = modulus,
                Exponent = exponent
            };
        }

        private static string ToPem(string label, byte[] der)
        {
            var b64 = Convert.ToBase64String(der);
            var sb = new StringBuilder();
            sb.Append("-----BEGIN ").Append(label).AppendLine("-----");
            for (int i = 0; i < b64.Length; i += 64)
                sb.AppendLine(b64.Substring(i, Math.Min(64, b64.Length - i)));
            sb.Append("-----END ").Append(label).AppendLine("-----");
            return sb.ToString();
        }

        private static byte[] FromPem(string pem, string label)
        {
            var begin = "-----BEGIN " + label + "-----";
            var end = "-----END " + label + "-----";
            int start = pem.IndexOf(begin, StringComparison.Ordinal) + begin.Length;
            int endPos = pem.IndexOf(end, StringComparison.Ordinal);
            var b64 = pem.Substring(start, endPos - start)
                .Replace("\r", "").Replace("\n", "").Replace(" ", "").Trim();
            return Convert.FromBase64String(b64);
        }

        private class Asn1Writer
        {
            private readonly MemoryStream _ms;

            public Asn1Writer(MemoryStream ms)
            {
                _ms = ms;
            }

            public void WriteSEQUENCE(Action<Asn1Writer> content)
            {
                var child = new Asn1Writer(new MemoryStream());
                content(child);
                var childBytes = child._ms.ToArray();
                WriteTagLen(0x30, childBytes.Length);
                _ms.Write(childBytes, 0, childBytes.Length);
            }

            public void WriteINTEGER(int value)
            {
                WriteINTEGER(new[] { (byte)value });
            }

            public void WriteINTEGER(byte[] value)
            {
                int start = 0;
                while (start < value.Length - 1 && value[start] == 0) start++;
                int len = value.Length - start;
                bool needPad = (value[start] & 0x80) != 0;
                int totalLen = len + (needPad ? 1 : 0);
                WriteTagLen(0x02, totalLen);
                if (needPad) _ms.WriteByte(0x00);
                _ms.Write(value, start, len);
            }

            public void WriteOCTET_STRING(byte[] value)
            {
                WriteTagLen(0x04, value.Length);
                _ms.Write(value, 0, value.Length);
            }

            public void WriteBIT_STRING(byte[] value)
            {
                WriteTagLen(0x03, value.Length + 1);
                _ms.WriteByte(0x00);
                _ms.Write(value, 0, value.Length);
            }

            public void WriteNULL()
            {
                _ms.WriteByte(0x05);
                _ms.WriteByte(0x00);
            }

            public void WriteOID(string oid)
            {
                var parts = oid.Split('.');
                var buf = new MemoryStream();
                buf.WriteByte((byte)(int.Parse(parts[0]) * 40 + int.Parse(parts[1])));
                for (int i = 2; i < parts.Length; i++)
                    WriteOIDComponents(buf, long.Parse(parts[i]));
                var body = buf.ToArray();
                WriteTagLen(0x06, body.Length);
                _ms.Write(body, 0, body.Length);
            }

            private void WriteOIDComponents(MemoryStream buf, long value)
            {
                if (value < 0x80)
                {
                    buf.WriteByte((byte)value);
                    return;
                }
                var stack = new byte[10];
                int idx = stack.Length - 1;
                stack[idx] = (byte)(value & 0x7F);
                value >>= 7;
                while (value > 0)
                {
                    stack[--idx] = (byte)((value & 0x7F) | 0x80);
                    value >>= 7;
                }
                buf.Write(stack, idx, stack.Length - idx);
            }

            private void WriteTagLen(byte tag, int length)
            {
                _ms.WriteByte(tag);
                if (length < 0x80)
                {
                    _ms.WriteByte((byte)length);
                }
                else if (length < 0x100)
                {
                    _ms.WriteByte(0x81);
                    _ms.WriteByte((byte)length);
                }
                else if (length < 0x10000)
                {
                    _ms.WriteByte(0x82);
                    _ms.WriteByte((byte)(length >> 8));
                    _ms.WriteByte((byte)(length & 0xFF));
                }
                else
                {
                    _ms.WriteByte(0x83);
                    _ms.WriteByte((byte)(length >> 16));
                    _ms.WriteByte((byte)((length >> 8) & 0xFF));
                    _ms.WriteByte((byte)(length & 0xFF));
                }
            }
        }

        private class Asn1Reader
        {
            private readonly byte[] _data;
            private int _pos;

            public Asn1Reader(byte[] data)
            {
                _data = data;
                _pos = 0;
            }

            public void ReadSEQUENCE()
            {
                ExpectTag(0x30);
                SkipLength();
            }

            public void SkipSEQUENCE()
            {
                ExpectTag(0x30);
                var len = ReadLength();
                _pos += len;
            }

            public byte[] ReadBIT_STRING()
            {
                ExpectTag(0x03);
                var len = ReadLength();
                _pos++;
                var val = new byte[len - 1];
                Array.Copy(_data, _pos, val, 0, val.Length);
                _pos += val.Length;
                return val;
            }

            public byte[] ReadOCTET_STRING()
            {
                ExpectTag(0x04);
                var len = ReadLength();
                var val = new byte[len];
                Array.Copy(_data, _pos, val, 0, len);
                _pos += len;
                return val;
            }

            public void ReadNULL()
            {
                ExpectTag(0x05);
                var len = _data[_pos++];
                if (len != 0) throw new InvalidOperationException("Invalid NULL length");
            }

            public void ReadOID()
            {
                ExpectTag(0x06);
                var len = ReadLength();
                _pos += len;
            }

            public byte[] ReadINTEGER()
            {
                ExpectTag(0x02);
                var len = ReadLength();
                var val = new byte[len];
                Array.Copy(_data, _pos, val, 0, len);
                _pos += len;

                int start = 0;
                while (start < val.Length - 1 && val[start] == 0) start++;
                if (start == 0) return val;
                var result = new byte[val.Length - start];
                Array.Copy(val, start, result, 0, result.Length);
                return result;
            }

            private void ExpectTag(byte expected)
            {
                var tag = _data[_pos++];
                if (tag != expected)
                    throw new InvalidOperationException($"Expected tag 0x{expected:X2}, got 0x{tag:X2}");
            }

            private int ReadLength()
            {
                var first = _data[_pos++];
                if (first < 0x80) return first;
                int numBytes = first & 0x7F;
                int length = 0;
                for (int i = 0; i < numBytes; i++)
                    length = (length << 8) | _data[_pos++];
                return length;
            }

            private void SkipLength()
            {
                ReadLength();
            }
        }
    }
}
