using System;

namespace dumplib.Image
{
    /// <summary>
    /// Sega Super 32x
    /// </summary>
    public class SegaSuper32X_ROM : SegaMegadrive_ROM
    {
        public SegaSuper32X_ROM(string Filepath)
            : base(Filepath)
        {
            //base.System = Systems.S32X;
        }

        protected void SetupHeader()
        {
            return;
        }
    }
}