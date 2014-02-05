using System;
using dumplib.Layout;

namespace dumplib.Image
{
    // --------------------------------------------------- SGG_ROM (Sega GameGear image)
    public class SegaGameGear_ROM : MediaImage
    {
        public enum SoftwareRegions : int
        {
            Unknown = 0,
            Japan = 5,
            Export = 6,
            International = 7
        }

        public SoftwareRegions SoftwareRegion
        {
            get;
            private set;
        }

        public int SoftwareCode
        {
            get;
            private set;
        }

        public SegaGameGear_ROM(string Filepath)
            : base(Filepath)
        {
            base.MediaType = MediaTypes.ROM;
            base.ReadWholeFile();
            //base.System = Systems.SGG;
            base.SoftwareTitle = "[Sega GameGear software]";
            SetupHeader();
        }

        protected void SetupHeader()
        {
            // the Product Code is 2 bytes and one nibble (upper); the first two are bianry coded decimal,
            this.SoftwareCode = int.Parse((GetByte(0x7ffe) >> 4).ToString() + (GetByte(0x7ffd).ToString("X") + GetByte(0x7ffc).ToString("X")));
            // set the region
            switch (GetByte(0x7fff) >> 4)
            {
                case 5:
                    this.SoftwareRegion = SoftwareRegions.Japan;
                    break;
                case 6:
                    this.SoftwareRegion = SoftwareRegions.Export;
                    break;
                case 7:
                    this.SoftwareRegion = SoftwareRegions.International;
                    break;
                default:
                    this.SoftwareRegion = SoftwareRegions.Unknown;
                    this.AddComment("Warning: Invalid region in software header");
                    break;
            }
        }

        public override Layout.ImageMap AutoMap()
        {
            var _out = base.AutoMap();
            uint banks = this.DataSize / 16384;
            for (uint j = 0; j < banks; j++)
                _out.Add(new Chunk(new Range(j * 16384, 16384), ("ROM Bank " + j.ToString())));
            return _out;
        }
    }
}