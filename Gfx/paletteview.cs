using System;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;

namespace dumplib.Gfx
{
    public class PaletteViewer : UserControl
    {
        /// <summary>
        /// Fires when the user changes the subpalette selection
        /// </summary>
        public event EventHandler SubpaletteChanged;

        /// <summary>
        /// Fires when the user changes the selected color
        /// </summary>
        public event EventHandler SelectedColorChanged;

        //private static dumplib.Gfx.GetPalette GetPalette = new Gfx.GetPalette();
        
        private ColorPalette fullpalette;
        /// <summary>
        /// 8-bit color palette to display
        /// </summary>
        public ColorPalette FullPalette
        {
            get
            {
                return this.fullpalette;
            }
            set
            {
                this.fullpalette = value;
                this.DrawGrid();
                this.Line = 0;
            }
        }

        private ColorPalette subpalette;
        /// <summary>
        /// The palette to apply to the image
        /// </summary>
        public ColorPalette SubPalette
        {
            get
            {
                // if the palette is 8 bit (256 entries) return the full palette
                if (this.linewidth == 256)
                    return this.fullpalette;
                // else return the subpalette
                return this.subpalette;
            }
            
        }

        /// <summary>
        /// The color of the border around the selected subpalette
        /// </summary>
        public Color SubpaletteSelectBorder
        {
            get;
            set;
        }

        /// <summary>
        /// The color of the border around the selected color
        /// </summary>
        public Color ColorSelectBorder
        {
            get;
            set;
        }
        
        private int selectedcolor;
        
        // have this return a color instead of an int
        /// <summary>
        /// 
        /// </summary>
        public int SelectedColor
        {
            get { return this.selectedcolor; }
            set
            {
                this.selectedcolor = value;
                this.DrawColorSelection();
                this.Refresh();
                if (this.SelectedColorChanged != null) this.SelectedColorChanged(new object(), new EventArgs());
            }
        }

        private int linewidth;
        /// <summary>
        /// Size of the sub-palette
        /// </summary>
        public int SubpaletteSize
        {
            get
            {
                return this.linewidth;
            }
            set
            {
                //if (value != 2 || value != 4 || value != 8 || value != 16) throw new ArgumentOutOfRangeException(value.ToString() + " is not a valid palette line length");
                //MessageBox.Show("called! value = " + value.ToString());
                this.linewidth = value;
                
                if (this.linewidth != 256)
                    this.subpalette = this.linewidth == 2 ? dumplib.Gfx.CreatePalette.New_1bit() : dumplib.Gfx.CreatePalette.New_4bit(true);
                this.Line = 0;
            }
        }

        private int tilesize;
        
        private int line;
        /// <summary>
        /// Palette line to use for the sub-palette
        /// </summary>
        public int Line
        {
            get
            {
                return line;
            }
            set
            {
                this.line = value;
                if (this.linewidth != 256)
                {
                    for (int t = 0; t < this.linewidth; t++) this.subpalette.Entries[t] = this.fullpalette.Entries[(this.line * this.linewidth) + t];
                    DrawSelection();
                }
                if (this.SubpaletteChanged != null) this.SubpaletteChanged(new object(), new EventArgs());
                this.Refresh();
            }
        }

        private Bitmap colorgrid, selectbox, selectedcolorbox;
        private Rectangle selectrect;
        private Rectangle selectcolorrect;
        private int linestart;

        public PaletteViewer()
        {
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.tilesize = this.Height > this.Width ? this.Width / 16 : this.Height / 16;
            this.line = 0;
            this.linewidth = 16;
            this.selectedcolor = 0;
            this.selectrect = new Rectangle(0, 0, this.tilesize * this.linewidth, this.tilesize);
            this.fullpalette = dumplib.Gfx.CreatePalette.New_8bit();
            this.subpalette = dumplib.Gfx.CreatePalette.New_4bit(true);
        }

