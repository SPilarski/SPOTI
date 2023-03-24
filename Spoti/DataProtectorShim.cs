using Microsoft.AspNetCore.DataProtection;

namespace Spoti
{
    public class DataProtectorShim : IDataProtector
    {
        private readonly IDataProtector _protector;

        public DataProtectorShim(IDataProtector protector)
        {
            _protector = protector;
        }

        public IDataProtector CreateProtector(string purpose)
        {
            return new DataProtectorShim(_protector.CreateProtector(purpose));
        }

        public byte[] Protect(byte[] plaintext)
        {
            return _protector.Protect(plaintext);
        }

        public byte[] Unprotect(byte[] protectedData)
        {
            return _protector.Unprotect(protectedData);
        }
    }

}
