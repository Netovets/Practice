using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace ProductionIS.Controls
{

    public class PuzzleCaptcha : Panel
    {
        private const int Columns    = 2;
        private const int Rows       = 2;
        private const int TileWidth  = 100;
        private const int TileHeight = 100;

        private static readonly string[] ImagePaths =
        {
            @"C:\Users\user\source\repos\ProductionIS\1.png",
            @"C:\Users\user\source\repos\ProductionIS\2.png",
            @"C:\Users\user\source\repos\ProductionIS\3.png",
            @"C:\Users\user\source\repos\ProductionIS\4.png"
        };

        private readonly List<Bitmap> tileImages = new();
        private readonly List<int>    tileOrder  = new();

        private int   dragSourceIndex = -1;
        private Point dragCursorOffset;
        private Point dragCursorPosition;

        public bool IsSolved => tileOrder.SequenceEqual(Enumerable.Range(0, Columns * Rows));

        public PuzzleCaptcha()
        {
            Width        = Columns * TileWidth;
            Height       = Rows    * TileHeight;
            DoubleBuffered = true;
            LoadTileImages();
            Shuffle();
        }

        private void LoadTileImages()
        {
            tileImages.Clear();
            foreach (var path in ImagePaths)
            {
                var source  = new Bitmap(path);
                var resized = new Bitmap(TileWidth, TileHeight);
                using var g = Graphics.FromImage(resized);
                g.DrawImage(source, 0, 0, TileWidth, TileHeight);
                source.Dispose();
                tileImages.Add(resized);
            }
        }

        private void Shuffle()
        {
            var rng = new Random();
            tileOrder.Clear();
            tileOrder.AddRange(Enumerable.Range(0, tileImages.Count).OrderBy(_ => rng.Next()));
            Invalidate();
        }

        private Rectangle GetTileRect(int slotIndex)
        {
            int col = slotIndex % Columns;
            int row = slotIndex / Columns;
            return new Rectangle(col * TileWidth, row * TileHeight, TileWidth, TileHeight);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            for (int slot = 0; slot < tileOrder.Count; slot++)
            {
                if (slot == dragSourceIndex) continue; 

                var rect = GetTileRect(slot);
                e.Graphics.DrawImage(tileImages[tileOrder[slot]], rect);

                using var borderPen = new Pen(Color.FromArgb(100, 0, 0, 0), 1);
                e.Graphics.DrawRectangle(borderPen, rect);
            }

            if (dragSourceIndex >= 0)
            {
                var dest = new Rectangle(
                    dragCursorPosition.X - dragCursorOffset.X,
                    dragCursorPosition.Y - dragCursorOffset.Y,
                    TileWidth, TileHeight);
                e.Graphics.DrawImage(tileImages[tileOrder[dragSourceIndex]], dest);
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            int slot = HitTest(e.Location);
            if (slot < 0) return;

            dragSourceIndex  = slot;
            var tileRect     = GetTileRect(slot);
            dragCursorOffset = new Point(e.X - tileRect.X, e.Y - tileRect.Y);
            dragCursorPosition = e.Location;
            Cursor = Cursors.Hand;
            Invalidate();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (dragSourceIndex < 0) return;
            dragCursorPosition = e.Location;
            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (dragSourceIndex < 0) return;

            int dropSlot = HitTest(e.Location);
            if (dropSlot >= 0 && dropSlot != dragSourceIndex)
            {
                (tileOrder[dragSourceIndex], tileOrder[dropSlot]) =
                    (tileOrder[dropSlot], tileOrder[dragSourceIndex]);
            }

            dragSourceIndex = -1;
            Cursor = Cursors.Default;
            Invalidate();
        }

        private int HitTest(Point point)
        {
            for (int slot = 0; slot < tileOrder.Count; slot++)
                if (GetTileRect(slot).Contains(point)) return slot;
            return -1;
        }

        public void Reset() => Shuffle();
    }
}
