using System;
using System.Security.Cryptography;
using System.Text;

namespace PogoDom.Verification
{
    public sealed class MatchTicketSigner
    {
        private readonly byte[] _key;

        public MatchTicketSigner(byte[] secretKey)
        {
            if (secretKey == null || secretKey.Length < 32) throw new ArgumentException("Ticket signing key must contain at least 32 bytes.", nameof(secretKey));
            _key = (byte[])secretKey.Clone();
        }

        public SignedMatchTicket Sign(MatchTicket ticket)
        {
            if (ticket == null) throw new ArgumentNullException(nameof(ticket));
            return new SignedMatchTicket(ticket, Compute(ticket));
        }

        public bool Verify(SignedMatchTicket signed)
        {
            if (signed == null) return false;
            var expected = Compute(signed.Ticket);
            byte[] left;
            byte[] right;
            try
            {
                left = HexToBytes(expected);
                right = HexToBytes(signed.Signature);
            }
            catch
            {
                return false;
            }
            return left.Length == right.Length && CryptographicOperations.FixedTimeEquals(left, right);
        }

        private string Compute(MatchTicket ticket)
        {
            using (var hmac = new HMACSHA256(_key))
            {
                var bytes = Encoding.UTF8.GetBytes(ticket.CanonicalPayload());
                var hash = hmac.ComputeHash(bytes);
                var builder = new StringBuilder(hash.Length * 2);
                for (var i = 0; i < hash.Length; i++) builder.Append(hash[i].ToString("x2"));
                return builder.ToString();
            }
        }

        private static byte[] HexToBytes(string value)
        {
            if (value == null || value.Length % 2 != 0) throw new FormatException();
            var bytes = new byte[value.Length / 2];
            for (var i = 0; i < bytes.Length; i++) bytes[i] = Convert.ToByte(value.Substring(i * 2, 2), 16);
            return bytes;
        }
    }
}
