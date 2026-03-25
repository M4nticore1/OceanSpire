// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("D7Dv2/+22nVtBLxVUJg7ZSuMe1lEWBg6vmLF93dfUIn53MwPX2WfjU3/7atLOc4uPfBZVyoPDxH0/vbDKjWYYaWg5X6t/iR/oOLITLb9D6JT30UoCqtZEQnNJ1hCLHc6Yyt9sYRMFAYEgz5bWOZtlf/1zincW3nDszA+MQGzMDszszAwMZf3J2PaQmFEtXrkzVkqMQZzuVMYMjzJ7VtzNgGzMBMBPDc4G7d5t8Y8MDAwNDEyVdPGLZ6l1WM9e/YyscusPQ37qSIhSyx9UVmNo+YLivXtvxSjy+kZyHorwjt2Cp37IWQKYGyk1dLSKuJHpqtMYiOQPBv/1YlrxfIGDPseLuIum457JguX1fx0YTxHcTVFUBG7zKfB2RAq9DHvbjMyMDEw");
        private static int[] order = new int[] { 12,5,4,9,10,6,6,8,12,13,10,12,12,13,14 };
        private static int key = 49;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
