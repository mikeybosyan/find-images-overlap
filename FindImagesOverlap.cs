        public class BitmapsOverlap
        {
            public BitmapsOverlap(Bitmap sB, Bitmap tB, Point sP, Point tP)
            {
                sBitmap = sB;
                tBitmap = tB;
                sPoint = sP;
                tPoint = tP;
                size = new Size(1, 1);
            }

            private Bitmap sBitmap;
            private Bitmap tBitmap;
            private Point sPoint;
            private Point tPoint;
            private Size size;

            public bool isSignificant { get { return (size.Height > 5) && (size.Width > 5); } }
            public bool Encloses(Point p) { return (sPoint.X <= p.X) && (p.X < sPoint.X + size.Width) && (sPoint.Y <= p.Y) && (p.Y < sPoint.Y + size.Height); }
            public Rectangle sRectangle { get { return new Rectangle(sPoint, size); } }
            public Rectangle tRectangle { get { return new Rectangle(tPoint, size); } }

            public void Maximise()
            {
                int failCount = 0;
                while (true)
                {
                    failCount = TryExtendLeftEdge() ? 0 : failCount + 1;
                    if (failCount >= 4) break;

                    failCount = TryExtendBottomEdge() ? 0 : failCount + 1;
                    if (failCount >= 4) break;

                    failCount = TryExtendRightEdge() ? 0 : failCount + 1;
                    if (failCount >= 4) break;

                    failCount = TryExtendTopEdge() ? 0 : failCount + 1;
                    if (failCount >= 4) break;
                }
            }

            private bool TryExtendLeftEdge()
            {
                int sLeftEdge = sPoint.X - 1;
                int tLeftEdge = tPoint.X - 1;

                if (sLeftEdge < 0 || tLeftEdge < 0)
                {
                    return false;
                }

                for (int i = 0; i < size.Height; i++)
                {
                    if (sBitmap.GetPixel(sLeftEdge, sPoint.Y + i) != tBitmap.GetPixel(tLeftEdge, tPoint.Y + i))
                    {
                        return false;
                    }
                }

                sPoint.X = sPoint.X - 1;
                tPoint.X = tPoint.X - 1;
                size.Width = size.Width + 1;

                return true;
            }
            private bool TryExtendRightEdge()
            {
                int sRightEdge = sPoint.X + size.Width;
                int tRightEdge = tPoint.X + size.Width;

                if (sRightEdge >= sBitmap.Width || tRightEdge >= tBitmap.Width)
                {
                    return false;
                }

                for (int i = 0; i < size.Height; i++)
                {
                    if (sBitmap.GetPixel(sRightEdge, sPoint.Y + i) != tBitmap.GetPixel(tRightEdge, tPoint.Y + i))
                    {
                        return false;
                    }
                }

                size.Width = size.Width + 1;

                return true;
            }
            private bool TryExtendTopEdge()
            {
                int sTopEdge = sPoint.Y - 1;
                int tTopEdge = tPoint.Y - 1;

                if (sTopEdge < 0 || tTopEdge < 0)
                {
                    return false;
                }

                for (int i = 0; i < size.Width; i++)
                {
                    if (sBitmap.GetPixel(sPoint.X + i, sTopEdge) != tBitmap.GetPixel(tPoint.X + i, tTopEdge))
                    {
                        return false;
                    }
                }

                sPoint.Y = sPoint.Y - 1;
                tPoint.Y = tPoint.Y - 1;
                size.Height = size.Height + 1;

                return true;
            }
            private bool TryExtendBottomEdge()
            {
                int sBottomEdge = sPoint.Y + size.Height;
                int tBottomEdge = tPoint.Y + size.Height;

                if (sBottomEdge >= sBitmap.Height || tBottomEdge >= tBitmap.Height)
                {
                    return false;
                }

                for (int i = 0; i < size.Width; i++)
                {
                    if (sBitmap.GetPixel(sPoint.X + i, sBottomEdge) != tBitmap.GetPixel(tPoint.X + i, tBottomEdge))
                    {
                        return false;
                    }
                }

                size.Height = size.Height + 1;

                return true;
            }

            public override string ToString()
            {
                return string.Format("S = ({0}, {1}), T = ({2}, {3}), size = ({4}, {5})", sPoint.X, sPoint.Y, tPoint.X, tPoint.Y, size.Width, size.Height);
            }
        }

        private IEnumerable<BitmapsOverlap> FindBitmapsOverlap(Bitmap S, Bitmap T)
        {
            List<BitmapsOverlap> significantOverlaps = new List<BitmapsOverlap>();

            ILookup<Color, Point> tPointsByColour = ExtractBitmapPixels(T).ToLookup(x => x.Item2, x => x.Item1);

            IEnumerable<Point> sPoints = GetBitmapTestPoints(S);
            foreach (Point sPoint in sPoints)
            {
                if (significantOverlaps.Any(x => x.Encloses(sPoint)))
                    continue;

                Color sPointColour = S.GetPixel(sPoint.X, sPoint.Y);

                IEnumerable<Point> tPoints = tPointsByColour[sPointColour];
                foreach (Point tPoint in tPoints)
                {
                    BitmapsOverlap overlap = new BitmapsOverlap(S, T, sPoint, tPoint);

                    overlap.Maximise();

                    if (overlap.isSignificant)
                    {
                        significantOverlaps.Add(overlap);
                        break;
                    }
                }
            }

            return significantOverlaps;
        }

        private static IEnumerable<Point> GetBitmapTestPoints(Bitmap b)
        {
            int shortestSide = Math.Min(b.Width, b.Height);
            int step = shortestSide / 20;

            for (int x = (step / 2); x < b.Width; x += step)
            {
                for (int y = (step / 2); y < b.Height; y += step)
                {
                    yield return new Point(x, y);
                }
            }
        }

        private static IEnumerable<Tuple<Point, Color>> ExtractBitmapPixels(Bitmap b)
        {
            for (int x = 0; x < b.Width; x++)
            {
                for (int y = 0; y < b.Height; y++)
                {
                    yield return new Tuple<Point, Color>(new Point(x, y), b.GetPixel(x, y));
                }
            }
        }
