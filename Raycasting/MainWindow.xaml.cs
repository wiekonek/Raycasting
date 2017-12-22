using System;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Raycasting
{
	public partial class MainWindow
	{
		private Timer aTimer;

		public MainWindow()
		{
			InitializeComponent();

			aTimer = new Timer { Interval = 1000/2 };
			aTimer.Start();
			aTimer.Elapsed += OnTimedEvent;

			KeyDown += new KeyEventHandler(OnButtonKeyDown);
		}

		private void OnTimedEvent(Object source, ElapsedEventArgs e)
		{
			Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(DrawScene));
		}

		private void DrawScene()
		{ 
			MainCanvas.Children.Clear();
			for (int x = 0; x < Width; x += MapInfo.AtomSize)
			{
				var cameraX = 2 * x / Width - 1;
				var rayPosition = PlayerInfo.PlayerPosition;
				var rayDirection = (PlayerInfo.PlayerDirection + PlayerInfo.Plane) * cameraX;

				var mapBoxX = (int)PlayerInfo.PlayerPosition.X;
				var mapBoxY = (int)PlayerInfo.PlayerPosition.Y;

				var delta = new Vector()
				{
					X = Math.Sqrt(1 + (rayDirection.Y * rayDirection.Y) / (rayDirection.X * rayDirection.X)),
					Y = Math.Sqrt(1 + (rayDirection.X * rayDirection.X) / (rayDirection.Y * rayDirection.Y)),
				};

				int stepX, stepY;
				var sideDistance = new Vector();

				if (rayDirection.X < 0)
				{
					stepX = -1;
					sideDistance.X = (rayPosition.X - mapBoxX) * delta.X;
				}
				else
				{
					stepX = 1;
					sideDistance.X = (mapBoxX + 1.0 - rayPosition.X) * delta.X;
				}
				if (rayDirection.Y < 0)
				{
					stepY = -1;
					sideDistance.Y = (rayPosition.Y - mapBoxY) * delta.Y;
				}
				else
				{
					stepY = 1;
					sideDistance.Y = (mapBoxY + 1.0 - rayPosition.Y) * delta.Y;
				}

				var hit = false;
				var side = WallSide.NS;
		
				while (!hit)
				{
					if (delta.X < delta.Y)
					{
					sideDistance.X += delta.X;
					mapBoxX += stepX;
					side = WallSide.NS;
					}
					else
					{
					sideDistance.Y += delta.Y;
					mapBoxY += stepY;
					side = WallSide.EW;
					}
					if (MapInfo.Map[mapBoxX, mapBoxY] > 0) hit = true;
				}

				double wallDistance;

				if(side == WallSide.NS)
				{
					wallDistance = (mapBoxX - rayPosition.X + (1 - stepX) / 2) / rayDirection.X;
				}
				else
				{
					wallDistance = (mapBoxY - rayPosition.Y + (1 - stepY) / 2) / rayDirection.Y;
				}

				int lineHeight = (int)(Height / wallDistance);

				int drawStart = -lineHeight / 2 + (int)Height / 2;
				if (drawStart < 0) drawStart = 0;
				int drawEnd = lineHeight / 2 + (int)Height / 2;
				if (drawEnd >= (int)Height) drawEnd = (int)Height - 1;

				DrawHorizontalLine(x, drawStart, drawEnd, MapInfo.WallColors[MapInfo.Map[mapBoxX, mapBoxY]]);
			}
		}

		private void OnButtonKeyDown(object sender, KeyEventArgs e)
		{
			switch (e.Key)
			{
				case Key.Down:
					break;
				case Key.Up:
					PlayerInfo.PlayerDirection.Y++;
					break;
				case Key.Left:
					PlayerInfo.PlayerDirection.X++;
					break;
				case Key.Right:
					PlayerInfo.PlayerDirection.Y++;
					break;
			}
		}

		private void DrawHorizontalLine(int x, int start, int end, Color color)
		{
			for (int h = start; h < end; h+= MapInfo.AtomSize)
			{
				PutPixel(x, h, new SolidColorBrush(color));
			}
		}

		private void PutPixel(int x, int y, Brush color)
		{
			var rec = new Rectangle();
			Canvas.SetTop(rec, y);
			Canvas.SetLeft(rec, x);
			rec.Width = MapInfo.AtomSize;
			rec.Height = MapInfo.AtomSize;
			rec.Fill = color;
			MainCanvas.Children.Add(rec);
		}
	}
}