        protected override void OnLoad(EventArgs e)
        {
            //base.OnLoad(e);
            
            this.selectrect = new Rectangle(0, 0, this.tilesize * this.linewidth, this.tilesize);
            this.subpalette = this.linewidth == 2 ? dumplib.Gfx.CreatePalette.New_1bit() : dumplib.Gfx.CreatePalette.New_4bit();
            this.DrawGrid();
            this.DrawSelection();
            this.DrawColorSelection();
            this.Refresh();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            int clickedtile = ((e.Y / this.tilesize) * 16) + (e.X / this.tilesize);
            switch (e.Button)
            {
                case System.Windows.Forms.MouseButtons.Left:
                    if (this.linewidth != 256)
                    {
                        this.linestart = clickedtile == 0 ? 0 : clickedtile - (clickedtile % this.linewidth);
                        this.Line = linestart / linewidth;
                    }
                    break;
                case System.Windows.Forms.MouseButtons.Right:
                    this.SelectedColor = clickedtile;
                    break;
            }
        }

        protected override void OnResize(EventArgs e)
        {
            this.Height -= this.Height % 16;
            this.Width -= this.Width % 16;
            this.tilesize = this.Height > this.Width ? this.Width / 16 : this.Height / 16;

            this.DrawGrid();
            this.DrawSelection();
            this.DrawColorSelection();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.DrawImage(this.colorgrid, 0, 0);
            if(this.linewidth != 256) e.Graphics.DrawImage(this.selectbox, this.selectrect);
            e.Graphics.DrawImage(this.selectedcolorbox, this.selectcolorrect);
        }

        private void DrawSelection()
        {
            if (this.line == 0) this.selectrect = new Rectangle(0, 0, this.tilesize * this.linewidth, this.tilesize);
            else this.selectrect = new Rectangle((this.linestart % 16) * this.tilesize, (this.linestart / 16) * tilesize, this.tilesize * this.linewidth, this.tilesize);

            this.selectbox = new Bitmap(this.selectrect.Width, this.selectrect.Height);
            Graphics drawbox = Graphics.FromImage(this.selectbox);

            Pen outline = new Pen(this.SubpaletteSelectBorder, 2);
            drawbox.DrawRectangle(outline, 0, 0, this.selectrect.Width, this.selectrect.Height);
            outline.Dispose();
            drawbox.Dispose();
        }

        private void DrawColorSelection()
        {
            if (this.selectedcolor == 0) this.selectcolorrect = new Rectangle(0, 0, this.tilesize, this.tilesize);
            else this.selectcolorrect = new Rectangle((this.selectedcolor % 16) * this.tilesize, (this.selectedcolor / 16) * tilesize, this.tilesize, this.tilesize);

            this.selectedcolorbox = new Bitmap(this.selectcolorrect.Width, this.selectcolorrect.Height);
            Graphics drawbox = Graphics.FromImage(this.selectedcolorbox);

            Pen outline = new Pen(this.ColorSelectBorder, 2);
            drawbox.DrawRectangle(outline, 0, 0, this.selectcolorrect.Width, this.selectcolorrect.Height);
            outline.Dispose();
            drawbox.Dispose();
        }

        private void DrawGrid()
        {
            this.colorgrid = new Bitmap(this.Width, this.Height);
            SolidBrush tile = null;
            Rectangle tilerect = new Rectangle();
            Graphics drawtiles = Graphics.FromImage(this.colorgrid);
            int currtile = 0;

            // draw all the color tiles
            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    //currtile = (y * 16) + x;
                    tilerect = new Rectangle(x * this.tilesize, y * this.tilesize, this.tilesize, this.tilesize);
                    tile = new SolidBrush(this.fullpalette.Entries[currtile]);
                    drawtiles.FillRectangle(tile, tilerect);
                    currtile++;
                }
            }
            tile.Dispose();
            drawtiles.Dispose();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // PaletteViewer
            // 
            this.Name = "PaletteViewer";
            this.Load += new System.EventHandler(this.PaletteViewer_Load);
            this.ResumeLayout(false);

        }

        private void PaletteViewer_Load(object sender, EventArgs e)
        {

        }

    }
}
