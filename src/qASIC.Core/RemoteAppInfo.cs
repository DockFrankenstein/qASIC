using qASIC.Communication;

namespace qASIC
{
    public class RemoteAppInfo : NetworkServerInfo
    {
        public string projectName = string.Empty;
        public string version = string.Empty;
        public string engine = string.Empty;
        public string engineVersion = string.Empty;

        public List<SystemInfo> systems = new List<SystemInfo>();

        public void RegisterSystem(string systemInfo, string version)
        {
            if (!UsesSystem(systemInfo))
                systems.Add(new SystemInfo(systemInfo, version));
        }

        public bool UsesSystem(string systemName) =>
            systems.Any(x => x.name == systemName);

        public override Packet Write(Packet packet)
        {
            base.Write(packet)
                .Write(projectName)
                .Write(version)
                .Write(engine)
                .Write(engineVersion)
                .Write(systems.Count);

            foreach (var item in systems)
                packet.Write(item.name)
                    .Write(item.version);

            return packet;
        }

        public override void Read(Packet packet)
        {
            base.Read(packet);
            projectName = packet.ReadString();
            version = packet.ReadString();
            engine = packet.ReadString();
            engineVersion = packet.ReadString();

            systems.Clear();
            for (int i = 0; i < packet.ReadInt(); i++)
            {
                systems.Add(new SystemInfo()
                {
                    name = packet.ReadString(),
                    version = packet.ReadString(),
                });
            }
        }

        public struct SystemInfo
        {
            public SystemInfo(string name, string version)
            {
                this.name = name;
                this.version = version;
            }

            public string name;
            public string version;
        }
    }
}
