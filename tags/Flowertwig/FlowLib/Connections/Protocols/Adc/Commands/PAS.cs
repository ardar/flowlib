using Flowertwig.Utils.Connections.Interfaces;
using Flowertwig.Utils.Hashing;

namespace FlowLib.Connections.Protocols.Adc.Commands
{
    public class PAS : AdcBaseMessage
    {
        protected string password = null;
        protected string random = null;
        protected string encrypted = null;

        public string Password
        {
            get { return password; }
            set { 
                password = value;
                EncryptPassword();
                CreateRaw();
            }
        }
        public PAS(IConnection con, string raw)
            : base(con, raw)
        {
        }

        public PAS(IConnection con, string randomdata, string password)
            : base(con, null)
        {
            this.random = randomdata;
            Password = password;
        }

        private void CreateRaw()
        {
            Raw = "HPAS " + encrypted + "\n";
        }

        private void EncryptPassword()
        {
            try
            {
                var tiger = new Tiger();
                byte[] data = System.Text.Encoding.UTF8.GetBytes(password);
                var ms = new System.IO.MemoryStream();
                ms.Write(data, 0, data.Length);
                data = Base32.Decode(random);
                ms.Write(data, 0, data.Length);

                data = tiger.ComputeHash(ms.ToArray());
                encrypted = Base32.Encode(data);
            }
            catch { }
        }
    }
}