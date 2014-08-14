using System;
using System.IO;

namespace dumplib.Image
{
    /// <summary>
    /// Sega Super 32x
    /// </summary>
    public class SegaSuper32X_ROM : SegaMegadrive_ROM
    {
        private readonly static string HW_Worldwide = "Sega 32X";
        private readonly static string HW_Japan = "セガ　スーパー32X";
        private readonly static string HW_JapanRomaji = "Sega Super 32X";

        public SegaSuper32X_ROM(Stream Datastream, IDumpConverter Converter = null)
            : base(Datastream, Converter)
        {
            this.Init();
        }

        private void Init()
        {
            base.HardwareName = SegaSuper32X_ROM.HW_Worldwide;
        }

        protected void SetupHeader()
        {
            return;
        }

        new public string HardwareName_Worldwide
        {
            get
            {
                return SegaSuper32X_ROM.HW_Worldwide;
            }
        }

        new public string HardwareName_Japan
        {
            get
            {
                return SegaSuper32X_ROM.HW_Japan;
            }
        }

        new public string HardwareName_JapanRomaji
        {
            get
            {
                return SegaSuper32X_ROM.HW_JapanRomaji;
            }
        }
    }
}