using Neo.Core;
using Neo.Implementations.Blockchains.LevelDB;
using Neo.IO;
using Neo.Network;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace StateOfNeo.Server.Infrastructure
{
    public static class BlockchainEngine
    {
        public static void StartBlockchain(LocalNode localNode)
        {
            Neo.Core.Blockchain.RegisterBlockchain(new LevelDBBlockchain(NeoSettings.Default.DataDirectoryPath));

            localNode = new LocalNode();
            Task.Run(() =>
            {
                const string acc_path = "chain.acc";
                const string acc_zip_path = acc_path + ".zip";
                if (File.Exists(acc_path))
                {
                    using (FileStream fs = new FileStream(acc_path, FileMode.Open, FileAccess.Read, FileShare.None))
                    {
                        ImportBlocks(fs);
                    }
                    File.Delete(acc_path);
                }
                else if (File.Exists(acc_zip_path))
                {
                    using (FileStream fs = new FileStream(acc_zip_path, FileMode.Open, FileAccess.Read, FileShare.None))
                    using (ZipArchive zip = new ZipArchive(fs, ZipArchiveMode.Read))
                    using (Stream zs = zip.GetEntry(acc_path).Open())
                    {
                        ImportBlocks(zs);
                    }
                    File.Delete(acc_zip_path);
                }

                localNode.Start(NeoSettings.Default.NodePort, NeoSettings.Default.WsPort);
            });
        }

        private static void ImportBlocks(Stream stream)
        {
            LevelDBBlockchain blockchain = (LevelDBBlockchain)Neo.Core.Blockchain.Default;
            blockchain.VerifyBlocks = false;
            using (BinaryReader r = new BinaryReader(stream))
            {
                uint count = r.ReadUInt32();
                for (int height = 0; height < count; height++)
                {
                    byte[] array = r.ReadBytes(r.ReadInt32());
                    if (height > Neo.Core.Blockchain.Default.Height)
                    {
                        Block block = array.AsSerializable<Block>();
                        Neo.Core.Blockchain.Default.AddBlock(block);
                    }
                }
            }
            blockchain.VerifyBlocks = true;
        }
    }
}
