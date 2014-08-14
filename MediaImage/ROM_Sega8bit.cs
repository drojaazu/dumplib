using System;
using dumplib.Layout;
using System.IO;

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

        private static readonly string HW_Worldwide = "Sega Game Gear";
        private static readonly string HW_Japan = "セガ　ゲームギア";

        public string HardwareName_Worldwide
        {
            get
            {
                return SegaGameGear_ROM.HW_Worldwide;
            }
        }

        public string HardwareName_Japan
        {
            get
            {
                return SegaGameGear_ROM.HW_Japan;
            }
        }

        public string HardwareName_JapanRomaji
        {
            get
            {
                return SegaGameGear_ROM.HW_Worldwide;
            }
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

        public SegaGameGear_ROM(Stream Datastream, IDumpConverter Converter = null)
            : base(Datastream, Converter)
        {
            this.Init();
        }

        private void Init()
        {
            base.MediaType = MediaTypes.ROM;
            base.HardwareName = SegaGameGear_ROM.HW_Worldwide;
            base.SoftwareTitle = "[Sega GameGear software]";
            SetupHeader();
        }

        protected void SetupHeader()
        {
            // the Product Code is 2 bytes and one nibble (upper); the first two are bianry coded decimal,
            //this.SoftwareCode = int.Parse((base.Datastream.ReadByte(0x7ffe) >> 4).ToString() + (GetByte(0x7ffd).ToString("X") + GetByte(0x7ffc).ToString("X")));
            // set the region
            base.Datastream.Seek(0x7fff, System.IO.SeekOrigin.Begin);
            switch (base.Datastream.ReadByte() >> 4)
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
                    
                    //log("Warning: Invalid region in software header");
                    break;
            }
        }

        public override Layout.ImageMap AutoMap()
        {
            var _out = base.AutoMap();
            int banks = (int)(base.Datastream.Length / 16384);
            for (int j = 0; j < banks; j++)
                _out.Add(new ChunkInfo(new Range(j * 16384, 16384), ("ROM Bank " + j.ToString())));
            return _out;
        }
    }
}