namespace Jogl.Server.Data
{
    public enum WalletType { EVM, NEAR }

    public class Wallet
    {
        public WalletType Type { get; set; }

        public string Address { get; set; }
    }
}